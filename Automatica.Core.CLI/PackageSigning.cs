using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Automatica.Core.CLI.Data;
using Newtonsoft.Json;

namespace Automatica.Core.CLI
{
    /// <summary>
    /// Signs plugin (*.acpkg) and update (*.zip) packages: automatica-signature.json in the zip root holds the SHA-256 of every
    /// other entry and an ECDSA P-256 signature over the canonical payload. The payload must stay byte identical to
    /// Automatica.Core.Common.Update.Signing.PackageSignatureFormat in the core repository.
    ///
    /// The private key (PKCS#8 or SEC1 PEM) comes from AUTOMATICA_PACKAGE_SIGNING_KEY (the PEM text, or its base64 DER) or
    /// AUTOMATICA_PACKAGE_SIGNING_KEY_FILE (path to the PEM file). It is never committed.
    /// </summary>
    internal static class PackageSigning
    {
        public const string KeyEnv = "AUTOMATICA_PACKAGE_SIGNING_KEY";
        public const string KeyFileEnv = "AUTOMATICA_PACKAGE_SIGNING_KEY_FILE";

        /// <summary>
        /// "true": pack/deploy fail if a package cannot be signed (no key)
        /// </summary>
        public const string RequireEnv = "AUTOMATICA_REQUIRE_PACKAGE_SIGNATURE";

        public const string SignatureFileName = "automatica-signature.json";
        public const string TypePlugin = "plugin";
        public const string TypeUpdate = "update";
        public const string UpdateName = "Automatica.Core";
        private const string PayloadHeader = "AUTOMATICA-PACKAGE-SIGNATURE-V1";
        private const string Algorithm = "ECDSA-P256-SHA256";

        private class SignatureFile
        {
            [JsonProperty("formatVersion")] public int FormatVersion { get; set; } = 1;
            [JsonProperty("algorithm")] public string Algorithm { get; set; } = PackageSigning.Algorithm;
            [JsonProperty("keyId")] public string KeyId { get; set; }
            [JsonProperty("packageType")] public string PackageType { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("version")] public string Version { get; set; }
            [JsonProperty("rid")] public string Rid { get; set; }
            [JsonProperty("files")] public List<SignedFile> Files { get; set; } = new List<SignedFile>();
            [JsonProperty("signature")] public string Signature { get; set; }
        }

        private class SignedFile
        {
            [JsonProperty("path")] public string Path { get; set; }
            [JsonProperty("sha256")] public string Sha256 { get; set; }
        }

        public static bool SignatureRequired
        {
            get
            {
                var value = Environment.GetEnvironmentVariable(RequireEnv);
                return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1";
            }
        }

        /// <returns>the signing key, null if none is configured</returns>
        public static ECDsa LoadKey()
        {
            var keyText = Environment.GetEnvironmentVariable(KeyEnv);
            var keyFile = Environment.GetEnvironmentVariable(KeyFileEnv);

            if (string.IsNullOrWhiteSpace(keyText) && !string.IsNullOrWhiteSpace(keyFile))
            {
                keyText = File.ReadAllText(keyFile);
            }

            if (string.IsNullOrWhiteSpace(keyText))
            {
                return null;
            }

            var key = ECDsa.Create();
            keyText = keyText.Trim();
            if (keyText.Contains("-----BEGIN"))
            {
                key.ImportFromPem(keyText);
            }
            else
            {
                var der = Convert.FromBase64String(keyText);
                try
                {
                    key.ImportPkcs8PrivateKey(der, out _);
                }
                catch (CryptographicException)
                {
                    key.ImportECPrivateKey(der, out _);
                }
            }

            if (key.KeySize != 256)
            {
                throw new CryptographicException("The package signing key must be an ECDSA P-256 key");
            }

            return key;
        }

        /// <summary>
        /// Signs the package if a key is configured. Without a key: warning, or error if <see cref="RequireEnv"/> is set.
        /// </summary>
        /// <returns>0 on success (signed, or unsigned and allowed), otherwise an error code</returns>
        public static int SignIfConfigured(string zipFile, string packageType)
        {
            ECDsa key;
            try
            {
                key = LoadKey();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Could not load the package signing key: {e.Message}");
                return -10;
            }

            if (key == null)
            {
                if (IsSigned(zipFile))
                {
                    Console.WriteLine($"{zipFile} is already signed");
                    return 0;
                }

                if (SignatureRequired)
                {
                    Console.Error.WriteLine($"No package signing key ({KeyEnv} / {KeyFileEnv}) but {RequireEnv} is set, {zipFile} is not signed");
                    return -11;
                }

                Console.WriteLine($"##[warning]No package signing key ({KeyEnv} / {KeyFileEnv}), {zipFile} is NOT signed. Cores with mandatory package verification will reject it.");
                return 0;
            }

            using (key)
            {
                try
                {
                    var signature = packageType == TypePlugin ? SignPlugin(zipFile, key) : SignUpdate(zipFile, key);
                    Console.WriteLine($"Signed {zipFile} ({signature.Name} {signature.Version}, {signature.Files.Count} files, key {signature.KeyId})");
                    return 0;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Could not sign {zipFile}: {e}");
                    return -12;
                }
            }
        }

        public static bool IsSigned(string zipFile)
        {
            using var archive = ZipFile.OpenRead(zipFile);
            return archive.Entries.Any(a => IsSignatureEntry(Normalize(a.FullName)));
        }

