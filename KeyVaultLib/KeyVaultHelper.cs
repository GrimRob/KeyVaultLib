using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KeyVaultLib;

public class KeyVaultHelper : IKeyVaultHelper
{
    private const int CacheHours = 2;
    private readonly ICacheAsideHelper _cacheAsideHelper;
    private SecretClient _keyVaultSecretClient;
    private CertificateClient _keyVaultCertificateClient;

    public KeyVaultHelper(ICacheAsideHelper cacheAsideHelper, IConfiguration configuration)
    {
        _cacheAsideHelper = cacheAsideHelper;
        OpenVault(configuration["KeyVault:VaultName"]);
    }

    /// <summary>
    /// Get a list of all the vault's secrets
    /// </summary>
    /// <returns>A List of the secrets</returns>
    public List<SecretProperties> GetSecretsList()
    {
        var secretList = new List<SecretProperties>();
        var secrets = _keyVaultSecretClient.GetPropertiesOfSecrets();
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
        foreach (var secret in secrets)
        {
            await _keyVaultSecretClient.SetSecretAsync(secret).ConfigureAwait(false);
        }
    }
    
    /// <summary>
         /// Gets a secret from an Azure key vault
         /// Caches the secret in memory for 2 hours
         /// </summary>
         /// <param name="secretName"></param>
         /// <returns>The secret value or string.Empty if it does not exist</returns>
    public string GetSecretValue(string secretName)
    {
        var cacheKey = $"SECRET{secretName}";

        try
        {
            // Attempt to get the cached secret
            var secret = _cacheAsideHelper.GetOrAdd(
                () => _keyVaultSecretClient.GetSecretAsync(secretName).GetAwaiter().GetResult().Value,
                new TimeSpan(CacheHours, 0, 0),
                cacheKey
            );

            if (secret.Properties.NotBefore == null || secret.Properties.NotBefore <= DateTimeOffset.UtcNow &&
                (secret.Properties.ExpiresOn == null || secret.Properties.ExpiresOn >= DateTimeOffset.UtcNow))
            {
                // Current version is valid
                return secret?.Value ?? string.Empty;
            }

            // Fall back to checking all versions if the current version is not valid
            var versions = _keyVaultSecretClient.GetPropertiesOfSecretVersions(secretName)
                .Where(v => (v.Enabled ?? false) &&
                            (!v.NotBefore.HasValue || v.NotBefore.Value <= DateTimeOffset.UtcNow) &&
                            (!v.ExpiresOn.HasValue || v.ExpiresOn.Value >= DateTimeOffset.UtcNow))
                .OrderByDescending(_ => _.CreatedOn)
                .ToList();

            if (versions.Any())
            {
                var currentVersion = versions.First(); // Assuming first valid version is desired
                var validSecret = _keyVaultSecretClient.GetSecret(secretName, currentVersion.Version);
                return validSecret?.Value.Value ?? string.Empty;
            }

            return string.Empty; // No valid version found
        }
        catch (RequestFailedException ex)
        {
            if (ex.ErrorCode == "SecretNotFound")
                return string.Empty; // Secret does not exist, return blank

            throw;
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
        var cacheKey = $"SECRET{secretName}";

        try
        {
            // Attempt to get the cached secret
            var secret = await _cacheAsideHelper.GetOrAddAsync(
                async () =>
                {
                    var s = await _keyVaultSecretClient.GetSecretAsync(secretName).ConfigureAwait(false);
                    return s.Value; // Extract and cache only the secret's value
                },
                TimeSpan.FromHours(CacheHours), // Simplified TimeSpan creation
                cacheKey
            ).ConfigureAwait(false);


            if (secret.Properties.NotBefore <= DateTimeOffset.UtcNow &&
                (secret.Properties.ExpiresOn == null || secret.Properties.ExpiresOn >= DateTimeOffset.UtcNow))
            {
                // Current version is valid
                return secret?.Value ?? string.Empty;
            }

            // Fall back to checking all versions if the current version is not valid
            var versions = _keyVaultSecretClient.GetPropertiesOfSecretVersions(secretName)
                .Where(v => (v.Enabled ?? false) &&
                            (!v.NotBefore.HasValue || v.NotBefore.Value <= DateTimeOffset.UtcNow) &&
                            (!v.ExpiresOn.HasValue || v.ExpiresOn.Value >= DateTimeOffset.UtcNow))
                .OrderByDescending(_ => _.CreatedOn)
                .ToList();

            if (versions.Any())
            {
                var currentVersion = versions.First(); // Assuming first valid version is desired
                var validSecret = await _keyVaultSecretClient.GetSecretAsync(secretName, currentVersion.Version).ConfigureAwait(false);
                return validSecret?.Value.Value ?? string.Empty;
            }

            return string.Empty; // No valid version found
        }
        catch (RequestFailedException ex)
        {
            if (ex.ErrorCode == "SecretNotFound")
                return string.Empty; // Secret does not exist, return blank

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
            var key = $"CERT{certName}";
            var cert = await _cacheAsideHelper.GetOrAdd(async () => await _keyVaultCertificateClient.GetCertificateAsync(certName).ConfigureAwait(false), new TimeSpan(CacheHours, 0, 0), key).ConfigureAwait(false);
            return cert;
        }
        catch (RequestFailedException ex)
        {
            if (ex.ErrorCode == "CertificateNotFound") return null;
            throw;
        }
    }

    private static SecretClient GetSecretClient(string vaultName)
    {
        var clientOptions = new SecretClientOptions
        {
            Retry =
            {
                NetworkTimeout = TimeSpan.FromSeconds(30)
            }
        };

        var secretClient = new SecretClient(vaultUri: new Uri(vaultName), credential: new DefaultAzureCredential(), clientOptions);
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
        return secretClient;
    }

    private static CertificateClient GetCertificateClient(string vaultName)
    {
        var clientOptions = new CertificateClientOptions
        {
            Retry =
            {
                NetworkTimeout = TimeSpan.FromSeconds(30)
            }
        };

        var certificateClient = new CertificateClient(vaultUri: new Uri(vaultName), credential: new DefaultAzureCredential(), clientOptions);
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
        return certificateClient;
    }

    private void OpenVault(string vaultName)
    {
        if (string.IsNullOrWhiteSpace(vaultName)) throw new ArgumentException("vaultName is required");
        _keyVaultSecretClient = GetSecretClient(vaultName);
        _keyVaultCertificateClient = GetCertificateClient(vaultName);
    }
}

