using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServerDemo
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes => new List<ApiScope>
        {
            new ApiScope("api", "my api"),
            new ApiScope("identity", "my identity"),

        };

        public static IEnumerable<ApiResource> ApiResources => new List<ApiResource>
        {
             new ApiResource
            {
                Name = "api",
                DisplayName = "Api service",
                Scopes = new List<string> {"api"},
                ApiSecrets = { new Secret("api-secret".Sha256()) },
            },
              new ApiResource
            {
                Name = "identity",
                DisplayName = "Identity service",
                Scopes = new List<string> {"identity"},
                ApiSecrets = { new Secret("identity-secret".Sha256()) },
            }
        };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "learner",
                    ClientName = "Learner Portal",
                    AllowedGrantTypes = GrantTypes.Code,
                    ClientSecrets = { new Secret("learner".Sha256()) },
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AccessTokenType = AccessTokenType.Reference,
                    RedirectUris = new List<string> { "http://localhost:4200/signin-oidc"},
                    PostLogoutRedirectUris = new List<string> {"http://localhost:4200/"},
                    AllowedCorsOrigins = new List<string> {"http://localhost:4200"},
                    AllowAccessTokensViaBrowser = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "api"
                    }
                },
                new Client
                {
                    ClientId = "mobile",
                    ClientName = "Mobile Portal",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    ClientSecrets = { new Secret("mobile".Sha256()) },
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RedirectUris = new List<string> { "http://localhost:4200/signin-oidc"},
                    PostLogoutRedirectUris = new List<string> {"http://localhost:4200/"},
                    AllowedCorsOrigins = new List<string> {"http://localhost:4200"},
                    AllowAccessTokensViaBrowser = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "api"
                    }
                }
            };
    }
}
