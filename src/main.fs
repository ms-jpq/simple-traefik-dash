namespace STD

open DomainAgnostic
open Consts
open System
open STD.Services
open STD.Env
open DomainAgnostic.Globals
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open STD.Parsers.Traefik
open STD.Controllers
open System.IO

module Entry =

    [<EntryPoint>]
    let main argv =
        echo README

        let deps = Opts()
        use state = new GlobalVar<Route seq>(Seq.empty)
        use server =
            ((WebHostBuilder().UseKestrel().UseUrls(sprintf "http://localhost:%d" deps.apiPort)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging(fun logging -> logging.AddConsole().AddFilter((<=) LogLevel.Debug) |> ignore)
                .ConfigureServices
                (fun services ->
                services.AddSingleton(Container deps).AddSingleton(state).AddHostedService<PollingService>()
                    .AddControllers() |> ignore))
                .Configure
                (fun app ->
                app.UseStatusCodePages().UseDeveloperExceptionPage().UseStaticFiles().UseRouting()
                   .UseEndpoints((fun endpoint ->
                   endpoint.MapControllerRoute("default", "{controller=Entry}/{action=Index}") |> ignore)) |> ignore))
                .CaptureStartupErrors(true).Build()
        server.Start()
        echo TEXTDIVIDER

        0
