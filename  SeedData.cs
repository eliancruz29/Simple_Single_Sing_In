using System;
using System.Collections.Generic;
using System.Linq;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Simple_Single_Sing_On
{
    public class  SeedData
    {
        public static void EnsureSeedData(IApplicationBuilder app)
        {
            Console.WriteLine("Seeding database...");

            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();

                var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                // scope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();

                EnsureSeedClientData(context);
                // EnsureSeedIdentityResourcesData(context, configuration);
                EnsureSeedApiResourcesData(context);
            }

            Console.WriteLine("Done seeding database.");
            Console.WriteLine();
        }

        private static void EnsureSeedClientData(ConfigurationDbContext ctx)
        {
            Console.WriteLine("Clients being populated");
            #region Add or Update Clients
            foreach (var client in Config.GetClients().ToList())
            {
                var _client = ctx.Clients
                    .Include(c => c.AllowedGrantTypes)
                    .Include(c => c.AllowedCorsOrigins)
                    .Include(c => c.ClientSecrets)
                    .Include(c => c.RedirectUris)
                    .Include(c => c.PostLogoutRedirectUris)
                    .Include(c => c.AllowedScopes)
                    .FirstOrDefault(c => c.ClientId.Equals(client.ClientId));

                if (null == _client)
                {
                    ctx.Clients.Add(client.ToEntity());
                }
                else
                {
                    var entity = client.ToEntity();
                    _client.ClientName = entity.ClientName;
                    _client.AllowedGrantTypes = UpdateListObject(_client.AllowedGrantTypes, entity.AllowedGrantTypes, "GrantType");
                    _client.AccessTokenLifetime = entity.AccessTokenLifetime;
                    _client.AllowedCorsOrigins = UpdateListObject(_client.AllowedCorsOrigins, entity.AllowedCorsOrigins, "Origin");
                    _client.ClientUri = entity.ClientUri;
                    _client.ClientSecrets = UpdateListObject(_client.ClientSecrets, entity.ClientSecrets, "Value");
                    _client.RedirectUris = UpdateListObject(_client.RedirectUris, entity.RedirectUris, "RedirectUri");
                    _client.PostLogoutRedirectUris = UpdateListObject(_client.PostLogoutRedirectUris, entity.PostLogoutRedirectUris, "PostLogoutRedirectUri");
                    _client.AllowedScopes = UpdateListObject(_client.AllowedScopes, entity.AllowedScopes, "Scope");
                    ctx.Clients.Update(_client);
                }
            }
            #endregion

            #region Delete unused Clients
            var clientIds = Config.GetClients().ToList().Select(c => c.ClientId);
            var clientsToDelete = ctx.Clients.Where(c => !clientIds.Contains(c.ClientId));

            if (clientsToDelete.Count() > 0)
            {
                ctx.Clients.RemoveRange(clientsToDelete);
            }
            #endregion
            ctx.SaveChanges();

            Console.WriteLine("Clients Finish");
        }

        private static void EnsureSeedIdentityResourcesData(ConfigurationDbContext ctx)
        {
            // Console.WriteLine("IdentityResources being populated");
            // #region Add or Update IdentityResources
            // foreach (var resource in Config.GetIdentityResources().ToList())
            // {
            //     var _resource = ctx.IdentityResources.FirstOrDefault(c => c.Name.Equals(resource.Name));

            //     if (null == _resource)
            //     {
            //         ctx.IdentityResources.Add(resource.ToEntity());
            //     }
            //     else
            //     {
            //         var entity = resource.ToEntity();
            //         _resource.DisplayName = entity.DisplayName;
            //         ctx.IdentityResources.Update(_resource);
            //     }
            // }
            // #endregion

            // #region Delete unused IdentityResources
            // var resources = Config.GetIdentityResources().ToList().Select(c => c.Name);
            // var resourcesToDelete = ctx.IdentityResources.Where(c => !resources.Contains(c.Name));

            // if (resourcesToDelete.Count() > 0)
            // {
            //     ctx.IdentityResources.RemoveRange(resourcesToDelete);
            // }
            // #endregion

            // ctx.SaveChanges();

            // Console.WriteLine("IdentityResources Finish");
        }

        private static void EnsureSeedApiResourcesData(ConfigurationDbContext ctx)
        {
            Console.WriteLine("ApiResources being populated");
            #region Add or Update ApiResources
            foreach (var resource in Config.GetApiResources().ToList())
            {
                var _resource = ctx.ApiResources
                    .Include(c => c.Secrets)
                    .FirstOrDefault(c => c.Name.Equals(resource.Name));

                if (null == _resource)
                {
                    ctx.ApiResources.Add(resource.ToEntity());
                }
                else
                {
                    var entity = resource.ToEntity();
                    _resource.DisplayName = entity.DisplayName;
                    _resource.Secrets = UpdateListObject(_resource.Secrets, entity.Secrets, "Value");
                    ctx.ApiResources.Update(_resource);
                }
            }
            #endregion

            #region Delete unused ApiResources
            var resources = Config.GetApiResources().ToList().Select(c => c.Name);
            var resourcesToDelete = ctx.ApiResources.Where(c => !resources.Contains(c.Name));

            if (resourcesToDelete.Count() > 0)
            {
                ctx.ApiResources.RemoveRange(resourcesToDelete);
            }
            #endregion
            ctx.SaveChanges();

            Console.WriteLine("ApiResources Finish");
        }

        private static List<T> UpdateListObject<T>(List<T> source, List<T> update, string propName)
        {
            var newValues = update.Where(g => !source.Any(
                a => a.GetType().GetProperty(propName).GetValue(a, null).Equals(
                    g.GetType().GetProperty(propName).GetValue(g, null))));

            if (newValues != null && newValues.Count() > 0)
                source.AddRange(newValues);

            return source.Where(g => update.Any(
                a => a.GetType().GetProperty(propName).GetValue(a, null).Equals(
                    g.GetType().GetProperty(propName).GetValue(g, null))))
                    .ToList();
        }
    }
}
