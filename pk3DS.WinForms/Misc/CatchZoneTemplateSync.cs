using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace pk3DS.WinForms;

/// <summary>
/// Applies user-provided Gen 6 catch-zone template folders before the Wild/OWSE editors run.
/// The templates are intentionally external instead of embedded in the source tree because mapGR
/// GARCs are large binary assets.
/// </summary>
internal static class CatchZoneTemplateSync
{
    private const string TemplateRoot = "catch_zone_templates";

    private static readonly Dictionary<string, string[]> LegacyFolderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["encdata"] = ["encdata", "2_g", "3_g"],
        ["mapGR"] = ["mapGR", "1_g", "9_g"],
    };

    internal static string[] GetWildEditorFiles()
    {
        var files = new List<string> { "encdata" };
        if (HasTemplateForCurrentGame("mapGR"))
            files.Add("mapGR");
        return files.ToArray();
    }

    internal static string ApplyCurrentGameTemplates(params string[] logicalFolders)
    {
        if (!IsGen6XYOrORAS())
            return string.Empty;

        var report = new List<string>();
        foreach (string folder in logicalFolders.Where(f => !string.IsNullOrWhiteSpace(f)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (TryApplyTemplate(folder, out string message))
                report.Add(message);
        }

        return report.Count == 0 ? string.Empty : string.Join(Environment.NewLine, report);
    }

    internal static bool HasAnyTemplateForCurrentGame()
    {
        return IsGen6XYOrORAS()
            && (HasTemplateForCurrentGame("encdata")
                || HasTemplateForCurrentGame("mapGR")
                || HasRawTemplateForCurrentGame());
    }
    internal static string ApplyCurrentGameRawFiles(string romfsPath)
    {
        if (!IsGen6XYOrORAS() || string.IsNullOrWhiteSpace(romfsPath))
            return string.Empty;

        string source = GetRawTemplateSourceFolder();
        if (source.Length == 0)
            return string.Empty;

        try
        {
            int count = 0;

            foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                string relative = Path.GetRelativePath(source, file);

                if (relative.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(relative))
                    continue;

                string target = Path.Combine(romfsPath, relative);
                string parent = Path.GetDirectoryName(target);

                if (!string.IsNullOrEmpty(parent))
                    Directory.CreateDirectory(parent);

                File.Copy(file, target, true);
                count++;
            }

            return count == 0
                ? string.Empty
                : $"Applied raw catch-zone files: {GetGameFolderName()}/raw ({count.ToString(CultureInfo.InvariantCulture)} files)";
        }
        catch (Exception ex)
        {
            return $"Could not apply raw catch-zone files: {ex.Message}";
        }
    }

    private static bool TryApplyTemplate(string logicalFolder, out string message)
    {
        message = string.Empty;
        string source = GetTemplateSourceFolder(logicalFolder);
        if (source.Length == 0)
            return false;

        try
        {
            if (Directory.Exists(logicalFolder))
                Directory.Delete(logicalFolder, true);

            CopyDirectory(source, logicalFolder);
            int files = Directory.GetFiles(logicalFolder, "*", SearchOption.AllDirectories).Length;
            message = $"Applied catch-zone template: {GetGameFolderName()}/{logicalFolder} ({files.ToString(CultureInfo.InvariantCulture)} files)";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Could not apply catch-zone template for {logicalFolder}: {ex.Message}";
            return true;
        }
    }

    private static bool HasTemplateForCurrentGame(string logicalFolder) => GetTemplateSourceFolder(logicalFolder).Length != 0;
    private static bool HasRawTemplateForCurrentGame() => GetRawTemplateSourceFolder().Length != 0;

    private static string GetRawTemplateSourceFolder()
    {
        string game = GetGameFolderName();

        if (game.Length == 0)
            return string.Empty;

        string preferred = Path.Combine(TemplateRoot, game, "raw");

        if (Directory.Exists(preferred))
            return preferred;

        string flat = Path.Combine(TemplateRoot, game + "_raw");

        return Directory.Exists(flat) ? flat : string.Empty;
    }

    private static string GetTemplateSourceFolder(string logicalFolder)
    {
        string game = GetGameFolderName();
        if (game.Length == 0)
            return string.Empty;

        if (!LegacyFolderNames.TryGetValue(logicalFolder, out string[] candidates))
            candidates = [logicalFolder];

        // Preferred layout:
        // catch_zone_templates/XY/encdata
        // catch_zone_templates/XY/mapGR
        // catch_zone_templates/ORAS/encdata
        // catch_zone_templates/ORAS/mapGR
        foreach (string candidate in candidates)
        {
            string path = Path.Combine(TemplateRoot, game, candidate);
            if (Directory.Exists(path))
                return path;
        }

        // Also accept a flattened layout, useful while testing:
        // catch_zone_templates/XY_encdata, catch_zone_templates/ORAS_mapGR, etc.
        foreach (string candidate in candidates)
        {
            string path = Path.Combine(TemplateRoot, game + "_" + candidate);
            if (Directory.Exists(path))
                return path;
        }

        return string.Empty;
    }

    private static bool IsGen6XYOrORAS() => Main.Config?.XY == true || Main.Config?.ORAS == true;

    private static string GetGameFolderName()
    {
        if (Main.Config?.ORAS == true)
            return "ORAS";
        if (Main.Config?.XY == true)
            return "XY";
        return string.Empty;
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (string dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(source, dir);
            Directory.CreateDirectory(Path.Combine(destination, relative));
        }

        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(source, file);
            string target = Path.Combine(destination, relative);
            string parent = Path.GetDirectoryName(target);
            if (!string.IsNullOrEmpty(parent))
                Directory.CreateDirectory(parent);
            File.Copy(file, target, true);
        }
    }
}
