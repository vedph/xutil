using Serilog;
using Spectre.Console.Cli;
using Spectre.Console;
using System.Diagnostics;
using XUtil.Commands;
using System.IO;
using System.Threading.Tasks;
using System;

namespace XUtil;

public static class Program
{
#if DEBUG
    private static void DeleteLogs()
    {
        foreach (var path in Directory.EnumerateFiles(
            AppDomain.CurrentDomain.BaseDirectory, "xutil-log*.txt"))
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
#endif

    public static async Task<int> Main(string[] args)
    {
        try
        {
            // https://github.com/serilog/serilog-sinks-file
            //string logFilePath = Path.Combine(
            //    Path.GetDirectoryName(
            //        Assembly.GetExecutingAssembly().Location) ?? "",
            //        "xutil-log.txt");
//            Log.Logger = new LoggerConfiguration()
//#if DEBUG
//                .MinimumLevel.Debug()
//#else
//                .MinimumLevel.Information()
//#endif
//                .Enrich.FromLogContext()
//                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
//                .CreateLogger();
#if DEBUG
            DeleteLogs();
#endif
            Stopwatch stopwatch = new();
            stopwatch.Start();

            CommandApp app = new();
            app.Configure(config =>
            {
                config.AddCommand<CheckWellFormedCommand>("check")
                    .WithDescription("Check well-formedness for the specified XML files.");
                config.AddCommand<ValidateCommand>("validate")
                    .WithDescription("Validate the specified XML files against a schema.");
                config.AddCommand<TransformCommand>("transform")
                    .WithDescription("Apply the specified XSLT 1.0 transformation " +
                    "to XML files.");
            });

            int result = await app.RunAsync(args);

            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                Console.WriteLine("\nTime: {0}h{1}'{2}\"",
                    stopwatch.Elapsed.Hours,
                    stopwatch.Elapsed.Minutes,
                    stopwatch.Elapsed.Seconds);
            }

            return result;
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, ex.Message);
            Debug.WriteLine(ex.ToString());
            AnsiConsole.WriteException(ex);
            return 2;
        }
    }
}
