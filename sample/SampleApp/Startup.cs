﻿using MassTransit;
using Metrics.Extensions.MassTransit;
using Metrics.Extensions.Tracking;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SampleApp.Database;
using SampleApp.Database.Entities;
using SampleApp.MessageBus;
using System;
using System.Linq;

namespace SampleApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureDatabase(services);

            services
                .AddMvc(options => { options.EnableEndpointRouting = false; });
            ConfigureMessageBus(services);
        }

        private void ConfigureMessageBus(IServiceCollection services)
        {
            services.Configure<RabbitMQConfiguration>(Configuration.GetSection(RabbitMQConfiguration.SectionKey));

            services.AddMassTransit(x =>
            {
                x.AddBus(context =>
                {
                    return Bus.Factory.CreateUsingRabbitMq(cfg =>
                    {
                        var config = context.GetService<IOptions<RabbitMQConfiguration>>().Value;

                        cfg.Host(new Uri($"rabbitmq://{config.Host}/"), host =>
                        {
                            host.Username(config.User);
                            host.Password(config.Password);
                        });

                        // add a custom tracker from Metrics.Extensions.MassTransit
                        cfg.AddPipeSpecification(new TrackConsumingSpecification<ConsumeContext>());

                        cfg.ReceiveEndpoint("queue-name", ec =>
                        {
                            ec.Consumer<MyMessageConsumer>();
                        });
                    });
                });
            });

            services.AddHostedService<BusService>();

            services.AddTransient<ICustomTracker, CustomTracker>();
        }

        private void ConfigureDatabase(IServiceCollection services)
        {
            services.Configure<DatabaseConfiguration>(Configuration.GetSection(DatabaseConfiguration.SectionKey));
            services.AddDbContext<MyDbContext>(options => options.UseSqlServer(Configuration.GetSection(DatabaseConfiguration.SectionKey).Get<DatabaseConfiguration>().ConnectionString));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            MigrateDatabase(app);
        }

        private static void MigrateDatabase(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<MyDbContext>().Database.Migrate();

                var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                if (!context.MyEntities.Any())
                {
                    context.MyEntities.Add(new MyEntity { Value = "1" });
                    context.MyEntities.Add(new MyEntity { Value = "2" });
                    context.MyEntities.Add(new MyEntity { Value = "3" });
                    context.SaveChanges();
                }
            }
        }
    }
}
