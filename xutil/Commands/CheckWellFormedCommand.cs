using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using XUtil.Services;

namespace XUtil.Commands;

internal sealed class CheckWellFormedCommand :
    AsyncCommand<CheckWellFormedCommandSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context,
        CheckWellFormedCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[green]CHECK XML WELL-FORMEDNESS[/]");
        AnsiConsole.MarkupLine($"Input files: [cyan]{settings.InputFileMask}[/]");

        int count = 0;
        List<string> files = Directory.EnumerateFiles(
            Path.GetDirectoryName(settings.InputFileMask) ?? "",
            Path.GetFileName(settings.InputFileMask) ?? "",
            settings.IsRecursive
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly)
            .OrderBy(s => s)
            .ToList();

        foreach (string file in files)
        {
            AnsiConsole.MarkupLine($"{++count:000}. [yellow]{file}[/]");
            CliContext.Logger.LogInformation("Checking {Path}", file);

            try
            {
                XDocument.Load(file, LoadOptions.PreserveWhitespace);
                AnsiConsole.MarkupLine("    [green]OK[/]");
                CliContext.Logger.LogInformation("{Path} is well-formed", file);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"    [red]ERROR: {ex.Message}[/]");
                CliContext.Logger.LogError(ex, "{Path} has error: {Message}",
                    file, ex.Message);
            }
        }

        return Task.FromResult(0);
    }
}

internal class CheckWellFormedCommandSettings : CommandSettings
{
    [CommandArgument(0, "<InputFileMask>")]
    [Description("The input file(s) mask")]
    public string InputFileMask { get; set; }

    [CommandOption("--recurse|-r")]
    [Description("Recurse into subdirectories")]
    public bool IsRecursive { get; set; }

    public CheckWellFormedCommandSettings()
    {
        InputFileMask = "*.xml";
    }
}
