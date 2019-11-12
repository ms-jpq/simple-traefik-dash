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

module Entry =

    [<EntryPoint>]
    let main argv =
        echo README

        let deps = Opts()
        use state = new GlobalVar<int>(1)

        let srv =
            ((WebHostBuilder().UseKestrel().UseUrls(sprintf "http://0.0.0.0:%d" deps.apiPort)
                .ConfigureLogging(fun logging -> logging.AddConsole().AddFilter((<=) LogLevel.Information) |> ignore)
                .ConfigureServices
                (fun services ->
                (services.AddSingleton(Container(deps)).AddSingleton(state)).AddHostedService<PollingService>()
                    .AddControllersWithViews() |> ignore))
                .Configure
                (fun app ->
                app.UseStatusCodePages().UseDeveloperExceptionPage().UseStaticFiles().UseRouting()
                   .UseEndpoints((fun endpoint ->
                   endpoint.MapControllerRoute("default", "{controller=Home}/{action=Index}") |> ignore)) |> ignore))
                .Build()

        srv.Start()
        echo TEXTDIVIDER

        0
