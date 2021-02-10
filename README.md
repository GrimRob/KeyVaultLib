# KeyVaultLib Usage

To get a secret:

    var secretValue = KeyVaultHelper.GetSecretValue("MySecretName");

 To get a certificate:

    var certBundle = KeyVaultHelper.GetCertificateValue("MyCertName");