        private static SignatureFile SignPlugin(string zipFile, ECDsa key)
        {
            PluginManifest manifest;
            using (var archive = ZipFile.OpenRead(zipFile))
            {
                var entry = archive.GetEntry("automatica-manifest.json") ?? throw new InvalidDataException("automatica-manifest.json not found");
                using var reader = new StreamReader(entry.Open());
                manifest = JsonConvert.DeserializeObject<PluginManifest>(reader.ReadToEnd());
            }

            var version = manifest?.Automatica?.PluginVersion?.ToString() ?? throw new InvalidDataException("The plugin manifest has no plugin version");
            return Sign(zipFile, key, TypePlugin, manifest.Automatica.ComponentName, version, string.Empty);
        }

        private static SignatureFile SignUpdate(string zipFile, ECDsa key)
        {
            UpdateManifest manifest;
            using (var archive = ZipFile.OpenRead(zipFile))
            {
                var entry = archive.GetEntry("automatica-update.manifest") ?? throw new InvalidDataException("automatica-update.manifest not found");
                using var reader = new StreamReader(entry.Open());
                manifest = JsonConvert.DeserializeObject<UpdateManifest>(reader.ReadToEnd());
            }

            var version = manifest?.Version?.ToString() ?? throw new InvalidDataException("The update manifest has no version");
            return Sign(zipFile, key, TypeUpdate, UpdateName, version, manifest.Rid ?? string.Empty);
        }

        private static SignatureFile Sign(string zipFile, ECDsa key, string packageType, string name, string version, string rid)
        {
            using var archive = ZipFile.Open(zipFile, ZipArchiveMode.Update);

            foreach (var existing in archive.Entries.Where(a => IsSignatureEntry(Normalize(a.FullName))).ToList())
            {
                existing.Delete();
            }

            var files = new List<SignedFile>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var sha = SHA256.Create())
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                    {
                        continue;
                    }

                    var path = Normalize(entry.FullName);
                    if (!seen.Add(path))
                    {
                        throw new InvalidDataException($"Duplicate zip entry {path}");
                    }

                    using var stream = entry.Open();
                    files.Add(new SignedFile { Path = path, Sha256 = ToHex(sha.ComputeHash(stream)) });
                }
            }

            var signature = new SignatureFile
            {
                KeyId = ToHex(SHA256.HashData(key.ExportSubjectPublicKeyInfo())).Substring(0, 16),
                PackageType = packageType,
                Name = name,
                Version = version,
                Rid = rid,
                Files = files.OrderBy(a => a.Path, StringComparer.Ordinal).ToList()
            };

            var payload = BuildPayload(signature);
            signature.Signature = Convert.ToBase64String(key.SignData(payload, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation));

            var signatureEntry = archive.CreateEntry(SignatureFileName, CompressionLevel.Optimal);
            using (var writer = new StreamWriter(signatureEntry.Open()))
            {
                writer.Write(JsonConvert.SerializeObject(signature, Formatting.Indented));
            }

            return signature;
        }

        /// <summary>
        /// Canonical payload, identical to PackageSignatureFormat.BuildPayload in the core
        /// </summary>
        private static byte[] BuildPayload(SignatureFile signature)
        {
            var sb = new StringBuilder();
            void Line(string value)
            {
                if (value.Contains('\n') || value.Contains('\r'))
                {
                    throw new InvalidDataException("Line breaks are not allowed in signed values");
                }
                sb.Append(value).Append('\n');
            }

            var sorted = signature.Files.OrderBy(a => a.Path, StringComparer.Ordinal).ToList();
            Line(PayloadHeader);
            Line($"type={signature.PackageType}");
            Line($"name={signature.Name}");
            Line($"version={signature.Version}");
            Line($"rid={signature.Rid ?? string.Empty}");
            Line($"files={sorted.Count}");
            foreach (var file in sorted)
            {
                Line($"{file.Sha256.ToLowerInvariant()} {file.Path}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        /// <summary>
        /// Generates a new P-256 key pair: the private key (PKCS#8 PEM) is written to <paramref name="privateKeyFile"/>,
        /// the public key (SubjectPublicKeyInfo) is printed for TrustedPackageKeys in the core
        /// </summary>
        public static int GenerateKey(string privateKeyFile)
        {
            if (File.Exists(privateKeyFile))
            {
                Console.Error.WriteLine($"{privateKeyFile} already exists");
                return -1;
            }

            using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var directory = Path.GetDirectoryName(Path.GetFullPath(privateKeyFile));
            Directory.CreateDirectory(directory!);
            File.WriteAllText(privateKeyFile, key.ExportPkcs8PrivateKeyPem());
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(privateKeyFile, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }

            Console.WriteLine($"Private key written to {privateKeyFile} (keep it secret, store it as the CI secret {KeyEnv})");
            Console.WriteLine("Public key for TrustedPackageKeys.PublicKeys:");
            Console.WriteLine(Convert.ToBase64String(key.ExportSubjectPublicKeyInfo()));
            return 0;
        }

        private static string Normalize(string path) => path.Replace('\\', '/').TrimStart('/');

        private static bool IsSignatureEntry(string path) => string.Equals(path, SignatureFileName, StringComparison.OrdinalIgnoreCase);

        private static string ToHex(byte[] data) => Convert.ToHexString(data).ToLowerInvariant();
    }
}
