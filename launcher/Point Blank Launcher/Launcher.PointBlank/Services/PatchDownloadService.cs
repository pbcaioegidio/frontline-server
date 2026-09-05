using Launcher.PointBlank.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Launcher.PointBlank.Services
{
    public class PatchDownloadService
    {
        private readonly string _clientPath;
        private readonly LauncherConnection _connection;

        public PatchDownloadService(string clientPath, LauncherConnection connection)
        {
            _clientPath = clientPath;
            _connection = connection;
        }
        public async Task DownloadFilesAsync(
            List<UpdateFile> filesToUpdate,
            IProgress<UpdateDownloadProgress> progress = null)
        {
            string downloadFolder = Path.Combine(_clientPath, "_DownloadPatchFiles");
            Directory.CreateDirectory(downloadFolder);
            int totalFiles = filesToUpdate.Count;
            for (int i = 0; i < totalFiles; i++)
            {
                UpdateFile file = filesToUpdate[i];
                bool hasZip = !string.IsNullOrEmpty(file.Zip);
                string remotePath = hasZip ? file.Zip : file.Path;
                string downloadedFile = Path.Combine(downloadFolder, Path.GetFileName(remotePath));

                progress?.Report(new UpdateDownloadProgress
                {
                    CurrentFile = file.Path,
                    BytesReceived = 0,
                    TotalBytes = file.Size,
                    CurrentFileIndex = i + 1,
                    TotalFiles = totalFiles
                });
                await _connection.DownloadFileAsync(
                    remotePath,
                    downloadedFile,
                    (received, total) => progress?.Report(new UpdateDownloadProgress
                    {
                        CurrentFile = file.Path,
                        BytesReceived = received,
                        TotalBytes = total,
                        CurrentFileIndex = i + 1,
                        TotalFiles = totalFiles
                    })
                );
                string destPath = Path.Combine(_clientPath, file.Path.TrimStart('/', '\\'));
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                if (hasZip)
                {
                    string destDir = Path.GetDirectoryName(
                        Path.Combine(_clientPath, file.Path.TrimStart('/', '\\'))
                    );
                    Directory.CreateDirectory(destDir);

                    using (ZipArchive archive = ZipFile.OpenRead(downloadedFile))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (string.IsNullOrEmpty(entry.Name))
                                continue;

                            string entryDest = Path.Combine(destDir, entry.Name);
                            entry.ExtractToFile(entryDest, overwrite: true);
                        }
                    }

                    File.Delete(downloadedFile);
                }
                else
                {
                    if (File.Exists(destPath))
                        File.Delete(destPath);
                    File.Move(downloadedFile, destPath);
                }
            }
            if (Directory.Exists(downloadFolder) &&
                Directory.GetFiles(downloadFolder, "*", SearchOption.AllDirectories).Length == 0)
                Directory.Delete(downloadFolder, recursive: true);
        }
    }
}
