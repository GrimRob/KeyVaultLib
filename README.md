# What is KeyVaultLib

   KeyVaultLib can be used to interact with Azure key vaults and provide functions to get pre-existing items from a vault. It also caches the result (for 2 hours) to reduce external calls to the vault.

   KeyVaultLib should work with .NET Core and Framework as it is written in C# netstandard2.0

   Your vault should be configured to give access to the identity your code is running under. See [tutorial-windows-vm-access-nonaad]( https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/tutorial-windows-vm-access-nonaad).

# How to use KeyVaultLib 

 Inject the IKeyVaultHelper and ICacheAsideHelper implementations into your service collection at startup.

```
    services.AddSingleton<ICacheAsideHelper, CacheAsideHelper>()
            .AddSingleton(IKeyVaultHelper>, KeyVaultHelper>()
```

## To get a secret:

```
var secretValue = await _keyVaultHelper.GetSecretValueAsync("MySecretName");
```
*returns an empty string if it does not exist*

## To get a list of secrets:

```
var secrets = await _keyVaultHelper.GetSecretsListAsync()
```

## To get a certificate:

```
var certBundle = await _keyVaultHelper.GetCertificateValueAsync("MyCertName");
 ```

*returns null if it does not exist*

## Unit tests

In `KeyVaultHelperTests\appsettings.json` The variable VaultName should point to the full url of your vault, e.g. https://myvault.vault.azure.net/

