﻿using ClusterEmulator.Service.Simulation.Core;
using ClusterEmulator.Service.Simulation.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace CoreService
{
    /// <summary>
    /// Provides an ease-of-use abstraction for instance container managment and http pipeline configuration.
    /// </summary>
    public class Startup
    {
        private IRegistry Registry { get; }


        /// <summary>
        /// Creates a new instance of <see cref="Startup"/>
        /// </summary>
        /// <param name="registry">The simulation registry</param>
        public Startup(IRegistry registry)
        {
            Registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }


        /// <summary>
        /// This method gets called by the runtime and is used to add services to the container.
        /// </summary>
        /// <param name="services">The service container.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSimulationEngineClients(Registry);
        }


        /// <summary>
        /// This method gets called by the runtime and is used to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">Environemnt information wrapper.</param>
        /// <param name="clientFactory">A factory for creating Http Clients</param>
        /// <param name="registry">The simulation registry containing configuration information.</param>
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
