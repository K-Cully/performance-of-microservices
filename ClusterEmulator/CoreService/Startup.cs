using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CoreService.Simulation.Core;
using CoreService.Simulation.HttpClientConfiguration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;

namespace CoreService
{
    public class Startup
    {
        private IRegistry Registry { get; }
        private IConfiguration Configuration { get; }


        public Startup(IConfiguration configuration, IRegistry registry)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Register initialised policy registry
            services.AddPolicyRegistry(Registry.PolicyRegistry);

            // Enumerate all registered clients
            var clients = new List<KeyValuePair<string, ClientConfig>>(Registry.Clients);
            foreach ((string name, var config) in clients)
            {
                IHttpClientBuilder builder = services.AddHttpClient(name, c =>
                {
                    c.BaseAddress = new Uri(config.BaseAddress);
                    if (config.RequestHeaders != null)
                    {
                        foreach ((string key, string value) in config.RequestHeaders)
                        {
                            c.DefaultRequestHeaders.Add(key, value);
                        }
                    }

                    // TODO: add user agent c.DefaultRequestHeaders.Add(Headers.)
                });

                // Add Polly policies to http client builder.
                // Note that any policies added through the http client factory initialization
                // must be of type IAsyncPolicy<HttpResponseMessage> or Polly will throw errors.
                foreach(string policy in config.Policies)
                {
                    builder = builder.AddPolicyHandlerFromRegistry(policy);
                }
            }
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IHttpClientFactory clientFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // TODO: decide on use of this based on AppInsights integration complexity

            // Write streamlined Serilog completion events, instead of the more verbose ones from the framework.
            // To use the default framework request logging instead, remove this line and set the "Microsoft"
            // level in appsettings.json to "Information".
            // To use Serilog request logging, set "Microsoft" and "System" levels to "Warning" and uncomment here.

            // app.UseSerilogRequestLogging()

            app.UseMvc();
            Registry.ConfigureHttpClients(clientFactory);
        }
    }
}
