using Microsoft.Azure.KeyVault;
using System;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;

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
        public async Task<List<SecretItem>> GetSecretsListAsync()
        {
            var keyVaultClient = GetClient();
            var secretList = new List<SecretItem>();
            var secrets = await keyVaultClient.GetSecretsAsync(_vaultName).ConfigureAwait(false);
            while (secrets != null)
            {
                foreach (var secret in secrets)
                {
                    secretList.Add(secret);
                }
                if (secrets.NextPageLink == null) break;
                secrets = await keyVaultClient.GetSecretsNextAsync(secrets.NextPageLink).ConfigureAwait(false);
            }
            return secretList;
        }


        /// <summary>
        /// Adds a list of secrets from the Vault to a new Vault
        /// </summary>
        /// <returns></returns>
        public async Task SetSecretsAsync(List<SecretItem> secrets, string newVault)
        {
            var keyVaultClient = GetClient();
            foreach (var secret in secrets)
            {
                var secretName = secret.Identifier.Name;
                var secretBundle = await keyVaultClient.GetSecretAsync(_vaultName, secretName).ConfigureAwait(false);
                if (secretBundle?.Value != null)
                    await keyVaultClient.SetSecretAsync(newVault, secretName, secretBundle.Value).ConfigureAwait(false);
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
                var keyVaultClient = GetClient();
                var key = $"SECRET{secretName}";
                var secret = await CacheAsideHelper.GetOrAddAsync(async () => await keyVaultClient.GetSecretAsync(_vaultName, secretName).ConfigureAwait(false), new TimeSpan(CacheHours, 0, 0), key);
                return secret?.Value ?? string.Empty;
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Body.Error.Code == "SecretNotFound") return string.Empty;    // secret does not exist - return blank
                throw;
            }
        }

        /// <summary>
        /// Gets a certificate from an Azure key vault
        /// Caches the certficate in memory for 2 hours 
        /// </summary>
        /// <param name="certName"></param>
        /// <returns>The certificate bundle or null if it does not exist</returns>
        public async Task<CertificateBundle> GetCertificateValueAsync(string certName)
        {
            try
            {
                var keyVaultClient = GetClient();
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var key = $"CERT{certName}";
                var cert = await CacheAsideHelper.GetOrAddAsync(async () => await keyVaultClient.GetCertificateAsync(_vaultName, certName).ConfigureAwait(false), new TimeSpan(CacheHours, 0, 0), key);
                return cert;
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Body.Error.Code == "CertificateNotFound") return null;
                throw;
            }
        }

        private static KeyVaultClient GetClient()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            return keyVaultClient;
        }
    }
}
