using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FL.Guard.Core.Identity
{
    /// <summary>
    /// Identificadores da máquina já em forma de hash (SHA-256 hex).
    /// Nenhum valor bruto (serial, UUID) sai do PC do jogador — o servidor só precisa comparar igualdade.
    /// </summary>
    public sealed class HardwareComponents
    {
        /// <summary>Hash composto: motherboard|bios_uuid|cpu|disk|ram|tpm.</summary>
        [JsonPropertyName("fingerprint")]
        public string Fingerprint { get; set; } = "";

        [JsonPropertyName("motherboard")]
        public string Motherboard { get; set; } = "";

        [JsonPropertyName("bios_uuid")]
        public string BiosUuid { get; set; } = "";

        [JsonPropertyName("cpu")]
        public string Cpu { get; set; } = "";

        [JsonPropertyName("disk")]
        public string Disk { get; set; } = "";

        [JsonPropertyName("ram")]
        public string Ram { get; set; } = "";

        /// <summary>SHA-256 da chave pública de endosso (EK) do TPM. Vazio se não houver TPM acessível.</summary>
        [JsonPropertyName("tpm")]
        public string Tpm { get; set; } = "";

        [JsonPropertyName("gpu")]
        public string Gpu { get; set; } = "";

        [JsonPropertyName("machine_guid")]
        public string MachineGuid { get; set; } = "";

        /// <summary>MACs físicos (hash individual de cada um), ordenados.</summary>
        [JsonPropertyName("macs")]
        public List<string> Macs { get; set; } = new List<string>();

        /// <summary>Quantos componentes fortes (motherboard, bios_uuid, disk, ram, tpm) foram coletados. 0–5.</summary>
        [JsonPropertyName("strength")]
        public int Strength { get; set; }

        [JsonPropertyName("tpm_present")]
        public bool TpmPresent { get; set; }

        /// <summary>Quais coletas falharam/estouraram tempo (diagnóstico; nomes dos componentes).</summary>
        [JsonPropertyName("missing")]
        public List<string> Missing { get; set; } = new List<string>();

        [JsonPropertyName("collected_at")]
        public DateTime CollectedAtUtc { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

        public static HardwareComponents FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new HardwareComponents();
            try
            {
                return JsonSerializer.Deserialize<HardwareComponents>(json, JsonOptions) ?? new HardwareComponents();
            }
            catch
            {
                return new HardwareComponents();
            }
        }

        /// <summary>Pares (kind, hash) para gravar/consultar em ban_identifiers.</summary>
        public IEnumerable<KeyValuePair<string, string>> EnumerateIdentifiers()
        {
            if (!string.IsNullOrEmpty(Fingerprint)) yield return new KeyValuePair<string, string>("fingerprint", Fingerprint);
            if (!string.IsNullOrEmpty(Motherboard)) yield return new KeyValuePair<string, string>("motherboard", Motherboard);
            if (!string.IsNullOrEmpty(BiosUuid)) yield return new KeyValuePair<string, string>("bios_uuid", BiosUuid);
            if (!string.IsNullOrEmpty(Cpu)) yield return new KeyValuePair<string, string>("cpu", Cpu);
            if (!string.IsNullOrEmpty(Disk)) yield return new KeyValuePair<string, string>("disk", Disk);
            if (!string.IsNullOrEmpty(Ram)) yield return new KeyValuePair<string, string>("ram", Ram);
            if (!string.IsNullOrEmpty(Tpm)) yield return new KeyValuePair<string, string>("tpm", Tpm);
            if (!string.IsNullOrEmpty(Gpu)) yield return new KeyValuePair<string, string>("gpu", Gpu);
            if (!string.IsNullOrEmpty(MachineGuid)) yield return new KeyValuePair<string, string>("machine_guid", MachineGuid);
            foreach (string mac in Macs)
                if (!string.IsNullOrEmpty(mac)) yield return new KeyValuePair<string, string>("mac", mac);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("fingerprint=").Append(Short(Fingerprint));
            sb.Append(" strength=").Append(Strength).Append("/5");
            sb.Append(" tpm=").Append(TpmPresent ? "yes" : "no");
            sb.Append(" mb=").Append(Short(Motherboard));
            sb.Append(" bios=").Append(Short(BiosUuid));
            sb.Append(" cpu=").Append(Short(Cpu));
            sb.Append(" disk=").Append(Short(Disk));
            sb.Append(" ram=").Append(Short(Ram));
            sb.Append(" gpu=").Append(Short(Gpu));
            sb.Append(" guid=").Append(Short(MachineGuid));
            sb.Append(" macs=").Append(Macs.Count);
            if (Missing.Count > 0)
                sb.Append(" missing=").Append(string.Join(",", Missing));
            return sb.ToString();
        }

        private static string Short(string h) => string.IsNullOrEmpty(h) ? "-" : h.Substring(0, Math.Min(10, h.Length));
    }
}
