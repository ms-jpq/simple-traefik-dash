namespace STD

// open DomainAgnostic
// open DomainAgnostic.Globals
// open Microsoft.AspNetCore.Builder
// open Microsoft.AspNetCore.Hosting
// open Microsoft.AspNetCore.Routing
// open Microsoft.Extensions.DependencyInjection
// open Microsoft.Extensions.Logging
// open System
// open System.IO
// open STD.Services

// [<RequireQualifiedAccess>]
// module APISrv =

//     let private confLogging level (builder: ILoggingBuilder) = builder.AddFilter((<=) level).AddConsole() |> ignore


//     let private confJsonFormat (format: JsonSerializerSettings) =
//         format.MissingMemberHandling <- MissingMemberHandling.Error


//     let private confServices deps globals (services: IServiceCollection) =
//         services.AddSingleton(implementationInstance = Container(deps)).AddSingleton(implementationInstance = globals).AddHostedService<PollingService>
//             () |> ignore
//         services.AddMvcCore().AddJsonFormatters(Action<JsonSerializerSettings> confJsonFormat) |> ignore


//     let private confRouting (routes: IRouteBuilder) =
//         routes.MapRoute(name = "default", template = "{controller}/{action}") |> ignore


//     let private confApp (app: IApplicationBuilder) =
//         app.UseStatusCodePages("application/json", @"{{ ""error"": {0} }}").UseDeveloperExceptionPage()
//            .UseMvc(Action<IRouteBuilder> confRouting) |> ignore


//     let Build<'D> (deps: Variables) (globals: GlobalVar<'D>) =
//         WebHostBuilder().UseKestrel().UseUrls(sprintf "http://0.0.0.0:%d" deps.metricsPort)
//             .ConfigureLogging(confLogging deps.logLevel).ConfigureServices(confServices deps globals)
//             .Configure(Action<IApplicationBuilder> confApp).Build()
