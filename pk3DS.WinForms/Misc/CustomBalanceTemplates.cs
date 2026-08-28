using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace pk3DS.WinForms;

internal static class CustomBalanceTemplates
{
    internal const string TemplateRoot = "custom_balance_templates";

    internal sealed class MovePatchRow
    {
        public int Move { get; init; }
        public string Type { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string Quality { get; init; } = string.Empty;
        public int? Power { get; init; }
        public int? Accuracy { get; init; }
        public int? PP { get; init; }
        public int? Priority { get; init; }
        public int? HitMin { get; init; }
        public int? HitMax { get; init; }
        public int? CriticalStage { get; init; }
        public int? Flinch { get; init; }
        public int? Effect { get; init; }
        public int? Param0x0B { get; init; }
        public int? Heal { get; init; }
        public int? Recoil { get; init; }
        public int? TurnMin { get; init; }
        public int? TurnMax { get; init; }
        public string Targeting { get; init; } = string.Empty;
        public int? Inflict { get; init; }
        public string InflictToken { get; init; } = string.Empty;
        public int? InflictChance { get; init; }
        public bool ClearStatEffects { get; init; }
        public string UserStat { get; init; } = string.Empty;
        public int? UserStatChange { get; init; }
        public int? UserStatChance { get; init; }
        public string TargetStat { get; init; } = string.Empty;
        public int? TargetStatChange { get; init; }
        public int? TargetStatChance { get; init; }
        public string Stat1 { get; init; } = string.Empty;
        public int? Stat1Change { get; init; }
        public int? Stat1Chance { get; init; }
        public string Stat2 { get; init; } = string.Empty;
        public int? Stat2Change { get; init; }
        public int? Stat2Chance { get; init; }
        public string Stat3 { get; init; } = string.Empty;
        public int? Stat3Change { get; init; }
        public int? Stat3Chance { get; init; }
        public bool ClearFlags { get; init; }
        public string SetFlags { get; init; } = string.Empty;
        public string UnsetFlags { get; init; } = string.Empty;
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
        Dictionary<string, int> header = null;

        foreach (var fields in ReadCsv(path))
        {
            if (fields.Length == 0)
                continue;

            string first = Get(fields, 0);
            if (first.Length == 0 || first.StartsWith('#'))
                continue;

            if (IsHeader(first, "Move"))
            {
                header = BuildHeaderMap(fields);
                continue;
            }

            string moveToken = GetField(fields, header, 0, "Move");
            int move = ResolveId(moveToken, moveNames);
            if (move <= 0)
                continue;

            string inflict = GetField(fields, header, 6, "Inflict", "InflictStatus", "Status");

            rows.Add(new MovePatchRow
            {
                Move = move,
                Type = GetField(fields, header, -1, "Type"),
                Category = GetField(fields, header, -1, "Category", "DamageCategory"),
                Quality = GetField(fields, header, -1, "Quality", "MoveQuality"),
                Power = ParseNullableInt(GetField(fields, header, 1, "Power", "BasePower")),
                Accuracy = ParseNullableInt(GetField(fields, header, 2, "Accuracy", "Acc")),
                PP = ParseNullableInt(GetField(fields, header, 3, "PP")),
                Priority = ParseNullableInt(GetField(fields, header, -1, "Priority")),
                HitMin = ParseNullableInt(GetField(fields, header, -1, "HitMin", "MinHits")),
                HitMax = ParseNullableInt(GetField(fields, header, -1, "HitMax", "MaxHits")),
                CriticalStage = ParseNullableInt(GetField(fields, header, 4, "CriticalStage", "CritStage", "Critical", "Crit")),
                Flinch = ParseNullableInt(GetField(fields, header, -1, "Flinch", "FlinchChance")),
                Effect = ParseNullableInt(GetField(fields, header, -1, "Effect", "EffectID")),
                Param0x0B = ParseNullableInt(GetField(fields, header, -1, "Param0x0B", "0x0B", "Byte0B", "Extra0x0B", "Raw0x0B")),
                Heal = ParseNullableInt(GetField(fields, header, 5, "Heal", "Drain")),
                Recoil = ParseNullableInt(GetField(fields, header, -1, "Recoil")),
                TurnMin = ParseNullableInt(GetField(fields, header, -1, "TurnMin", "MinTurns")),
                TurnMax = ParseNullableInt(GetField(fields, header, -1, "TurnMax", "MaxTurns")),
                Targeting = GetField(fields, header, -1, "Targeting", "Target", "Targets"),
                Inflict = ParseNullableInt(inflict),
                InflictToken = int.TryParse(inflict, NumberStyles.Integer, CultureInfo.InvariantCulture, out _) ? string.Empty : inflict,
                InflictChance = ParseNullableInt(GetField(fields, header, 7, "InflictChance", "StatusChance", "EffectChance", "SecondaryChance")),
                ClearStatEffects = ParseBool(GetField(fields, header, 8, "ClearStatEffects", "ClearStats")),
                UserStat = GetField(fields, header, -1, "UserStat", "SelfStat"),
                UserStatChange = ParseNullableInt(GetField(fields, header, -1, "UserStatChange", "SelfStatChange")),
                UserStatChance = ParseNullableInt(GetField(fields, header, -1, "UserStatChance", "SelfStatChance")),
                TargetStat = GetField(fields, header, -1, "TargetStat", "FoeStat"),
                TargetStatChange = ParseNullableInt(GetField(fields, header, -1, "TargetStatChange", "FoeStatChange")),
                TargetStatChance = ParseNullableInt(GetField(fields, header, -1, "TargetStatChance", "FoeStatChance")),
                Stat1 = GetField(fields, header, -1, "Stat1"),
                Stat1Change = ParseNullableInt(GetField(fields, header, -1, "Stat1Change")),
                Stat1Chance = ParseNullableInt(GetField(fields, header, -1, "Stat1Chance")),
                Stat2 = GetField(fields, header, -1, "Stat2"),
                Stat2Change = ParseNullableInt(GetField(fields, header, -1, "Stat2Change")),
                Stat2Chance = ParseNullableInt(GetField(fields, header, -1, "Stat2Chance")),
                Stat3 = GetField(fields, header, -1, "Stat3"),
                Stat3Change = ParseNullableInt(GetField(fields, header, -1, "Stat3Change")),
                Stat3Chance = ParseNullableInt(GetField(fields, header, -1, "Stat3Chance")),
                ClearFlags = ParseBool(GetField(fields, header, -1, "ClearFlags")),
                SetFlags = GetField(fields, header, -1, "SetFlags", "Flags", "AddFlags"),
                UnsetFlags = GetField(fields, header, -1, "UnsetFlags", "RemoveFlags"),
                KingShieldAttackMinusOne = ParseBool(GetField(fields, header, 9, "KingShieldAttackMinusOne")),
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
        "Move,Type,Category,Quality,Power,Accuracy,PP,Priority,HitMin,HitMax,CriticalStage,Flinch,Effect,Param0x0B,Inflict,InflictChance,Heal,Recoil,TurnMin,TurnMax,Targeting,ClearStatEffects,UserStat,UserStatChange,UserStatChance,TargetStat,TargetStatChange,TargetStatChance,Stat1,Stat1Change,Stat1Chance,Stat2,Stat2Change,Stat2Chance,Stat3,Stat3Change,Stat3Chance,ClearFlags,SetFlags,UnsetFlags,KingShieldAttackMinusOne,Notes" + Environment.NewLine +
        "# Move can be an ID or exact move name. Empty cells keep the current value." + Environment.NewLine +
        "# Type/Category/Targeting/Inflict/Stats accept IDs or names. Flags accept enum names, numeric masks, or tokens separated by ; | ," + Environment.NewLine +
        "15,Grass,Physical,,70,100,15,,,,1,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,Cut / Corte" + Environment.NewLine +
        "249,Fighting,Physical,,60,100,,,,,,,,,,,,,,,,,,,Defense,-1,100,,,,,,,,,,,,Rock Smash / Golpe Roca" + Environment.NewLine +
        "19,Flying,Physical,,130,100,,,,,,,,,,,,33,,,,,,,,,,,,,,,,,,,,,Fly / Vuelo" + Environment.NewLine +
        "147,,Status,,,80,,,,,,,,Sleep,100,,,,,,,,,,,,,,,,,,,,,,,,Spore / Espora" + Environment.NewLine +
        "79,,Status,,,70,,,,,,,,Sleep,100,,,,,,,,,,,,,,,,,,,,,,,,Sleep Powder / Somnifero" + Environment.NewLine +
        "601,,Status,,,,,-1,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,Geomancy / Geocontrol priority -1" + Environment.NewLine +
        "6,,Physical,,25,,40,,,,,40,,,,,,,,,,,,,,,,,,,,,,,,,,,,,Pay Day / Dia de Pago" + Environment.NewLine +
        "594,Water,Special,,30,100,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,Water Shuriken / Shuriken de Agua" + Environment.NewLine +
        "588,,Status,,,5,,,,,,,,,,,,,,,,true,,,,,,,,,,,,,,,,,,true,King's Shield PP 5 and -1 Attack" + Environment.NewLine;

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
        var current = new StringBuilder();
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

    private static Dictionary<string, int> BuildHeaderMap(string[] fields)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < fields.Length; i++)
        {
            string key = NormalizeToken(fields[i]);
            if (key.Length != 0 && !result.ContainsKey(key))
                result.Add(key, i);
        }
        return result;
    }

    private static string GetField(string[] fields, Dictionary<string, int> header, int fallbackIndex, params string[] names)
    {
        if (header is not null)
        {
            foreach (string name in names)
            {
                if (header.TryGetValue(NormalizeToken(name), out int index))
                    return Get(fields, index);
            }
            return string.Empty;
        }

        return fallbackIndex >= 0 ? Get(fields, fallbackIndex) : string.Empty;
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

    internal static int ResolveToken(string value, string[] names, Dictionary<string, int> aliases = null)
    {
        value = value.Trim();
        if (value.Length == 0)
            return -1;

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
            return id;

        string normalized = NormalizeToken(value);
        if (aliases is not null && aliases.TryGetValue(normalized, out int alias))
            return alias;

        return Array.FindIndex(names, z => NormalizeToken(z) == normalized);
    }

    internal static IEnumerable<string> SplitTokens(string value)
        => value.Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    internal static string NormalizeToken(string value)
    {
        if (value is null)
            return string.Empty;

        string lower = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(lower.Length);
        foreach (char c in lower)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(c))
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
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
