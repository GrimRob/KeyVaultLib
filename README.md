# KeyVaultLib Usage

To get a secret:

    var secretValue = await KeyVaultHelper.GetSecretValueAsync("MySecretName");

 To get a certificate:

    var certBundle = await KeyVaultHelper.GetCertificateValueAsync("MyCertName");
