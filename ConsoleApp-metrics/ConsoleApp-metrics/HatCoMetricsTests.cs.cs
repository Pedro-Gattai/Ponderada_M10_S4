using System;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ConsoleApp_metrics.Tests
{
    public class HatCoMetricsTests
    {
        [Fact]
        public void HatsSold_IncrementsCounter()
        {
            // Arrange
            var services = CreateServiceProvider();
            var metrics = services.GetRequiredService<HatCoMetrics>();
            var meterFactory = services.GetRequiredService<IMeterFactory>();
            var collector = new MetricCollector<int>(meterFactory, "HatCo.Store", "hatco.store.hats_sold");

            // Act
            metrics.HatsSold(10);

            // Assert
            var measurements = collector.GetMeasurementSnapshot();
            Assert.Single(measurements);
            Assert.Equal(10, measurements[0].Value);
        }

        private static IServiceProvider CreateServiceProvider()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMetrics();
            serviceCollection.AddSingleton<HatCoMetrics>();
            return serviceCollection.BuildServiceProvider();
        }
    }

    public class MetricCollector<T>
    {
        private readonly IMeterFactory _meterFactory;
        private readonly string _namespace;
        private readonly string _metricName;

        public MetricCollector(IMeterFactory meterFactory, string ns, string metricName)
        {
            _meterFactory = meterFactory;
            _namespace = ns;
            _metricName = metricName;
        }

        public System.Collections.Generic.List<Measurement<T>> GetMeasurementSnapshot()
        {
            return new System.Collections.Generic.List<Measurement<T>>
            {
                new Measurement<T> { Value = default(T), Tags = new System.Collections.Generic.Dictionary<string, object>() }
            };
        }
    }

    public class Measurement<T>
    {
        public T Value { get; set; }
        public System.Collections.Generic.Dictionary<string, object> Tags { get; set; }
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMetrics(this IServiceCollection services)
        {
            services.AddSingleton<IMeterFactory, DefaultMeterFactory>();
            return services;
        }
    }

    public interface IMeterFactory
    {
        Meter Create(string name);
    }

    public class DefaultMeterFactory : IMeterFactory
    {
        public Meter Create(string name)
        {
            return new Meter(name);
        }
    }
}
