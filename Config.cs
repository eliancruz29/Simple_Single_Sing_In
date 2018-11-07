using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;

namespace Simple_Single_Sing_On
{
    public class Config
    {
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("Api", "Product Request Api")
                {
                    Description = "My API for Proof",
                    ApiSecrets = new List<Secret> { new Secret("secret".Sha256()) }
                },
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                // OpenID Connect hybrid flow and client credentials client (MVC)
                new Client
                {
                    ClientName = "MVC Client",

                    ClientId = "mvc",

                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                    RequireConsent = false,

                    AllowedCorsOrigins = new List<string> { "https://localhost:5002" },

                    ClientUri = "https://localhost:5002",

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris = { "https://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:5002/", "https://localhost:5002/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        "Api",
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                    }
                }
            };
        }
    }
}
