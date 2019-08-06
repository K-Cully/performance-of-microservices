using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreService.Simulation.Core;
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

            // foreach client in registry
            // var httpClientBuilder = services.AddHttpClient(client.Name, client.Configuration) 
            //  foreach policyName in client.Policies
            //  httpClientBuilder.AddPolicyHandlerFromRegistry(policyName)

            // TODO: get policy names from client configuration and enumerate
            string policyName = "C";

            // Register initialised policy registry
            services.AddPolicyRegistry(Registry.PolicyRegistry);

            // TODO: replace with actual implementation (example code only)
            services.AddHttpClient("test", c =>
            {
                c.BaseAddress = new Uri("http://hostname.com/");
                c.Timeout = TimeSpan.FromSeconds(5);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                // TODO: add useragent c.DefaultRequestHeaders.Add(Headers.)
            })
            .AddPolicyHandlerFromRegistry(policyName);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
