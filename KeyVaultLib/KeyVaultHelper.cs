using Microsoft.Azure.KeyVault;
using System;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault.Models;
using System.Threading.Tasks;

namespace KeyVaultLib
{
    public static class KeyVaultHelper  
    {
        // TODO - change url to match Azure keyvault name
        private const string VaultName = "https://mykeyvault.vault.azure.net/";
        private const int CacheHours = 2;

        /// <summary>
        /// Gets a secret from an Azure key vault
        /// Caches the secret in memory for 2 hours 
        /// </summary>
        /// <param name="secretName"></param>
        /// <returns>The secret value or string.Empty if it does not exist</returns>
        public static async Task<string> GetSecretValueAsync(string secretName)
        {
            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var key = $"SECRET{secretName}";
                var secret = await CacheAsideHelper.GetOrAddAsync(async () => await keyVaultClient.GetSecretAsync(VaultName, secretName).ConfigureAwait(false), new TimeSpan(CacheHours, 0, 0), key);
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
        public static async Task<CertificateBundle> GetCertificateValueAsync(string certName)
        {
            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var key = $"CERT{certName}";
                var cert = await CacheAsideHelper.GetOrAddAsync(async () => await keyVaultClient.GetCertificateAsync(VaultName, certName).ConfigureAwait(false), new TimeSpan(CacheHours, 0, 0), key);
                return cert;
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Body.Error.Code == "CertificateNotFound") return null;    
                throw;
            }
        }
    }
}
