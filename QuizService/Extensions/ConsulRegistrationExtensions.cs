using Consul;

namespace QuizService.Extensions
{
    public static class ConsulRegistrationExtensions
    {
        public static IServiceCollection AddConsulClient(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var consulAddress = configuration["Consul:Address"] ?? "http://consul:8500";

            services.AddSingleton<IConsulClient, ConsulClient>(sp =>
                new ConsulClient(cfg =>
                {
                    cfg.Address = new Uri(consulAddress);
                }));

            return services;
        }

        public static IApplicationBuilder UseConsulRegistration(
            this IApplicationBuilder app,
            IConfiguration configuration,
            IHostApplicationLifetime lifetime)
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var logger       = app.ApplicationServices
                                   .GetRequiredService<ILogger<Program>>();

            var serviceId   = configuration["Consul:ServiceId"]   ?? "quizservice-1";
            var serviceName = configuration["Consul:ServiceName"] ?? "quizservice";
            var serviceHost = configuration["Consul:ServiceHost"] ?? "quizservice";
            var servicePort = int.Parse(configuration["Consul:ServicePort"] ?? "8080");

            var registration = new AgentServiceRegistration
            {
                ID      = serviceId,
                Name    = serviceName,
                Address = serviceHost,
                Port    = servicePort,
                Tags    = ["quiz", "api"],

                Check = new AgentServiceCheck
                {
                    HTTP                           = $"http://{serviceHost}:{servicePort}/health",
                    Interval                       = TimeSpan.FromSeconds(10),
                    Timeout                        = TimeSpan.FromSeconds(5),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30)
                }
            };

            lifetime.ApplicationStarted.Register(async () =>
            {
                try
                {
                    await consulClient.Agent.ServiceDeregister(serviceId);
                    await consulClient.Agent.ServiceRegister(registration);
                    logger.LogInformation(
                        "Consul: registered service '{Name}' (id={Id}) at {Host}:{Port}",
                        serviceName, serviceId, serviceHost, servicePort);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Consul: failed to register service '{Name}'.", serviceName);
                }
            });

            lifetime.ApplicationStopping.Register(async () =>
            {
                try
                {
                    await consulClient.Agent.ServiceDeregister(serviceId);
                    logger.LogInformation(
                        "Consul: deregistered service '{Name}' (id={Id})",
                        serviceName, serviceId);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Consul: failed to deregister service '{Name}'.", serviceName);
                }
            });

            return app;
        }
    }
}
