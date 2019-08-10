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
        public Startup(IConfiguration configuration, IRegistry registry)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }


        private IRegistry Registry { get; }


        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // TODO: add registry and construct http clients
            // as per https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory
            // and https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.1

            // On initializing registry create ClientConfiguration definitions
            // ClientConfiguration should have name and http client Action

            // Register initialised policy registry
            services.AddPolicyRegistry(Registry.PolicyRegistry);

            // Enumerate all registered clients
            var clients = new List<KeyValuePair<string, ClientConfig>>(Registry.Clients);
            foreach ((string name, var config) in clients)
            {
                IHttpClientBuilder builder = services.AddHttpClient(name, c =>
                {
                    c.BaseAddress = new Uri(config.BaseAddress);

                    // TODO: deal with RequestHeaders null
                    foreach ((string key, string value) in config.RequestHeaders)
                    {
                        c.DefaultRequestHeaders.Add(key, value);
                    }

                    // TODO: add user agent c.DefaultRequestHeaders.Add(Headers.)
                });

                foreach(string policy in config.Policies)
                {
                    // TODO: E2E test this registers correctly
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

            app.UseMvc();
            Registry.ConfigureHttpClients(clientFactory);
        }
    }
}
