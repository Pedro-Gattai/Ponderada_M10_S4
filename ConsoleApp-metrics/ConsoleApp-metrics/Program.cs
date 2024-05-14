using ConsoleApp_metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.Metrics;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<HatCoMetrics>();

var app = builder.Build();

app.MapGet("/", (HatCoMetrics hatCoMetrics) =>
{
    hatCoMetrics.SimulateMetrics();
    return "Metrics Updated";
});

app.Run();

public partial class Program
{
    static Meter s_meter = new Meter("HatCo.Store");
    static Counter<int> s_hatsSold = s_meter.CreateCounter<int>("hatco.store.hats_sold");

    public static void Main(string[] args)
    {
        Console.WriteLine("Projeto do João Executando");
        while (!Console.KeyAvailable)
        {
            Thread.Sleep(1000);
            s_hatsSold.Add(4);
        }
    }
}
