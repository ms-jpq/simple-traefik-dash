namespace STD

open DomainAgnostic
open DomainAgnostic.Globals
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System
open System.IO
open STD.Env
open STD.Services
open STD.Consts
open Microsoft.Extensions.Hosting

[<RequireQualifiedAccess>]
module Server =

    let private confLogging level (logging: ILoggingBuilder) =
        logging.AddFilter((<=) level) |> ignore
        logging.AddConsole() |> ignore


    let private confServices deps (globals: GlobalVar<'D>) (services: IServiceCollection) =
        services.AddSingleton(Container deps).AddSingleton(globals) |> ignore
        services.AddHostedService<PollingService>() |> ignore
        services.AddControllers() |> ignore

    let private confApp (app: IApplicationBuilder) =
        app.UseStatusCodePages().UseDeveloperExceptionPage() |> ignore
        app.UseRouting().UseEndpoints(fun endpoint -> endpoint.MapControllers() |> ignore) |> ignore

    let private confWebhost port (webhost: IWebHostBuilder) =
        webhost.UseKestrel() |> ignore
        webhost.UseUrls(sprintf "http://0.0.0.0:%d" port) |> ignore
        webhost.Configure(Action<IApplicationBuilder> confApp) |> ignore

    let Build<'D> (deps: Variables) (globals: GlobalVar<'D>) =
        let host = Host.CreateDefaultBuilder()
        host.UseContentRoot(CONTENTROOT) |> ignore
        host.ConfigureLogging(confLogging deps.logLevel) |> ignore
        host.ConfigureServices(confServices deps globals) |> ignore
        host.ConfigureWebHostDefaults(Action<IWebHostBuilder>(confWebhost deps.apiPort)) |> ignore
        host.Build()
