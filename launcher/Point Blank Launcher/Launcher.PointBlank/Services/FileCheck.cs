using Launcher.PointBlank.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Launcher.PointBlank.Services
{
    public class FileCheck
    {
        private const int MaxBarWidth = 463;
        private readonly string _startupPath;

        public FileCheck(string startupPath)
        {
            _startupPath = startupPath;
        }

        public async Task<FileCheckResult> CheckFilesAsync(
            Dictionary<string, string> userFiles,
            IProgress<FileCheckProgress> progress = null)
        {
            FileCheckResult result = new FileCheckResult();

            string datPath = Path.Combine(_startupPath, "UserFileList.dat");
            string sigPath = Path.Combine(_startupPath, "UserFileList.sig");
            if (!ManifestTrust.VerifyFile(datPath, sigPath, out string trustError))
            {
                result.Success = false;
                result.Message = trustError;
                return result;
            }

            if (userFiles == null || userFiles.Count == 0)
            {
                result.Success = false;
                result.Message = "Lista de arquivos vazia.";
                return result;
            }

            int totalFiles = userFiles.Count;
            int done = 0;
            var invalid = new ConcurrentBag<string>();

            await Task.Run(() =>
            {
                Parallel.ForEach(userFiles, new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(2, Environment.ProcessorCount)
                }, item =>
                {
                    string relativePath = IntegrityRules.NormalizeLocal(item.Key);
                    if (IntegrityRules.ShouldSkip(relativePath))
                    {
                        Interlocked.Increment(ref done);
                        return;
                    }
                    string expectedHash = item.Value;
                    string localPath = Path.Combine(_startupPath, relativePath);

                    if (!File.Exists(localPath) || !HashesMatch(localPath, expectedHash))
                    {
                        invalid.Add(relativePath);
                        Logger.LogFileCheckException(Path.GetFileName(relativePath));
                    }

                    int n = Interlocked.Increment(ref done);
                    if (n == totalFiles || n % 8 == 0)
                    {
                        int bar = (int)(MaxBarWidth * (double)n / totalFiles);
                        progress?.Report(new FileCheckProgress
                        {
                            CurrentFile = relativePath,
                            CurrentIndex = n,
                            TotalFiles = totalFiles,
                            FileBarWidth = bar,
                            TotalBarWidth = bar,
                            StatusMessage = "FL Guard verificando..."
                        });
                    }
                });
            }).ConfigureAwait(true);

            // Extra fora da lista (ex.: sujeira do instalador) — remove e segue.
            // Hash errado / faltando continua bloqueando (precisa Update).
            var removedExtras = new List<string>();
            foreach (string extra in FindProtectedExtras(userFiles.Keys))
            {
                string full = Path.Combine(_startupPath, extra);
                try
                {
                    if (File.Exists(full))
                    {
                        File.SetAttributes(full, FileAttributes.Normal);
                        File.Delete(full);
                        removedExtras.Add(extra);
                        Logger.Log($"FL Guard removeu extra: {extra}");
                    }
                }
                catch (Exception ex)
                {
                    invalid.Add(extra);
                    Logger.LogFileCheckException("extra:" + extra + " (" + ex.Message + ")");
                }
            }

            result.InvalidFiles = invalid.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList();
            result.Success = result.InvalidFiles.Count == 0;
            if (result.Success)
            {
                result.Message = removedExtras.Count > 0
                    ? $"FL Guard: removeu {removedExtras.Count} arquivo(s) extra. Você já pode jogar."
                    : "FL Guard: arquivos íntegros. Você já pode jogar.";
            }
            else
            {
                string sample = string.Join("\n", result.InvalidFiles.Take(8));
                int n = result.InvalidFiles.Count;
                result.Message = n == 1
                    ? $"FL Guard bloqueou 1 arquivo alterado ou extra.\n{sample}"
                    : $"FL Guard bloqueou {n} arquivos alterados ou extras.\n{sample}";
            }
            return result;
        }

        private IEnumerable<string> FindProtectedExtras(IEnumerable<string> listed)
        {
            var known = new HashSet<string>(listed.Select(IntegrityRules.NormalizeLocal), StringComparer.OrdinalIgnoreCase);
            var extras = new List<string>();
            foreach (string full in Directory.EnumerateFiles(_startupPath, "*", SearchOption.AllDirectories))
            {
                string rel = IntegrityRules.NormalizeLocal(Path.GetRelativePath(_startupPath, full));
                if (known.Contains(rel) || IntegrityRules.ShouldSkip(rel))
                    continue;
                if (IntegrityRules.IsProtectedExtra(rel))
                    extras.Add(rel);
            }
            return extras;
        }

        private static bool HashesMatch(string path, string expectedHash)
        {
            try
            {
                string local = HashFile(path, expectedHash);
                return string.Equals(local, expectedHash, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        internal static string HashFile(string path, string expectedHash)
        {
            using HashAlgorithm algo = CreateHashAlgorithm(expectedHash);
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
            byte[] hash = algo.ComputeHash(stream);
            return Convert.ToHexString(hash);
        }

        private static HashAlgorithm CreateHashAlgorithm(string expectedHash)
        {
            if (string.IsNullOrWhiteSpace(expectedHash))
                throw new InvalidOperationException("Hash vazio na lista.");
            switch (expectedHash.Trim().Length)
            {
                case 32: return MD5.Create();
                case 64: return SHA256.Create();
                case 128: return SHA512.Create();
                default: throw new InvalidOperationException($"Formato de hash não suportado ({expectedHash.Length} caracteres).");
            }
        }
    }
}
