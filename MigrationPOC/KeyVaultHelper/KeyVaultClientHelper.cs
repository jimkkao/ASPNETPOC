using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace KeyVaultHelper
{
    public class KeyVaultClientHelper 
    {
        protected KeyVaultClient _client = null;

        public KeyVaultClientHelper()
        {
            try
            {           
                _client = CreateKVClient();
            }
            catch (Exception ex)
            {
                throw new Exception("Faile to create KeyVaultClient.", ex);
            }
        }

        protected string GetClientId()
        {
            string clientId = Environment.GetEnvironmentVariable("akvClientId");
            
            return clientId;
        }

        protected string GetClientSecret()
        {
            string clientSecret = Environment.GetEnvironmentVariable("akvClientSecret");
            return clientSecret;
        }

        protected KeyVaultClient CreateKVClient()
        {
            string clientId = GetClientId();
            string clientSecret = GetClientSecret();

            if( string.IsNullOrEmpty(clientId ) ||
                string.IsNullOrEmpty(clientSecret) )
            {
                return CreateKVClientDefaultSPN();
            }
            else
            {
                return CreateKVClient(clientId, clientSecret);
            }
        }


        protected KeyVaultClient CreateKVClient(string clientId, string clientSecret)
        {

            KeyVaultClient kvClient = new KeyVaultClient(async (authority, resource, scope) =>
            {
                var adCredential = new ClientCredential(clientId, clientSecret);
                var authenticationContext = new AuthenticationContext(authority, null);
                return (await authenticationContext.AcquireTokenAsync(resource, adCredential)).AccessToken;
            });

            return kvClient;
        }

        protected KeyVaultClient CreateKVClientDefaultSPN()
        {
            try
            {
                AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();

                var client = new KeyVaultClient(
                        new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

                return client;
            }
            catch (Exception ex)
            {
                throw new Exception("Faile to create KeyVaultClient.", ex);
            }
        }

        public async Task<string> EncryptAsync(string vaultUrl, string keyName, string keyVersion, byte[] input, string algorithm)
        {
            var encryptedData = await _client.EncryptAsync(vaultUrl, keyName, keyVersion, algorithm, input);

            return System.Convert.ToBase64String(encryptedData.Result);
        }

        public async Task<string> EncryptAsync(string keyId, byte[] input, string algorithm)
        {
            var encrytpedData = await _client.EncryptAsync(keyId, algorithm, input);

            return System.Convert.ToBase64String(encrytpedData.Result);
        }

        public async Task<string> EncryptAsync(string vaultUrl, string keyName, string keyVersion, string input, string algorithm)
        {
            var encryptedData = await _client.EncryptAsync(vaultUrl, keyName, keyVersion, algorithm, Encoding.UTF8.GetBytes(input));

            return System.Convert.ToBase64String(encryptedData.Result);
        }

        public async Task<string> EncryptAsync(string keyId, string input, string algorithm)
        {
            var encryptedData = await _client.EncryptAsync(keyId, algorithm, Encoding.UTF8.GetBytes(input));

            return System.Convert.ToBase64String(encryptedData.Result);
        }

        public async Task<byte[]> DecryptAsync(string vaultUrl, string keyName, string keyVersion, string base64Input, string algorithm)
        {
            var decryptedData = await _client.DecryptAsync(vaultUrl, keyName, keyVersion, algorithm, System.Convert.FromBase64String(base64Input));

            return decryptedData.Result;
        }

        public async Task<byte[]> DecryptAsync(string keyId, string base64Input, string algorithm)
        {
            var decryptedData = await _client.DecryptAsync(keyId, algorithm, System.Convert.FromBase64String(base64Input));

            return decryptedData.Result;
        }


        public async Task<JsonWebKey> GetKeyAsync(string vaultUrl, string keyName)
        {
            var bundle = await _client.GetKeyAsync(vaultUrl, keyName);

            return bundle.Key;
        }

        public async Task<string> GetSecretAsync(string vaultUrl, string secretName)
        {
            var bundle = await _client.GetSecretAsync(vaultUrl, secretName);

            return bundle.Value;
        }
    }
}
