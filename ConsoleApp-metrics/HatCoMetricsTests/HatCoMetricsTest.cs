using ConsoleApp_metrics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.Metrics;

namespace HatCoMetricsTests
{
    [TestClass]
    public class HatCoMetricsTest
    {
        private HatCoMetrics? _hatCoMetrics;
        private MockMeterFactory? _meterFactory;

        [TestInitialize]
        public void Setup()
        {
            _meterFactory = new MockMeterFactory();
            _hatCoMetrics = new HatCoMetrics(_meterFactory);
        }

        [TestMethod]
        public void HatsSold_IncrementsCounter()
        {
            // Arrange
            int initialHatsSold = _meterFactory.HatsSoldCount;
            int quantity = 5;

            // Act
            _hatCoMetrics.HatsSold(quantity);

            // Assert
            Assert.AreEqual(initialHatsSold + quantity, _meterFactory.HatsSoldCount, "Hats sold counter did not increment correctly.");
        }

        [TestMethod]
        public void RecordOrderProcessingTime_RecordsValue()
        {
            // Arrange
            double processingTime = 0.01;

            // Act
            _hatCoMetrics.RecordOrderProcessingTime(processingTime);

            // Assert
            Assert.AreEqual(processingTime, _meterFactory.LastProcessingTime, "Order processing time was not recorded correctly.");
        }

        [TestMethod]
        public void SimulateCoatSale_IncrementsCoatsSold()
        {
            // Arrange
            int initialCoatsSold = _meterFactory.CoatsSoldCount;
            int expectedIncrement = 3;

            // Act
            _hatCoMetrics.SimulateCoatSale();

            // Assert
            Assert.AreEqual(initialCoatsSold + expectedIncrement, _meterFactory.CoatsSoldCount, "Coats sold did not increment correctly.");
        }

        [TestMethod]
        public void SimulateOrderQueue_UpdatesOrdersPending()
        {
            // Act
            _hatCoMetrics.SimulateOrderQueue();
            int ordersPending = _meterFactory.OrdersPendingCount;

            // Assert
            Assert.IsTrue(ordersPending >= 0 && ordersPending < 20, "Orders pending is not within the expected range.");
        }

        [TestMethod]
        public void SimulateMetrics_UpdatesAllMetrics()
        {
            // Arrange
            int initialHatsSold = _meterFactory.HatsSoldCount;
            int initialCoatsSold = _meterFactory.CoatsSoldCount;

            // Act
            _hatCoMetrics.SimulateMetrics();

            // Assert
            Assert.AreEqual(initialHatsSold + 4, _meterFactory.HatsSoldCount, "Hats sold did not increment correctly.");
            Assert.AreEqual(initialCoatsSold + 3, _meterFactory.CoatsSoldCount, "Coats sold did not increment correctly.");
            Assert.IsTrue(_meterFactory.OrdersPendingCount >= 0, "Orders pending is not updated correctly.");
            Assert.IsTrue(_meterFactory.LastProcessingTime >= 0, "Order processing time is not recorded correctly.");
        }
    }

    public class MockMeterFactory : IMeterFactory
    {
        public int HatsSoldCount { get; private set; }
        public double LastProcessingTime { get; private set; }
        public int CoatsSoldCount { get; private set; }
        public int OrdersPendingCount { get; private set; }

        public Meter Create(string name)
        {
            return new Meter(name);
        }

        public Counter<int> CreateCounter(string name)
        {
            return new MockCounter(this);
        }

        public Histogram<double> CreateHistogram(string name)
        {
            return new MockHistogram(this);
        }

        public ObservableCounter<int> CreateObservableCounter(string name, Func<int> valueProvider)
        {
            return new MockObservableCounter(this, valueProvider);
        }

        public ObservableGauge<int> CreateObservableGauge(string name, Func<int> valueProvider)
        {
            return new MockObservableGauge(this, valueProvider);
        }

        private class MockCounter : Counter<int>
        {
            private readonly MockMeterFactory _factory;

            public MockCounter(MockMeterFactory factory)
                : base(new Meter("Mock"), "MockCounter")
            {
                _factory = factory;
            }

            public void Add(int value)
            {
                _factory.HatsSoldCount += value;
            }
        }

        private class MockHistogram : Histogram<double>
        {
            private readonly MockMeterFactory _factory;

            public MockHistogram(MockMeterFactory factory)
                : base(new Meter("Mock"), "MockHistogram")
            {
                _factory = factory;
            }

            public void Record(double value)
            {
                _factory.LastProcessingTime = value;
            }
        }

        private class MockObservableCounter : ObservableCounter<int>
        {
            private readonly MockMeterFactory _factory;
            private readonly Func<int> _valueProvider;

            public MockObservableCounter(MockMeterFactory factory, Func<int> valueProvider)
                : base(new Meter("Mock"), "MockObservableCounter", valueProvider)
            {
                _factory = factory;
                _valueProvider = valueProvider;
            }
        }

        private class MockObservableGauge : ObservableGauge<int>
        {
            private readonly MockMeterFactory _factory;
            private readonly
