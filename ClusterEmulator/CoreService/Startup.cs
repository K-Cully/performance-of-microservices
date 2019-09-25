using ClusterEmulator.Service.Simulation.Core;
using ClusterEmulator.Service.Simulation.HttpClientConfiguration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace CoreService
{
    public class Startup
    {
        private IRegistry Registry { get; }


        public Startup(IRegistry registry)
        {
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

                    c.DefaultRequestHeaders.Add("User-Agent", $"ClusterEmulator.CoreService/1.0 (Client:{name})");
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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IHttpClientFactory clientFactory, IRegistry registry)
        {
            _ = app ?? throw new ArgumentNullException(nameof(app));
            _ = env ?? throw new ArgumentNullException(nameof(env));
            _ = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _ = registry ?? throw new ArgumentNullException(nameof(registry));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Injecting registry here to ensure it is added from the second, more locally scoped, instance container.
            registry.ConfigureHttpClients(clientFactory);
            app.UseMvc();
        }
    }
}
