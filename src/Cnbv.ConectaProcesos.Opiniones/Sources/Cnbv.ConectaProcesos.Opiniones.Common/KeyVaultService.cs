using Azure.Security.KeyVault.Secrets;

namespace Cnbv.ConectaProcesos.Opiniones.Common
{
  public class KeyVaultService
  {
    private readonly SecretClient _secretClient;

    public KeyVaultService(SecretClient secretClient)
    {
      _secretClient = secretClient;
    }

    public async Task<string> GetSecretValueAsync(string secretName)
    {
      KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
      return secret.Value;
    }
  }
}
