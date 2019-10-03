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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IConfiguration Configuration { get; }

        private string randomUrl;
        private string lookupUrl;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            randomUrl = Settings.RandomServiceBaseUrl;
            lookupUrl = Settings.LookupServiceBaseUrl;

            // Add http clients to the default factory
            services.AddHttpClient(Settings.RandomApiClientName,
                c => c.BaseAddress = new Uri(randomUrl));
            services.AddHttpClient(Settings.NameLookupApiClientName,
                c => c.BaseAddress = new Uri(lookupUrl));

            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            logger.LogInformation("Resolved {RandomServiceUrl} for random generator http client", randomUrl);
            logger.LogInformation("Resolved {LookupServiceUrl} for name lookup http client", lookupUrl);

            app.UseMvc();
            app.UseHealthChecks("/health");
        }
    }
}
