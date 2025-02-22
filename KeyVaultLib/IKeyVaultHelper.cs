using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KeyVaultLib;

public interface IKeyVaultHelper
{
    List<SecretProperties> GetSecretsList();

    string GetSecretValue(string secretName);

    Task<string> GetSecretValueAsync(string secretName);

    Task SetSecretsAsync(List<KeyVaultSecret> secrets);

    Task<KeyVaultCertificateWithPolicy> GetCertificateValueAsync(string certName);
}
