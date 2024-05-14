using ConsoleApp_metrics;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics.Metrics;

namespace ConsoleApp_metrics
{
    public class HatCoMetrics
    {
        private readonly Counter<int> _hatsSold;
        private readonly Histogram<double> _orderProcessingTime;
        private static int _coatsSold;
        private static int _ordersPending;
        private static Random _rand = new Random();

        public HatCoMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("HatCo.Store");
            _hatsSold = meter.CreateCounter<int>("hatco.store.hats_sold");
            _orderProcessingTime = meter.CreateHistogram<double>("hatco.store.order_processing_time");
            meter.CreateObservableCounter<int>("hatco.store.coats_sold", () => _coatsSold);
            meter.CreateObservableGauge<int>("hatco.store.orders_pending", () => _ordersPending);
        }

        public void HatsSold(int quantity)
        {
            _hatsSold.Add(quantity);
        }

        public void RecordOrderProcessingTime(double time)
        {
            _orderProcessingTime.Record(time);
        }

        public void SimulateCoatSale()
        {
            _coatsSold += 3;
        }

        public void SimulateOrderQueue()
        {
            _ordersPending = _rand.Next(0, 20);
        }

        public void SimulateMetrics()
        {
            HatsSold(4);
            SimulateCoatSale();
            SimulateOrderQueue();
            RecordOrderProcessingTime(_rand.Next(5, 15) / 1000.0); // Random time between 0.005 and 0.015 seconds
        }
    }
}





