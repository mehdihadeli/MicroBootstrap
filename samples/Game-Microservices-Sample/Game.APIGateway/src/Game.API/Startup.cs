using Autofac;
using MicroBootstrap.Jaeger;
using MicroBootstrap.Redis;
using MicroBootstrap.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Autofac.Extensions.DependencyInjection;
using Game.API.Services;
using MicroBootstrap.Swagger;
using MicroBootstrap;
using Consul;
using MicroBootstrap.MessageBrokers.RabbitMQ;
using MicroBootstrap.Authentication;
using Game.API.Infrastructure;
using PGame.API.Controllers.Infrastructure;
using Microsoft.Extensions.Configuration;
using MicroBootstrap.Discovery.Consul.Consul;
using MicroBootstrap.Http.RestEase;

namespace Game.API
{
    public class Startup
    {
        private static readonly string[] Headers = new[] { "X-Operation", "X-Resource", "X-Total-Count" };
        public IConfiguration Configuration { get; }
        public ILifetimeScope AutofacContainer { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<ICorrelationContextBuilder, CorrelationContextBuilder>();

            //This is called after ConfigureContainer. You can use IApplicationBuilder.ApplicationServices
            // here if you need to resolve things from the container.
            //this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();
            services.AddWebApi();
            services.AddHealthChecks();
            //services.AddSwaggerDocs();
            services.AddConsul();
            // services.AddJwt();
            services.AddJaeger();
            services.AddOpenTracing();
            services.AddRedis();
            services.AddRabbitMQ();// Rabbit will add Redis automatically if options.MessageProcessor.Type = redis
            // services.AddAuthorization(x => x.AddPolicy("admin", p => p.RequireRole("admin")));
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", cors =>
                    cors.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders(Headers));
            });
            services.AddInitializers();

            //RestEase Register Services
            services.AddRestEaseClient<IGameEventProcessorService>("game-event-processor-service");
        }


        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Register your own things directly with Autofac
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
        IHostApplicationLifetime applicationLifetime, IConsulClient client)
        {
            // If, for some reason, you need a reference to the built container, you
            // can use the convenience extension method GetAutofacRoot.
            this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseErrorHandler();
            app.UseStaticFiles();
            //app.UseSwaggerDocs();
            app.UseInitializers();
            app.UseRouting();
            // app.UseAuthentication(); // Must be after UseRouting()
            // app.UseAuthorization(); // Must be after UseAuthentication()
            // app.UseAccessTokenValidator();
            app.UseServiceId();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapHealthChecks("/healthz");
                endpoints.MapGet("/", async context =>
                     await context.Response.WriteAsync(context.RequestServices.GetService<AppOptions>().Name));
            }
        );
            app.UseRabbitMQ();
        }
    }
}
