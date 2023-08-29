using Fusi.Tools.Text;
using Fusi.Xml.Schema;
using Fusi.Xml;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XUtil.Services;

namespace XUtil.Commands;

internal sealed class ValidateCommand : AsyncCommand<ValidateCommandSettings>
{
    private static IXmlValidator? CreateValidator(ValidateCommandSettings settings)
    {
        IXmlValidator validator;
        switch (Path.GetExtension(settings.SchemaPath).ToLowerInvariant())
        {
            case ".xsd":
            case ".xsdl":
                validator = new XsdValidator
                {
                    BaseUri = Path.GetDirectoryName(settings.SchemaPath),
                    IsDistinct = settings.IsDistinct
                };
                break;
            default:
                return null;
        }

        if (!string.IsNullOrEmpty(settings.FilterPath))
        {
            validator.MessageFilter = new TextReplacer(false);
            using StreamReader reader = new(settings.FilterPath, Encoding.UTF8);
            validator.MessageFilter.Load(reader);
        }

        using (StreamReader reader = new(settings.SchemaPath, Encoding.UTF8))
        {
            validator.SetSchema(reader);
        }
        return validator;
    }

    public override Task<int> ExecuteAsync(CommandContext context,
        ValidateCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[green]VALIDATE XML[/]");
        AnsiConsole.MarkupLine($"Schema path: [cyan]{settings.SchemaPath}[/]");
        AnsiConsole.MarkupLine($"Input files: [cyan]{settings.InputFileMask}[/]");

        IXmlValidator? validator = CreateValidator(settings)
            ?? throw new ArgumentException("Unknown schema type");

        int invalid = 0;
        List<string> files = Directory.EnumerateFiles(
            Path.GetDirectoryName(settings.InputFileMask) ?? "",
            Path.GetFileName(settings.InputFileMask) ?? "",
            settings.IsRecursive
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly)
            .OrderBy(s => s)
            .ToList();

        int n = 0, errCount = 0;
        foreach (string file in files)
        {
            AnsiConsole.MarkupLine($"{++n:000}. [yellow]{file}[/]");
            CliContext.Logger.LogInformation("Validating {Path}", file);
            using StreamReader reader = new(file, Encoding.UTF8);

            if (!validator.Validate(reader))
            {
                errCount += validator.Errors.Count;
                foreach (var error in validator.Errors)
                {
                    AnsiConsole.MarkupLine(
                        $"    [red]ERROR: {error.LineNumber},{error.ColumnNumber}: " +
                        $"{error.Message}[/]");

                    CliContext.Logger.LogError(
                        "{Y},{X}: {Message}", error.LineNumber, error.ColumnNumber,
                        $"{error.Message}[/]");
                }
                invalid++;
            }
            else
            {
                CliContext.Logger.LogInformation("{Path} is valid", file);
                AnsiConsole.MarkupLine("    [green]OK[/]");
            }
        }

        AnsiConsole.MarkupLine($"Invalid: {0} of {files.Count} (total errors: {errCount})");

        return Task.FromResult(0);
    }
}

internal class ValidateCommandSettings : CommandSettings
{
    [CommandArgument(1, "<SchemaPath>")]
    [Description("The path to the schema file")]
    public string SchemaPath { get; set; }

    [CommandArgument(1, "<InputFileMask>")]
    [Description("The input file(s) mask")]
    public string InputFileMask { get; set; }

    [CommandOption("--recurse|-r")]
    [Description("Recurse into subdirectories")]
    public bool IsRecursive { get; set; }

    [CommandOption("--distinct|-d")]
    [Description("Distinct error messages only")]
    public bool IsDistinct { get; set; }

    [CommandOption("--filter|-f")]
    [Description("The path to the filter file (when distinct is on)")]
    public string? FilterPath { get; set; }

    public ValidateCommandSettings()
    {
        SchemaPath = "";
        InputFileMask = "";
    }
}
