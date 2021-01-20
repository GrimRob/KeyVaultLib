using Microsoft.Azure.KeyVault;
using System;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault.Models;

namespace KeyVaultLib
{
    public static class KeyVaultHelper  
    {
        private const string VaultName = "https://mykeyvault.vault.azure.net/";

        public static string GetSecretValue(string secretName)
        {
            try
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var key = $"SECRET{secretName}";
                var secret = CacheAsideHelper.GetOrAdd(async () => await keyVaultClient.GetSecretAsync(VaultName, secretName).ConfigureAwait(false), new TimeSpan(2, 0, 0), key);
                return secret.Value;
            }
            catch (KeyVaultErrorException ex)
            {
                if (ex.Body.Error.Code == "SecretNotFound") return string.Empty;    // secret does not exist - return blank
                throw;
            }
        }
    }
}
