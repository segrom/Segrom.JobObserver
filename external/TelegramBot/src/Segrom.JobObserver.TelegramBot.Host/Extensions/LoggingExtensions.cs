using System.Reflection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Graylog;

namespace Segrom.JobObserver.TelegramBot.Host.Extensions;

public static class LoggingExtensions
{
	public static WebApplicationBuilder AddGraylog(this WebApplicationBuilder builder)
	{
		var cfg = builder.Configuration.GetSection("Graylog");
		var logger = new LoggerConfiguration().Enrich.FromLogContext()
			.WriteTo.Graylog(new GraylogSinkOptions
			{
				Facility = cfg.GetValue<string>("Facility") 
				           ?? Assembly.GetEntryAssembly()?.GetName().Name,
				HostnameOrAddress = cfg.GetValue<string>("Host")!,
				Port = cfg.GetValue<int>("Port"),
				TransportType = Serilog.Sinks.Graylog.Core.Transport.TransportType.Udp,
			})
			.WriteTo.Console()
			.MinimumLevel.Verbose()
			.MinimumLevel.Override( "Microsoft.AspNetCore", LogEventLevel.Warning )
			.MinimumLevel.Override( "Microsoft.Extensions.Hosting", LogEventLevel.Information )
			.MinimumLevel.Override( "Microsoft.Hosting", LogEventLevel.Information )
			.CreateLogger();
		Log.Logger = logger;
		builder.Services.AddSerilog(logger);
		builder.Host.UseSerilog(logger);
		return builder;
	}
}