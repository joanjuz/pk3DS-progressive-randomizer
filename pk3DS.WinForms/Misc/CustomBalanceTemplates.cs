using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace pk3DS.WinForms;

internal static class CustomBalanceTemplates
{
    internal const string TemplateRoot = "custom_balance_templates";

    internal sealed class MovePatchRow
    {
        public int Move { get; init; }
        public int? Power { get; init; }
        public int? Accuracy { get; init; }
        public int? PP { get; init; }
        public int? CriticalStage { get; init; }
        public int? Heal { get; init; }
        public int? Inflict { get; init; }
        public int? InflictChance { get; init; }
        public bool ClearStatEffects { get; init; }
        public bool KingShieldAttackMinusOne { get; init; }
    }

    internal sealed class EvolutionPatchRow
    {
        public int Source { get; init; }
        public int Target { get; init; }
        public string Method { get; init; } = string.Empty;
        public int Level { get; init; }
        public int Argument { get; init; }
        public sbyte Form { get; init; } = -1;
        public string ItemName { get; init; } = string.Empty;
        public string AltItemName { get; init; } = string.Empty;
    }

    internal static string GetMoveTemplatePath(int generation)
    => Path.Combine(GetTemplateRoot(), $"moves_gen{generation}.csv");

    internal static string GetEvolutionTemplatePath(int generation)
        => Path.Combine(GetTemplateRoot(), $"evolutions_gen{generation}.csv");

    internal static MovePatchRow[] LoadMovePatches(int generation, string[] moveNames)
    {
        string path = GetMoveTemplatePath(generation);
        if (!File.Exists(path))
            return [];

        var rows = new List<MovePatchRow>();

        foreach (var fields in ReadCsv(path))
        {
            if (fields.Length == 0)
                continue;

            string first = Get(fields, 0);
            if (first.Length == 0 || first.StartsWith('#'))
                continue;

            if (IsHeader(first, "Move"))
                continue;

            int move = ResolveId(first, moveNames);
            if (move <= 0)
                continue;

            rows.Add(new MovePatchRow
            {
                Move = move,
                Power = ParseNullableInt(Get(fields, 1)),
                Accuracy = ParseNullableInt(Get(fields, 2)),
                PP = ParseNullableInt(Get(fields, 3)),
                CriticalStage = ParseNullableInt(Get(fields, 4)),
                Heal = ParseNullableInt(Get(fields, 5)),
                Inflict = ParseNullableInt(Get(fields, 6)),
                InflictChance = ParseNullableInt(Get(fields, 7)),
                ClearStatEffects = ParseBool(Get(fields, 8)),
                KingShieldAttackMinusOne = ParseBool(Get(fields, 9)),
            });
        }

        return [.. rows];
    }
    private static string GetTemplateRoot()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        string[] candidates =
        [
            Path.Combine(baseDir, TemplateRoot),
        Path.Combine(Environment.CurrentDirectory, TemplateRoot),
        Path.Combine(AppContext.BaseDirectory, TemplateRoot),
    ];

        foreach (string candidate in candidates)
        {
            if (Directory.Exists(candidate))
                return candidate;
        }

        return Path.Combine(baseDir, TemplateRoot);
    }
    internal static EvolutionPatchRow[] LoadEvolutionPatches(int generation, string[] speciesNames)
    {
        string path = GetEvolutionTemplatePath(generation);
        if (!File.Exists(path))
            return [];

        var rows = new List<EvolutionPatchRow>();

        foreach (var fields in ReadCsv(path))
        {
            if (fields.Length == 0)
                continue;

            string first = Get(fields, 0);
            if (first.Length == 0 || first.StartsWith('#'))
                continue;

            if (IsHeader(first, "Source"))
                continue;

            int source = ResolveId(first, speciesNames);
            int target = ResolveId(Get(fields, 1), speciesNames);
            if (source <= 0 || target <= 0)
                continue;

            rows.Add(new EvolutionPatchRow
            {
                Source = source,
                Target = target,
                Method = Get(fields, 2),
                Level = ParseInt(Get(fields, 3)),
                Argument = ParseInt(Get(fields, 4)),
                Form = (sbyte)ParseInt(Get(fields, 5), -1),
                ItemName = Get(fields, 6),
                AltItemName = Get(fields, 7),
            });
        }

        return [.. rows];
    }

    internal static void WriteExampleTemplatesIfMissing()
    {
        string root = GetTemplateRoot(); 
        Directory.CreateDirectory(root);
        WriteIfMissing(Path.Combine(root, "moves_gen6.csv"), ExampleMoves());
        WriteIfMissing(Path.Combine(root, "moves_gen7.csv"), ExampleMoves());
        WriteIfMissing(Path.Combine(root, "evolutions_gen6.csv"), ExampleEvolutions());
        WriteIfMissing(Path.Combine(root, "evolutions_gen7.csv"), ExampleEvolutions());
    }

    private static string ExampleMoves() =>
        "Move,Power,Accuracy,PP,CriticalStage,Heal,Inflict,InflictChance,ClearStatEffects,KingShieldAttackMinusOne" + Environment.NewLine +
        "# You can use move IDs or exact move names. Empty cells mean keep current value." + Environment.NewLine +
        "15,70,100,15,1,,,,," + Environment.NewLine +
        "249,60,100,,,,,,," + Environment.NewLine +
        "588,,,7,,,,,false,true" + Environment.NewLine;

    private static string ExampleEvolutions() =>
        "Source,Target,Method,Level,Argument,Form,ItemName,AltItemName" + Environment.NewLine +
        "# Methods: Level, Friendship, MaleLevel, FemaleLevel, UsedItem, or a numeric method ID." + Environment.NewLine +
        "25,26,Level,30,,, ," + Environment.NewLine +
        "44,182,Friendship,,,,," + Environment.NewLine +
        "356,477,UsedItem,,,,Reaper Cloth,Tela Terrible" + Environment.NewLine;

    private static void WriteIfMissing(string path, string text)
    {
        if (File.Exists(path))
            return;

        File.WriteAllText(path, text);
    }

    private static IEnumerable<string[]> ReadCsv(string path)
    {
        foreach (string line in File.ReadLines(path))
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0)
                continue;

            yield return SplitCsvLine(line);
        }
    }

    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        bool quote = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (quote && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    quote = !quote;
                }
                continue;
            }

            if (c == ',' && !quote)
            {
                result.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        result.Add(current.ToString().Trim());
        return [.. result];
    }

    private static string Get(string[] fields, int index)
        => index < fields.Length ? fields[index].Trim() : string.Empty;

    private static bool IsHeader(string value, string header)
        => string.Equals(value, header, StringComparison.OrdinalIgnoreCase);

    private static int ResolveId(string value, string[] names)
    {
        value = value.Trim();
        if (value.Length == 0)
            return -1;

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
            return id;

        return Array.FindIndex(names, z => string.Equals(z, value, StringComparison.OrdinalIgnoreCase));
    }

    private static int ParseInt(string value, int fallback = 0)
        => int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : fallback;

    private static int? ParseNullableInt(string value)
        => int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : null;

    private static bool ParseBool(string value)
    {
        value = value.Trim();
        return value.Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || value.Equals("y", StringComparison.OrdinalIgnoreCase)
            || value.Equals("si", StringComparison.OrdinalIgnoreCase)
            || value.Equals("sí", StringComparison.OrdinalIgnoreCase);
    }
}
