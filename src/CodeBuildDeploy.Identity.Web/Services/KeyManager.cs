using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

using Microsoft.IdentityModel.Tokens;

namespace CodeBuildDeploy.Identity.Web.Services
{
    public class KeyManager
    {
        public KeyManager()
        {
            RsaKey = RSA.Create();
            if (File.Exists("key"))
            {
                RsaKey.ImportRSAPrivateKey(File.ReadAllBytes("key"), out _);
            }
            else
            {
                var privateKey = RsaKey.ExportRSAPrivateKey();
                File.WriteAllBytes("key", privateKey);
            }

            string jwksStr;
            if (File.Exists("webkey"))
            {
                jwksStr = File.ReadAllText("webkey");
            }
            else
            {
                var key = new RsaSecurityKey(RsaKey);
                var webKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(new RsaSecurityKey(RsaKey));
                jwksStr = JsonSerializer.Serialize(new Dictionary<string, IList<JsonWebKey>>()
                {
                    { "keys", new List<JsonWebKey> { webKey } }
                });
            }
            JsonWebKeySet = new(jwksStr);
            File.WriteAllText("webkey", jwksStr);
        }

        public RSA RsaKey { get; }

        public JsonWebKeySet JsonWebKeySet { get; }
    }
}
