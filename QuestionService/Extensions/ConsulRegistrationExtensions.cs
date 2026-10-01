using Consul;

namespace QuestionService.Extensions
{
    public static class ConsulRegistrationExtensions
    {
        /// <summary>
        /// Đăng ký IConsulClient vào DI container.
        /// </summary>
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

        /// <summary>
        /// Đăng ký service lên Consul khi app khởi động
        /// và huỷ đăng ký (deregister) khi app tắt.
        /// </summary>
        public static IApplicationBuilder UseConsulRegistration(
            this IApplicationBuilder app,
            IConfiguration configuration,
            IHostApplicationLifetime lifetime)
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var logger       = app.ApplicationServices
                                   .GetRequiredService<ILogger<Program>>();

            // ── Đọc config ────────────────────────────────────────────────────
            var serviceId   = configuration["Consul:ServiceId"]   ?? "questionservice-1";
            var serviceName = configuration["Consul:ServiceName"] ?? "questionservice";
            var serviceHost = configuration["Consul:ServiceHost"] ?? "questionservice";
            var servicePort = int.Parse(configuration["Consul:ServicePort"] ?? "8080");

            var registration = new AgentServiceRegistration
            {
                ID      = serviceId,
                Name    = serviceName,
                Address = serviceHost,
                Port    = servicePort,
                Tags    = ["question", "api"],

                // Health-check: Consul gọi endpoint /health mỗi 10 giây
                Check = new AgentServiceCheck
                {
                    HTTP                           = $"http://{serviceHost}:{servicePort}/health",
                    Interval                       = TimeSpan.FromSeconds(10),
                    Timeout                        = TimeSpan.FromSeconds(5),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30)
                }
            };

            // ── Đăng ký khi app start ─────────────────────────────────────────
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
                        "Consul: failed to register service '{Name}'. " +
                        "Service discovery will be unavailable until Consul is reachable.",
                        serviceName);
                }
            });

            // ── Huỷ đăng ký khi app tắt ──────────────────────────────────────
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
