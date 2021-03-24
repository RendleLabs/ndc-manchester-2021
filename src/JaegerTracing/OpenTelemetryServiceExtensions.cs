using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StackExchange.Redis;

namespace JaegerTracing
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder AddJaegerTracing(this IHostBuilder hostBuilder, params string[] sources)
        {
            string name = null;
            string host = null;
            int port = 0;
            ResourceBuilder resourceBuilder = null;
            hostBuilder.ConfigureLogging(((context, builder) =>
            {
                name = context.Configuration.GetValue<string>("Jaeger:ServiceName");
                host = context.Configuration.GetValue<string>("Jaeger:Host");
                port = context.Configuration.GetValue<int>("Jaeger:Port");

                if (name is {Length: > 0} && host is {Length: > 0} && port > 0)
                {
                    resourceBuilder = ResourceBuilder.CreateDefault().AddService(name);
                    builder.AddOpenTelemetry(options => { options.SetResourceBuilder(resourceBuilder); });
                }
            }));

            if (resourceBuilder is not null)
            {
                hostBuilder.ConfigureServices(services =>
                {
                    services.AddOpenTelemetryTracing(((provider, builder) =>
                    {
                        {
                            builder.SetResourceBuilder(resourceBuilder)
                                .AddAspNetCoreInstrumentation()
                                .AddHttpClientInstrumentation()
                                .AddGrpcClientInstrumentation();

                            if (sources.Length > 0)
                            {
                                builder.AddSource(sources);
                            }

                            if (provider.GetService<IConnectionMultiplexer>() is { } redis)
                            {
                                builder.AddRedisInstrumentation(redis);
                            }

                            builder.AddJaegerExporter(options =>
                            {
                                options.AgentHost = host;
                                options.AgentPort = port;
                            });
                        }
                    }));
                });
            }

            return hostBuilder;
        }
    }

    public static class OpenTelemetryServiceExtensions
    {
        public static IServiceCollection AddJaegerTracing(this IServiceCollection services,
            params string[] sources)
        {
            services.AddOpenTelemetryTracing(((provider, builder) =>
            {
                var config = provider.GetRequiredService<IConfiguration>();

                var name = config.GetValue<string>("Jaeger:ServiceName");
                var host = config.GetValue<string>("Jaeger:Host");
                var port = config.GetValue<int>("Jaeger:Port");

                if (name is {Length: > 0} && host is {Length: > 0} && port > 0)
                {
                    builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(name))
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddGrpcClientInstrumentation();

                    if (sources.Length > 0)
                    {
                        builder.AddSource(sources);
                    }

                    if (provider.GetService<IConnectionMultiplexer>() is { } redis)
                    {
                        builder.AddRedisInstrumentation(redis);
                    }

                    builder.AddJaegerExporter(options =>
                    {
                        options.AgentHost = host;
                        options.AgentPort = port;
                    });
                }
            }));

            return services;
        }
    }
}