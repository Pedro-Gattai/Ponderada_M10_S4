# Documentação: Instrumentando Métricas em Aplicação Console

## Criação do Projeto

1. **Início do Projeto**: No Visual Studio, crie um novo projeto do tipo "Console App".
2. **Estrutura Inicial**: Por padrão, o projeto inclui um arquivo `Program.cs`.

## Implementação do Código

Substitua o código do arquivo `Program.cs` por:

```csharp
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
Este código implementa um contador de métricas para vendas de chapéus usando a biblioteca System.Diagnostics.Metrics. Ele inicia um medidor chamado "HatCo.Store" e define um contador s_hatsSold para rastrear a quantidade de chapéus vendidos. O programa entra em um loop que adiciona quatro unidades ao contador s_hatsSold a cada segundo, até que uma tecla seja pressionada.

- Faça a instalação de Dependências:
Instale o pacote NuGet System.Diagnostics.DiagnosticSource para habilitar as funcionalidades de diagnóstico avançado.

Adição de Classe para Métricas
Adicione uma nova classe HatCoMetrics ao projeto com o seguinte código:

```csharp

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
Esta classe gerencia métricas de vendas de chapéus para a empresa HatCo, utilizando a biblioteca System.Diagnostics.Metrics para criar e gerenciar o contador de vendas de chapéus.

Para monitorar as métricas em tempo real, execute o comando:

```bash
dotnet-counters monitor -p [process_id] --counters "HatCo.Store"
```
Substitua [process_id] pelo ID do processo da sua aplicação.

Integre a aplicação com um servidor HTTP para expor as métricas via requisições GET:

```csharp
Copiar código
using ConsoleApp_metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.Metrics;
using System.Threading;

var builder = WebOwner Application.CreateBuilder(args);
builder.Services.AddSingleton<HatCoMetrics>();

var app = builder.Build();

app.MapGet("/", (HatCoMetrics hatCoMetrics) =>
{
    hatCoMetrics.HatsSold(4);
    return "Metric Recorded";
});
app.Run();
```

Este código configura um servidor HTTP que registra 4 vendas de chapéus a cada requisição GET.

## Testes Unitários
Crie um projeto de teste do tipo xUnit para validar as funcionalidades da aplicação.
