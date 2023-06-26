using KeyVaultLib;

namespace TestProject1;

[TestClass]
public class KeyVaultHelperTests
{
    [TestMethod]
    public void GetSecretListTest()
    {
        var vault = Environment.GetEnvironmentVariable("TEST_VaultName");
        var kv = new KeyVaultHelper(vault);
        var list = kv.GetSecretsList();
        Assert.IsTrue(list.Count > 0);
    }

    [TestMethod]
    public async Task GetSecretAsyncTest()
    {
        var vault = Environment.GetEnvironmentVariable("TEST_VaultName");
        var kv = new KeyVaultHelper(vault);
        var mySecret = await kv.GetSecretValueAsync("MySecret");
        Assert.IsNotNull(mySecret);
    }

    [TestMethod]
    public async Task GetNoSecretAsyncTest()
    {
        var vault = Environment.GetEnvironmentVariable("TEST_VaultName");
        var kv = new KeyVaultHelper(vault);
        var dhdhjdhjdhj = await kv.GetSecretValueAsync("dhdhjdhjdhj");
        Assert.IsTrue(string.IsNullOrEmpty(dhdhjdhjdhj));
    }

    [TestMethod]
    public async Task GetNoCertificateAsyncTest()
    {
        var vault = Environment.GetEnvironmentVariable("TEST_VaultName");
        var kv = new KeyVaultHelper(vault);
        var dhdhjdhjdhj = await kv.GetCertificateValueAsync("dhdhjdhjdhj");
        Assert.IsNull(dhdhjdhjdhj);
    }

    [TestMethod]
    public async Task SetSecretAsyncTest()
    {
        var vault = Environment.GetEnvironmentVariable("TEST_VaultName");
        var kv = new KeyVaultHelper(vault);
        var secrets = new List<Azure.Security.KeyVault.Secrets.KeyVaultSecret>
        {
            new Azure.Security.KeyVault.Secrets.KeyVaultSecret("test", "testValue")
        };
        await kv.SetSecretsAsync(secrets);
        var testValue = await kv.GetSecretValueAsync("test");
        Assert.AreEqual("testValue", testValue);
    }
}