using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TestProject1;

public class KeyVaultHelperTests
{
    private IKeyVaultHelper _keyVaultHelper;

    public KeyVaultHelperTests()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();       
        
        _keyVaultHelper = new KeyVaultHelper(new CacheAsideHelper(), configuration);
    }

    [Fact]
    public void GetSecretsList_ShouldReturnNonEmptyList()
    {
        var list = _keyVaultHelper.GetSecretsList();
        list.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task GetSecretAsync_ShouldReturnSecretValue()
    {
        var mySecret = await _keyVaultHelper.GetSecretValueAsync("MySecret");
        mySecret.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetSecretAsync_ShouldReturnNullForNonExistentSecret()
    {
        var secret = await _keyVaultHelper.GetSecretValueAsync("NonExistentSecret");
        secret.ShouldBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCertificateAsync_ShouldReturnNullForNonExistentCertificate()
    {
        var certificate = await _keyVaultHelper.GetCertificateValueAsync("NonExistentCertificate");
        certificate.ShouldBeNull();
    }

    [Fact]
    public async Task SetSecretAsync_ShouldStoreAndRetrieveSecret()
    {
        var secrets = new List<Azure.Security.KeyVault.Secrets.KeyVaultSecret>
        {
            new Azure.Security.KeyVault.Secrets.KeyVaultSecret("test", "testValue")
        };
        await _keyVaultHelper.SetSecretsAsync(secrets);
        var testValue = await _keyVaultHelper.GetSecretValueAsync("test");
        testValue.ShouldBe("testValue");
    }
}
