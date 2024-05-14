# Instrumentando-metricas-console-app

## Passos para a execução:

1. No Visual Studio, foi criado o projeto do tipo "Console App".
   
![tipo_projeto](/Assets/tipo_projeto.png)

2. O projeto criado vem por padrão com um arquivo Program.cs.

   * O código do Program foi subtituido pelo código:
  
  ```
    using System;
    using System.Diagnostics.Metrics;
    using System.Threading;

    class Program
    {
        static Meter s_meter = new Meter("HatCo.Store");
        static Counter<int> s_hatsSold = s_meter.CreateCounter<int>("hatco.store.hats_sold");

        static void Main(string[] args)
        {
            Console.WriteLine("Projeto do João Executando");
            while (!Console.KeyAvailable)
            {
                Thread.Sleep(1000);
                s_hatsSold.Add(4);
            }
        }
    }
  
  ```

   * Esse código cria um contador de métricas para vendas de chapéus usando a biblioteca System.Diagnostics.Metrics. Ele inicia um medidor chamado "HatCo.Store" e define um contador s_hatsSold para rastrear a quantidade de chapéus vendidos. No método Main, o programa entra em um loop que a cada segundo adiciona quatro unidades ao contador s_hatsSold, enquanto não houver interrupção pelo usuário. Esse procedimento simula um cenário contínuo de vendas e serve para testar e monitorar o fluxo de dados de métricas

3. Em seguida foi instalado o Nuget System.Diagnostics.DiagnosticSource:

  ![tipo_projeto](/Assets/nuget_DiagnosticSource.png)

4. Nessa etapa o projeto foi executado com objetivo de verificar se a execução está conforme esperada

  ![tipo_projeto](/Assets/projeto_executando.png)


5. O próximo passo foi adicionar uma classe "HatCoMetrics" ao projeto, com o seguinte código:

```
    using System.Diagnostics.Metrics;

    namespace ConsoleApp_metrics
    {
        public class HatCoMetrics
        {
            private readonly Counter<int> _hatsSold;

            public HatCoMetrics(IMeterFactory meterFactory)
            {
                var meter = meterFactory.Create("HatCo.Store");
                _hatsSold = meter.CreateCounter<int>("hatco.store.hats_sold");
            }

            public void HatsSold(int quantity)
            {
                _hatsSold.Add(quantity);
            }
        }
    }
```

  * Esse código é responsável por gerenciar métricas de vendas de chapéus para a empresa HatCo. Ele utiliza a biblioteca System.Diagnostics.Metrics, a classe se integra a uma fábrica de medidores (IMeterFactory) para criar e gerenciar um contador de vendas de chapéus (_hatsSold). Este contador é incrementado através do método HatsSold(int quantity), que é chamado para adicionar a quantidade especificada de chapéus vendidos ao contador. 


6. Após isso já é possivel monitorar as métricas. Para conseguir obter os resultados das métricas é necessário que o código esteja em execução.

   * Descobrir o id do processo:

    ![id_processo](/Assets/id_processo.png)

    * Com o id do processo, o comando abaixo foi executado
  
    
    ```
        dotnet-counters monitor -p 22416 --counters "HatCo.Store"
    ```

    * Metricas geradas:
  
      ![metrica_01](/Assets/metricas_geradas.png)


### Coletando métricas com integração ao servidor:

Foi adicionado injeção de dependencia para a execução em singleton no container, o código modificado do Program está abaixo:


```

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
    hatCoMetrics.HatsSold(4);
    return "Metric Recorded";
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


```

 A inclusão de builder.Services.AddSingleton<HatCoMetrics>(); no método WebApplication.CreateBuilder(args) serve para registrar a classe HatCoMetrics como um serviço singleton no contêiner de injeção de dependência. Isso significa que uma única instância de HatCoMetrics será criada e reutilizada durante toda a vida útil da aplicação.

 Com essas mudanças no arquivo Program, ao entrar na porta 5000 localmente pelo brouser por exemplo, é feita uma requisição HTTP get, que incrementa 4 itens na lista de chapeus.

![localhost](/Assets/localhost.png)

![localhost](/Assets/contador_executando_com_local.png)

A cada requisição (refresh na pagina - f5), é adicionado 4 itens.

#### Adição de novas métricas:

* Metricas adicionadas:

1. Tempo de processamento do pedido
2. Número de casacos vendidos
3. Número de pedidos pendentes


Para adicionar novas métricas no código foi necessário modificar a classe HatCoMetrics, o código modificado está abaixo:

```
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

```

Também foram necessárias mudanças no arquivo Program:

```
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

```

Resultados obtidos:

![localhost](/Assets/resultados_novas_metricas.png)


## Testes Unitários:

1. Primeiramente foi criado um projeto do tipo xUnit para testar a aplicação:

![xunit](/Assets/xUnit_test.png)
