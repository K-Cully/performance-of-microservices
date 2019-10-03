using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NameGeneratorService.Core;

namespace NameGeneratorService
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IConfiguration Configuration { get; }

        public ILogger<Startup> Logger;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            string randomUrl = Settings.RandomServiceBaseUrl;
            string lookupUrl = Settings.LookupServiceBaseUrl;

            Logger.LogInformation("Resolved {RandomServiceUrl} for random generator http client", randomUrl);
            Logger.LogInformation("Resolved {LookupServiceUrl} for name lookup http client", lookupUrl);

            // Add http clients to the default factory
            services.AddHttpClient(Settings.RandomApiClientName,
                c => c.BaseAddress = new Uri(randomUrl));
            services.AddHttpClient(Settings.NameLookupApiClientName,
                c => c.BaseAddress = new Uri(lookupUrl));

            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseHealthChecks("/health");
        }
    }
}
