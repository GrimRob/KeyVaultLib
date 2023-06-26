# What is KeyVaultLib

   KeyVaultLib can be used to interact with Azure key vaults and provide functions to get items from a vault. It also caches the result (for 2 hours) to reduce external calls to the vault.

   KeyVaultLib should work with .NET Core and Framework as it is written in C# netstandard2.0

   Your vault should be configured to give access to the identity your code is running under. See [tutorial-windows-vm-access-nonaad]( https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/tutorial-windows-vm-access-nonaad).

# How to use KeyVaultLib

 Create a new instance, passing your keyvault url

```
var kv = new KeyVaultHelper("https://mykeyvault.vault.azure.net");  
```

## To get a secret:

```
var secretValue = await kv.GetSecretValueAsync("MySecretName");
```
*returns an empty string if it does not exist*

## To get a list of secrets:

```
var secrets = await kv.GetSecretsListAsync()
```

## To get a certificate:

```
var certBundle = await kv.GetCertificateValueAsync("MyCertName");
 ```

*returns null if it does not exist*

## Unit tests

The environmment variable TEST_VaultName should point to the full url of your vault, e.g. https://myvault.vault.azure.net/
