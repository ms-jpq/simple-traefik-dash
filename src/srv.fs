namespace STD

open STD.Services
open STD.Env
open DomainAgnostic
open DomainAgnostic.Globals
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Routing
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System

[<RequireQualifiedAccess>]
module Server =

    let private confLogging level (builder: ILoggingBuilder) = builder.AddFilter((<=) level).AddConsole() |> ignore


    let private confServices deps globals (services: IServiceCollection) =
        services.AddSingleton(implementationInstance = Container(deps)).AddSingleton(implementationInstance = globals).AddHostedService<PollingService>
            () |> ignore
        services.AddMvcCore() |> ignore

    let private confRouting (routes: IRouteBuilder) =
        routes.MapRoute(name = "default", template = "{controller}/{action}") |> ignore


    let private confApp (app: IApplicationBuilder) =
        app.UseStatusCodePages("application/json", @"{{ ""error"": {0} }}").UseDeveloperExceptionPage()
           .UseMvc(Action<IRouteBuilder> confRouting) |> ignore


    let Build<'D> (deps: Variables) (globals: GlobalVar<'D>) =
        WebHostBuilder().UseKestrel().UseUrls(sprintf "http://0.0.0.0:%d" deps.apiPort)
            .ConfigureLogging(confLogging deps.logLevel).ConfigureServices(confServices deps globals)
            .Configure(Action<IApplicationBuilder> confApp).Build()
