using System.Text.Json.Serialization;
using ChemDoserProxy.Configuration;
using ChemDoserProxy.Dto;
using ChemDoserProxy.Logic;
using ChemDoserProxy.State;
using ChemDoserProxy.Tcp;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddCommandLine(args)
    .AddEnvironmentVariables("DOSER_");

builder.Services
    .Configure<ProxySettings>(builder.Configuration.GetSection("proxy"))
    .Configure<ChemicalsSettings>(builder.Configuration.GetSection("chemicals"))
    .Configure<JsonOptions>(opts => opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter()))

    .AddHostedService<Listener>()
    .AddHostedService<DataFrameProcessor>()

    .AddSingleton<DataFrameQueue>()
    .AddSingleton<Forwarder>()

    .AddSingleton<StateManager>()
    .AddSingleton<ChemicalsManager>();

var app = builder.Build();
await app.Services.GetRequiredService<ChemicalsManager>().Initialize();

app.MapGet("/state", (HttpRequest req) => new
{
    state = req.HttpContext.RequestServices.GetRequiredService<StateManager>().State,
    levels = req.HttpContext.RequestServices.GetRequiredService<ChemicalsManager>().State,
});

app.MapPost("/chemical/{chemical}/fill", async (ChemicalType chemical, HttpRequest req) =>
{
    await req.HttpContext.RequestServices.GetRequiredService<ChemicalsManager>().RefillChemical(chemical);
});

await app.RunAsync();
