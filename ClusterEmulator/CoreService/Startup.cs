using ClusterEmulator.Service.Simulation.Core;
using ClusterEmulator.Service.Simulation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            services.AddSimulationEngineClients(Registry);
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
