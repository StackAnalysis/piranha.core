/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * http://github.com/tidyui/coreweb
 *
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Piranha;
using Piranha.AspNetCore.Identity.SQLServer;
using Piranha.AttributeBuilder;
using Piranha.Local;
using Piranha.Data.EF.SQLServer;
using System;

namespace MvcWeb
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddPiranha(options =>
            {
                options.AddRazorRuntimeCompilation = true;

                options.UseFileStorage(naming: FileStorageNaming.UniqueFolderNames);
                options.UseImageSharp();
                options.UseManager();
                options.UseTinyMCE();
                options.UseMemoryCache();

                options.UseEF<SQLServerDb>(db => db.UseSqlServer("Server=80.209.233.70;Database=StackAnalsysisBlog;User ID=sa;Password=Sarah#2010;MultipleActiveResultSets=true"));
                
                options.UseIdentityWithSeed<IdentitySQLServerDb>(db =>
                     db.UseSqlServer("Server=80.209.233.70;Database=StackAnalsysisBlog;User ID=sa;Password=Sarah#2010;MultipleActiveResultSets=true"),

                     identityOptions: io =>
                     {
                        // Password settings
                        io.Password.RequireDigit = false;
                         io.Password.RequiredLength = 6;
                         io.Password.RequireNonAlphanumeric = false;
                         io.Password.RequireUppercase = false;
                         io.Password.RequireLowercase = false;
                         io.Password.RequiredUniqueChars = 1;

                        // Lockout settings
                        io.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                         io.Lockout.MaxFailedAccessAttempts = 10;
                         io.Lockout.AllowedForNewUsers = true;

                        // User settings
                        io.User.RequireUniqueEmail = true;
                     });



                options.UseSecurity(o =>
                {
                    o.UsePermission("Subscriber");
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApi api)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            App.Init(api);

            // Configure cache level
            App.CacheLevel = Piranha.Cache.CacheLevel.Full;

            // Build content types
            new ContentTypeBuilder(api)
                .AddAssembly(typeof(Startup).Assembly)
                .Build()
                .DeleteOrphans();

            // Configure editor
            Piranha.Manager.Editor.EditorConfig.FromFile("editorconfig.json");

            /**
             *
             * Test another culture in the UI
             *
            var cultureInfo = new System.Globalization.CultureInfo("en-US");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
             */

            app.UsePiranha(options =>
            {
                options.UseManager();
                options.UseTinyMCE();
                options.UseIdentity();
            });

            Seed.RunAsync(api).GetAwaiter().GetResult();
        }
    }
}
