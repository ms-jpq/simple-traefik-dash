namespace STD

open DomainAgnostic
open DomainAgnostic.Globals
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System
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
        services.AddSingleton(Container deps) |> ignore
        services.AddSingleton(globals) |> ignore
        services.AddHostedService<PollingService>() |> ignore
        services.AddControllers() |> ignore


    let private confApp baseUri (app: IApplicationBuilder) =
        app.UseStatusCodePages().UseDeveloperExceptionPage() |> ignore
        app.UsePathBase(baseUri) |> ignore
        app.UseStaticFiles() |> ignore
        app.UseRouting() |> ignore
        app.UseCors() |> ignore
        app.UseEndpoints(fun ep -> ep.MapControllers() |> ignore) |> ignore


    let private confWebhost deps gloabls (webhost: IWebHostBuilder) =
        webhost.UseWebRoot(RESOURCESDIR) |> ignore
        webhost.UseKestrel() |> ignore
        webhost.UseUrls(sprintf "http://0.0.0.0:%d" deps.port) |> ignore
        webhost.ConfigureServices(confServices deps gloabls) |> ignore
        webhost.Configure(Action<IApplicationBuilder>(confApp deps.baseUri)) |> ignore


    let Build<'D> (deps: Variables) (globals: GlobalVar<'D>) =
        let host = Host.CreateDefaultBuilder()
        host.UseContentRoot(CONTENTROOT) |> ignore
        host.ConfigureLogging(confLogging deps.logLevel) |> ignore
        host.ConfigureWebHostDefaults(Action<IWebHostBuilder>(confWebhost deps globals)) |> ignore
        host.Build()
