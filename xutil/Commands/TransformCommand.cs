using Spectre.Console.Cli;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusi.Xml.Extras.Render;

namespace XUtil.Commands;

internal sealed class TransformCommand : AsyncCommand<TransformCommandSettings>
{
    private static string LoadText(string filePath)
    {
        using StreamReader reader = new(filePath, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public override Task<int> ExecuteAsync(CommandContext context,
        TransformCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[green]XSLT TRANSFORM[/]");
        AnsiConsole.MarkupLine($"Input: [cyan]{settings.InputFileMask}[/]");
        AnsiConsole.MarkupLine($"XSLT: [cyan]{settings.ScriptPath}[/]");
        AnsiConsole.MarkupLine($"Output: [cyan]{settings.OutputDirectory}[/]");

        if (!string.IsNullOrEmpty(settings.Extension))
        {
            Console.WriteLine($"Extension: {settings.Extension}\n");
            if (!settings.Extension.StartsWith(".", StringComparison.Ordinal))
                settings.Extension = "." + settings.Extension;
        }

        AnsiConsole.Status().Start("Applying transforms...", ctx =>
        {
            ctx.Spinner(Spinner.Known.Ascii);

            ctx.Status("Loading script");
            XsltTransformer transformer = new(LoadText(settings.ScriptPath));

            if (!Directory.Exists(settings.OutputDirectory))
                Directory.CreateDirectory(settings.OutputDirectory);

            List<string> files = Directory.GetFiles(
                Path.GetDirectoryName(settings.InputFileMask) ?? "",
                Path.GetFileName(settings.InputFileMask)!).OrderBy(s => s)
                .ToList();

            foreach (string fileIn in files)
            {
                ctx.Status($"[cyan]{fileIn}[/]");
                string ext = settings.Extension ?? Path.GetExtension(fileIn);

                string fileOut = Path.Combine(settings.OutputDirectory ?? "",
                    Path.GetFileNameWithoutExtension(fileIn) + ext);

                using StreamReader reader = new(fileIn, Encoding.UTF8);
                using StreamWriter writer = new(fileOut, false, Encoding.UTF8);
                transformer.Transform(reader, writer);
                writer.Flush();
            }
        });

        return Task.FromResult(0);
    }
}

internal class TransformCommandSettings : CommandSettings
{
    [CommandArgument(0, "<XSLT_PATH>")]
    [Description("The path to the XSLT file to run")]
    public string ScriptPath { get; set; }

    [CommandArgument(1, "<INPUT_FILES_MASK>")]
    [Description("The input file(s) mask")]
    public string InputFileMask { get; set; }

    [CommandArgument(2, "<OUTPUT_DIR>")]
    [Description("The output directory")]
    public string OutputDirectory { get; set; }

    [CommandOption("-x|--ext <EXTENSION>")]
    [Description("The file name extension (e.g. .txt) to set for the result file")]
    public string? Extension { get; set; }

    public TransformCommandSettings()
    {
        InputFileMask = "*.*";
        OutputDirectory = "";
        ScriptPath = "";
    }
}
