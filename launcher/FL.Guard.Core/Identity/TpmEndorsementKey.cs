using System;
using System.Runtime.InteropServices;

namespace FL.Guard.Core.Identity
{
    /// <summary>
    /// Lê a chave pública de endosso (EK) do TPM via CNG (Microsoft Platform Crypto Provider).
    /// A EK é gravada de fábrica no chip: não muda ao formatar, trocar disco, MAC ou IP.
    /// </summary>
    internal static class TpmEndorsementKey
    {
        private const string ProviderName = "Microsoft Platform Crypto Provider";
        private const string EkPubProperty = "PCP_EKPUB";
        private const int ErrorSuccess = 0;

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern int NCryptOpenStorageProvider(out IntPtr phProvider, string pszProviderName, uint dwFlags);

        [DllImport("ncrypt.dll", CharSet = CharSet.Unicode)]
        private static extern int NCryptGetProperty(IntPtr hObject, string pszProperty, byte[] pbOutput, int cbOutput, out int pcbResult, uint dwFlags);

        [DllImport("ncrypt.dll")]
        private static extern int NCryptFreeObject(IntPtr hObject);

        /// <summary>Retorna os bytes da EK pública, ou null se não houver TPM / provider indisponível.</summary>
        public static byte[] TryRead()
        {
            IntPtr provider = IntPtr.Zero;
            try
            {
                int hr = NCryptOpenStorageProvider(out provider, ProviderName, 0);
                if (hr != ErrorSuccess || provider == IntPtr.Zero)
                    return null;

                int size;
                hr = NCryptGetProperty(provider, EkPubProperty, null, 0, out size, 0);
                if (hr != ErrorSuccess || size <= 0 || size > 64 * 1024)
                    return null;

                var buffer = new byte[size];
                hr = NCryptGetProperty(provider, EkPubProperty, buffer, buffer.Length, out size, 0);
                if (hr != ErrorSuccess || size <= 0)
                    return null;

                if (size != buffer.Length)
                    Array.Resize(ref buffer, size);
                return buffer;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (provider != IntPtr.Zero)
                {
                    try { NCryptFreeObject(provider); } catch { }
                }
            }
        }
    }
}
