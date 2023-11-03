using DataMapperGrpcExternalService.Services;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace DataMapperGrpcExternalService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                //if (OperatingSystem.IsWindows())
                //{
                //    serverOptions.ListenNamedPipe("MyPipeName");
                //}
                //else
                //{
                //    var socketPath = Path.Combine(Path.GetTempPath(), "socket.tmp");
                //    serverOptions.ListenUnixSocket(socketPath);
                //}

                serverOptions.ConfigureEndpointDefaults(listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http2;
                });
				var http2 = serverOptions.Limits.Http2;
				http2.InitialConnectionWindowSize = 1024 * 1024 * 2; // 2 MB
				http2.InitialStreamWindowSize = 1024 * 1024; // 1 MB
			});

            // Additional configuration is required to successfully run gRPC on macOS.
            // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

            // Add services to the container.
            builder.Services.AddGrpc();
            builder.Services.AddSingleton<DataMapperMappingService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.MapGrpcService<GreeterService>();
            app.MapGrpcService<DataMapperService>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

            app.Run();
        }
    }
}