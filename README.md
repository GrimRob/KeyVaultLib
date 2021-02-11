# What is KeyVaultLib

   KeyVaultLib can be used to interact with Azure key vaults and provide functions to get items from a vault. It also caches the result (for 2 hours) to reduce external calls to the vault.

   KeyVaultLib should work with .NET Core and Framework as it is written in C# netstandard2.0

   Your vault should be configured to give access to the identity your code is running under. See https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/tutorial-windows-vm-access-nonaad

# How to use KeyVaultLib

Change this line of code to match your keyvault's url (default is to end with vault.azure.net)
   // TODO - change url to match Azure keyvault name
   private const string VaultName = "https://mykeyvault.vault.azure.net/";


# To get a secret:

    var secretValue = await KeyVaultHelper.GetSecretValueAsync("MySecretName");
    returns an empty string if it does not exist

# To get a certificate:

    var certBundle = await KeyVaultHelper.GetCertificateValueAsync("MyCertName");
    returns null if it does not exist

