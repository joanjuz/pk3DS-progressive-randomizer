namespace pk3DS.WinForms;

public sealed class TrainerLevelCapRule
{
    public bool Enabled { get; set; } = true;
    public int TrainerID { get; set; }
    public string Group { get; set; } = string.Empty;
    public string Trainer { get; set; } = string.Empty;
    public int CurrentAceLevel { get; set; }

    // 0 means: use the trainer's current highest-level Pokémon as the cap.
    public int LevelCap { get; set; }

    public TrainerLevelCapRule Clone()
    {
        return new TrainerLevelCapRule
        {
            Enabled = Enabled,
            TrainerID = TrainerID,
            Group = Group,
            Trainer = Trainer,
            CurrentAceLevel = CurrentAceLevel,
            LevelCap = LevelCap,
        };
    }
}

public sealed class TrainerLevelCapStage
{
    public int TrainerID { get; set; }
    public int OriginalAceLevel { get; set; }
    public int LevelCap { get; set; }
}
