using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Azure;
using Azure.Security.KeyVault.Certificates;

namespace KeyVaultLib
{
    public class KeyVaultHelper
    {
        private readonly string _vaultName;
        private const int CacheHours = 2;

        public KeyVaultHelper(string vaultName)
        {
            if (string.IsNullOrWhiteSpace(vaultName)) throw new ArgumentException("vaultName is required");
            _vaultName = vaultName;
        }


        /// <summary>
        /// Get a list of all the vault's secrets
        /// </summary>
        /// <returns>A List of the secrets</returns>
        public List<SecretProperties> GetSecretsList()
        {
            var keyVaultClient = GetSecretClient();
            var secretList = new List<SecretProperties>();
            var secrets = keyVaultClient.GetPropertiesOfSecrets();
            foreach (var secret in secrets)
            {
                secretList.Add(secret);
            }
            return secretList;
        }

        /// <summary>
        /// Sets a list of secrets 
        /// </summary>
        /// <returns></returns>
        public async Task SetSecretsAsync(List<KeyVaultSecret> secrets)
        {
            var keyVaultClient = GetSecretClient();
            foreach (var secret in secrets)
            {
                await keyVaultClient.SetSecretAsync(secret).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets a secret from an Azure key vault
        /// Caches the secret in memory for 2 hours 
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns>The secret value or string.Empty if it does not exist</returns>
        public async Task<string> GetSecretValueAsync(string secretName)
        {
            try
            {
                var keyVaultClient = GetSecretClient();
                var key = $"SECRET{secretName}";
                var secret = await CacheAsideHelper.GetOrAdd(async () => await keyVaultClient.GetSecretAsync(secretName).ConfigureAwait(false), new TimeSpan(CacheHours, 0, 0), key);
                return secret?.Value?.Value ?? string.Empty;
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode == "SecretNotFound") return string.Empty;    // secret does not exist - return blank
                throw;
            }
        }

        /// <summary>
        /// Gets a certificate from an Azure key vault
        /// Caches the certficate in memory for 2 hours 
        /// </summary>
        /// <param name="certName"></param>
        /// <returns>The certificate bundle or null if it does not exist</returns>
        public async Task<KeyVaultCertificateWithPolicy> GetCertificateValueAsync(string certName)
        {
            try
            {
                var keyVaultClient = GetCertificateClient();
                var key = $"CERT{certName}";
                var cert = await CacheAsideHelper.GetOrAdd(async () => await keyVaultClient.GetCertificateAsync(certName).ConfigureAwait(false), new TimeSpan(CacheHours, 0, 0), key);
                return cert;
            }
            catch (RequestFailedException ex)
            {
                if (ex.ErrorCode == "CertificateNotFound") return null;
                throw;
            }
        }

        private SecretClient GetSecretClient()
        {
            var secretClient = new SecretClient(vaultUri: new Uri(_vaultName), credential: new DefaultAzureCredential());
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            return secretClient;
        }

        private CertificateClient GetCertificateClient()
        {
            var certificateClient = new CertificateClient(vaultUri: new Uri(_vaultName), credential: new DefaultAzureCredential());
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            return certificateClient;
        }
    }
}
