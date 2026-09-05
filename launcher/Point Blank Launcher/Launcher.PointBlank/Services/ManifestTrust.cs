using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Launcher.PointBlank.Services
{
    /// <summary>
    /// Assinatura RSA do UserFileList.dat — a chave privada só fica na máquina de build.
    /// </summary>
    public static class ManifestTrust
    {
        public const string PublicPem =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCgKCAQEAnBfD0FQI9JJDqUvkpt09bfKrdsQJQZNn42fwyGfkDV0B5Mn4rdEZ
gjOJiHVat2S3AoZnKEVImc3oAu/ri7Vouez+MRikzUyrb2bsqboBKftbfLErsDas
40Zeb7qSIVymTn6J3chSBi/XoUbDFWwdKDqUSVmF9+8meqtXThZBqdtQJsXIU7Vd
xIikXCtgHqYRL4FPFiCEF5vJwV0QSAbIfNphSrvbhI1LgheCSjrAt9zKNKEpNnYk
7QCfr+NVF+Ee2dxP3MvNHcakhd6AvajKV6PKXvYo8jwSnnsSP3g8HzOo8Un/w0OG
HoBw4THLPHVu6BvxSyUdK5cgBKDBBgMlVQIDAQAB
-----END RSA PUBLIC KEY-----";

        public static bool VerifyFile(string datPath, string sigPath, out string error)
        {
            error = null;
            if (!File.Exists(datPath))
            {
                error = "UserFileList.dat ausente.";
                return false;
            }
            if (!File.Exists(sigPath))
            {
                error = "Assinatura da lista ausente (UserFileList.sig).";
                return false;
            }

            try
            {
                byte[] data = File.ReadAllBytes(datPath);
                byte[] sig = Convert.FromBase64String(File.ReadAllText(sigPath).Trim());
                using RSA rsa = RSA.Create();
                rsa.ImportFromPem(PublicPem);
                byte[] hash = SHA256.HashData(data);
                if (rsa.VerifyHash(hash, sig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                    return true;
                error = "Lista de arquivos adulterada (assinatura inválida).";
                return false;
            }
            catch (Exception ex)
            {
                error = "Falha ao validar a lista: " + ex.Message;
                return false;
            }
        }

        public static void SignFile(string datPath, string sigPath, string privatePem)
        {
            byte[] data = File.ReadAllBytes(datPath);
            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(privatePem);
            byte[] hash = SHA256.HashData(data);
            byte[] sig = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            File.WriteAllText(sigPath, Convert.ToBase64String(sig), Encoding.ASCII);
        }
    }
}
