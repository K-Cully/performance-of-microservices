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
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // TODO: use polly
            // TODO: add registry and construct http clients
            // as per https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory
            // and https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-2.1

            app.UseMvc();
        }
    }
}
