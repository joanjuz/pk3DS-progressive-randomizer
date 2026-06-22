using pk3DS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using pk3DS.Core.Randomizers;

namespace pk3DS.WinForms;

public partial class TrainerRand : Form
{
    public TrainerRand(List<TrainerLevelCapRule> levelCapRules = null)
    {
        InitializeComponent();
        LevelCapRules = levelCapRules?.Select(r => r.Clone()).ToList() ?? [];
        AddProgressiveBSTControls();
        CB_Moves.SelectedIndex = 1;
        var trClassnorep = new List<string>();
        trClassnorep.AddRange(trClass.Where(tclass => !trClassnorep.Contains(tclass) && !tclass.StartsWith("[~")));
        trClassnorep.Sort();
        RandSettings.GetFormSettings(this, Controls);

    }
    private void ShowManualBSTDialog()
    {
        using var form = new Form
        {
            Text = "Manual Progressive BST Settings",
            StartPosition = FormStartPosition.CenterParent,
            Size = new System.Drawing.Size(560, 360),
            MinimizeBox = false,
            MaximizeBox = false,
            FormBorderStyle = FormBorderStyle.FixedDialog,
        };

        var editableRules = new System.ComponentModel.BindingList<ProgressiveBSTRule>(
            ProgressiveBSTRules.Select(r => r.Clone()).ToList()
        );

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            DataSource = editableRules,
            AllowUserToAddRows = true,
            AllowUserToDeleteRows = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
        };

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ProgressiveBSTRule.MinLevel),
            HeaderText = "Min Level",
            Width = 80,
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ProgressiveBSTRule.MaxLevel),
            HeaderText = "Max Level",
            Width = 80,
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ProgressiveBSTRule.MinBST),
            HeaderText = "Min BST",
            Width = 80,
        });

        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ProgressiveBSTRule.MaxBST),
            HeaderText = "Max BST",
            Width = 80,
        });

        grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            DataPropertyName = nameof(ProgressiveBSTRule.FullRandom),
            HeaderText = "Full Random",
            Width = 95,
        });

        grid.DataError += (_, e) =>
        {
            e.ThrowException = false;
        };

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 42,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(6),
        };

        var ok = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.None,
            Width = 80,
        };

        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Width = 80,
        };

        var reset = new Button
        {
            Text = "Reset Defaults",
            Width = 105,
        };

        reset.Click += (_, _) =>
        {
            editableRules.Clear();

            foreach (var rule in GetDefaultProgressiveBSTRules())
                editableRules.Add(rule);
        };

        ok.Click += (_, _) =>
        {
            grid.EndEdit();

            var candidateRules = editableRules
                .Select(r => r.Clone())
                .OrderBy(r => r.MinLevel)
                .ToList();

            if (!ValidateProgressiveBSTRules(candidateRules))
                return;

            ProgressiveBSTRules = candidateRules;

            form.DialogResult = DialogResult.OK;
            form.Close();
        };

        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(reset);

        form.Controls.Add(grid);
        form.Controls.Add(buttons);

        form.ShowDialog(this);
    }
    private static bool ValidateProgressiveBSTRules(List<ProgressiveBSTRule> rules)
    {
        if (rules.Count == 0)
        {
            WinFormsUtil.Alert("You must define at least one BST range.");
            return false;
        }

        foreach (var rule in rules)
        {
            if (rule.MinLevel < 1 || rule.MaxLevel > 100 || rule.MinLevel > rule.MaxLevel)
            {
                WinFormsUtil.Alert("Invalid level range detected. Levels must be between 1 and 100, and Min Level must be less than or equal to Max Level.");
                return false;
            }

            if (!rule.FullRandom && (rule.MinBST < 1 || rule.MaxBST > 999 || rule.MinBST > rule.MaxBST))
            {
                WinFormsUtil.Alert("Invalid BST range detected. BST must be between 1 and 999, and Min BST must be less than or equal to Max BST.");
                return false;
            }
        }

        for (int i = 1; i < rules.Count; i++)
        {
            if (rules[i].MinLevel <= rules[i - 1].MaxLevel)
            {
                WinFormsUtil.Alert("Overlapping level ranges detected. Please make sure level ranges do not overlap.");
                return false;
            }
        }

        return true;
    }

    private static List<ProgressiveBSTRule> GetDefaultProgressiveBSTRules()
    {
        return
        [
            new ProgressiveBSTRule { MinLevel = 1,  MaxLevel = 10,  MinBST = 180, MaxBST = 320, FullRandom = false },
        new ProgressiveBSTRule { MinLevel = 11, MaxLevel = 20,  MinBST = 220, MaxBST = 380, FullRandom = false },
        new ProgressiveBSTRule { MinLevel = 21, MaxLevel = 30,  MinBST = 280, MaxBST = 450, FullRandom = false },
        new ProgressiveBSTRule { MinLevel = 31, MaxLevel = 40,  MinBST = 340, MaxBST = 520, FullRandom = false },
        new ProgressiveBSTRule { MinLevel = 41, MaxLevel = 50,  MinBST = 400, MaxBST = 580, FullRandom = false },
        new ProgressiveBSTRule { MinLevel = 51, MaxLevel = 100, MinBST = 480, MaxBST = 680, FullRandom = false },
    ];
    }

    //private readonly string[] trName = Main.Config.GetText(TextName.TrainerNames);
    private readonly string[] trClass = Main.Config.GetText(TextName.TrainerClasses);
    //private readonly List<string> trClassnorep;
    private static readonly int[] Legendary = Legal.Legendary_6;
    private static readonly int[] Mythical = Legal.Mythical_6;

    private CheckBox CHK_ProgressiveBST;
    private Button B_SetManualBST;
    private CheckBox CHK_LevelCaps;
    private Button B_SetLevelCaps;

    private List<ProgressiveBSTRule> ProgressiveBSTRules = GetDefaultProgressiveBSTRules();
    private List<TrainerLevelCapRule> LevelCapRules = [];
    private bool ApplyCapsToPreviousTrainers = true;
    private int PreviousTrainerGap = 2;
    private decimal RegularTrainerCurvePower = 1.6m;
    private bool GuaranteeMegaInImportantBattles = false;


    private void B_Close_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void B_Save_Click(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Randomize all? Cannot undo.", "Double check Randomization settings before continuing.") != DialogResult.Yes)
            return;

        RSTE.rPKM = CHK_RandomPKM.Checked;
        RSTE.rSmart = CHK_BST.Checked && !CHK_ProgressiveBST.Checked;
        RSTE.rProgressiveBST = CHK_ProgressiveBST.Checked;
        RSTE.rProgressiveBSTRules = ProgressiveBSTRules
            .Select(r => r.Clone())
            .ToList();
        RSTE.rLevel = CHK_Level.Checked;
        RSTE.rLevelMultiplier = NUD_Level.Value;
        RSTE.rUseLevelCaps = CHK_LevelCaps.Checked;
        RSTE.rLevelCapRules = LevelCapRules.Select(r => r.Clone()).ToList();
        RSTE.rLevelCapsApplyPrevious = ApplyCapsToPreviousTrainers;
        RSTE.rLevelCapPreviousGap = PreviousTrainerGap;
        RSTE.rLevelCapCurvePower = RegularTrainerCurvePower;
        RSTE.rLevelCapsGuaranteeMega = GuaranteeMegaInImportantBattles;
        RSTE.rNoFixedDamage = CHK_NoFixedDamage.Checked;

        RSTE.rMove = CB_Moves.SelectedIndex == 1;
        RSTE.rNoMove = CB_Moves.SelectedIndex == 2;
        RSTE.rMetronome = CB_Moves.SelectedIndex == 3;
        if (RSTE.rMove)
        {
            RSTE.rDMG = CHK_Damage.Checked;
            if (RSTE.rDMG)
                RSTE.rDMGCount = (int)NUD_Damage.Value;
            RSTE.rSTAB = CHK_STAB.Checked;
            if (RSTE.rSTAB)
                RSTE.rSTABCount = (int)NUD_STAB.Value;
        }
        RSTE.rItem = CHK_RandomItems.Checked;
        RSTE.rAbility = CHK_RandomAbilities.Checked;
        RSTE.rDiffIV = CHK_MaxDiffPKM.Checked;

        RSTE.rClass = CHK_RandomClass.Checked;
        if (RSTE.rClass)
        {
            RSTE.rIgnoreClass = CHK_IgnoreSpecialClass.Checked
                ? Main.Config.ORAS
                    ? Legal.SpecialClasses_ORAS
                    : Legal.SpecialClasses_XY
                : [];
            RSTE.rOnlySingles = CHK_OnlySingles.Checked;
        }
        RSTE.rGift = CHK_RandomGift.Checked;
        RSTE.rGiftPercent = NUD_GiftPercent.Value;
        RSTE.rDiffAI = CHK_MaxDiffAI.Checked;
        RSTE.rTypeTheme = CHK_TypeTheme.Checked;
        RSTE.rTypeGymTrainers = CHK_GymTrainers.Checked;
        RSTE.rGymE4Only = CHK_GymE4Only.Checked;
        RSTE.rMinPKM = NUD_RMin.Value;
        RSTE.rMaxPKM = NUD_RMax.Value;
        RSTE.r6PKM = CHK_6PKM.Checked;
        RSTE.rRandomMegas = CHK_RandomMegaForm.Checked;
        RSTE.rForceFullyEvolved = CHK_ForceFullyEvolved.Checked;
        RSTE.rForceFullyEvolvedLevel = NUD_ForceFullyEvolved.Value;
        RSTE.rForceHighPower = CHK_ForceHighPower.Checked;
        RSTE.rForceHighPowerLevel = NUD_ForceHighPower.Value;

        if (CHK_StoryMEvos.Checked)
        {
            RSTE.rEnsureMEvo = Main.Config.ORAS
                ? [178, 235, 557, 583, 687, 698, 699, 700, 701, 713, 906, 907, 908, 909, 910, 911, 912, 913, 942, 944, 946,
                ]
                : [188, 263, 276, 277, 519, 520, 521, 526, 599, 600, 601];
        }
        else
        {
            RSTE.rEnsureMEvo = [];
        }

        RSTE.rThemedClasses = new bool[trClass.Length];
        RSTE.rSpeciesRand = new SpeciesRandomizer(Main.Config)
        {
            G1 = CHK_G1.Checked,
            G2 = CHK_G2.Checked,
            G3 = CHK_G3.Checked,
            G4 = CHK_G4.Checked,
            G5 = CHK_G5.Checked,
            G6 = CHK_G6.Checked,

            L = CHK_L.Checked,
            E = CHK_E.Checked,
            Shedinja = true,

            rBST = CHK_BST.Checked && !CHK_ProgressiveBST.Checked,
            rEXP = false,
        };
        RSTE.rSpeciesRand.Initialize();

        // add Legendary/Mythical to final evolutions if checked
        if (CHK_L.Checked) RSTE.rFinalEvo = [.. RSTE.rFinalEvo, .. Legendary];
        if (CHK_E.Checked) RSTE.rFinalEvo = [.. RSTE.rFinalEvo, .. Mythical];

        RSTE.rDoRand = true;
        RandSettings.SetFormSettings(this, Controls);
        Close();
    }
    private void ShowLevelCapDialog()
    {
        if (LevelCapRules.Count == 0)
        {
            WinFormsUtil.Alert("No important trainers were detected for this game.");
            return;
        }

        TrainerLevelCapDialog.Edit(
            this,
            ref LevelCapRules,
            ref ApplyCapsToPreviousTrainers,
            ref PreviousTrainerGap,
            ref RegularTrainerCurvePower,
            ref GuaranteeMegaInImportantBattles
        );
    }

    private void AddProgressiveBSTControls()
    {
        // Hacer la ventana más ancha para que quepa el botón manual.
        const int targetWidth = 560;

        MaximumSize = new System.Drawing.Size(700, 700);
        MinimumSize = new System.Drawing.Size(targetWidth, Height);

        ClientSize = new System.Drawing.Size(targetWidth, ClientSize.Height);

        // Ensanchar el grupo Options.
        GB_Tweak.Width = ClientSize.Width - GB_Tweak.Left - 16;

        // Mover OK y Cancel hacia la derecha.
        B_OK.Left = ClientSize.Width - B_OK.Width - 16;
        B_Cancel.Left = B_OK.Left;

        CHK_ProgressiveBST = new CheckBox
        {
            AutoSize = true,
            Location = new System.Drawing.Point(CHK_BST.Left, CHK_BST.Bottom + 6),
            Name = "CHK_ProgressiveBST",
            TabIndex = 999,
            Text = "Progressive BST",
            UseVisualStyleBackColor = true,
            Enabled = CHK_RandomPKM.Checked,
        };

        B_SetManualBST = new Button
        {
            Location = new System.Drawing.Point(CHK_ProgressiveBST.Right + 16, CHK_ProgressiveBST.Top - 3),
            Name = "B_SetManualBST",
            Size = new System.Drawing.Size(150, 23),
            TabIndex = 1000,
            Text = "Set BST manually",
            UseVisualStyleBackColor = true,
            Enabled = false,
        };

        CHK_ProgressiveBST.CheckedChanged += (_, _) =>
        {
            if (CHK_ProgressiveBST.Checked)
                CHK_BST.Checked = false;

            B_SetManualBST.Enabled = CHK_ProgressiveBST.Checked && CHK_RandomPKM.Checked;
        };

        CHK_BST.CheckedChanged += (_, _) =>
        {
            if (CHK_BST.Checked)
                CHK_ProgressiveBST.Checked = false;
        };

        B_SetManualBST.Click += (_, _) => ShowManualBSTDialog();

        GB_Tweak.Controls.Add(CHK_ProgressiveBST);
        GB_Tweak.Controls.Add(B_SetManualBST);

        CHK_ProgressiveBST.BringToFront();
        B_SetManualBST.BringToFront();

        CHK_LevelCaps = new CheckBox
        {
            AutoSize = true,
            Location = new System.Drawing.Point(220, CHK_Level.Top),
            Name = "CHK_LevelCaps",
            TabIndex = 1001,
            Text = "Level Caps",
            UseVisualStyleBackColor = true,
            Enabled = LevelCapRules.Count > 0,
        };

        B_SetLevelCaps = new Button
        {
            Location = new System.Drawing.Point(330, CHK_Level.Top - 3),
            Name = "B_SetLevelCaps",
            Size = new System.Drawing.Size(145, 23),
            TabIndex = 1002,
            Text = "Set level caps",
            UseVisualStyleBackColor = true,
            Enabled = false,
        };

        CHK_LevelCaps.CheckedChanged += (_, _) =>
        {
            B_SetLevelCaps.Enabled = CHK_LevelCaps.Checked && LevelCapRules.Count > 0;
        };

        B_SetLevelCaps.Click += (_, _) => ShowLevelCapDialog();

        Controls.Add(CHK_LevelCaps);
        Controls.Add(B_SetLevelCaps);
        CHK_LevelCaps.BringToFront();
        B_SetLevelCaps.BringToFront();

        int requiredTopWidth = B_SetLevelCaps.Right + 16;
        if (ClientSize.Width < requiredTopWidth)
            ClientSize = new System.Drawing.Size(requiredTopWidth, ClientSize.Height);

        int requiredGroupWidth = B_SetManualBST.Right + 12;

        if (GB_Tweak.Width < requiredGroupWidth)
            GB_Tweak.Width = requiredGroupWidth;

        int requiredFormWidth = GB_Tweak.Right + 16;

        if (ClientSize.Width < requiredFormWidth)
            ClientSize = new System.Drawing.Size(requiredFormWidth, ClientSize.Height);

        // Reacomodar de nuevo por si la ventana creció más.
        B_OK.Left = ClientSize.Width - B_OK.Width - 16;
        B_Cancel.Left = B_OK.Left;
    }


    private void CHK_RandomPKM_CheckedChanged(object sender, EventArgs e)
    {
        GB_Tweak.Enabled =
            CHK_G1.Checked = CHK_G2.Checked = CHK_G3.Checked =
                CHK_G4.Checked = CHK_G5.Checked = CHK_G6.Checked =
                    CHK_L.Checked = CHK_E.Checked = CHK_StoryMEvos.Checked = CHK_ForceFullyEvolved.Checked =
                        CHK_RandomPKM.Checked;

        CHK_TypeTheme.Checked = CHK_GymTrainers.Checked = CHK_GymE4Only.Checked =
            CHK_BST.Checked = CHK_6PKM.Checked = CHK_RandomMegaForm.Checked = false;

        if (CHK_ProgressiveBST is not null)
        {
            CHK_ProgressiveBST.Enabled = CHK_RandomPKM.Checked;
            CHK_ProgressiveBST.Checked = false;
        }

        if (B_SetManualBST is not null)
            B_SetManualBST.Enabled = CHK_RandomPKM.Checked && CHK_ProgressiveBST.Checked;
    }

    private void CHK_Level_CheckedChanged(object sender, EventArgs e)
    {
        NUD_Level.Enabled = CHK_Level.Checked;
    }

    private void ChangeLevelPercent(object sender, EventArgs e)
    {
        CHK_Level.Checked = NUD_Level.Value != 0;
    }

    private void CHK_RandomGift_CheckedChanged(object sender, EventArgs e)
    {
        NUD_GiftPercent.Enabled = CHK_RandomGift.Checked;
        NUD_GiftPercent.Value = Convert.ToDecimal(CHK_RandomGift.Checked) * 15;
    }

    private void ChangeGiftPercent(object sender, EventArgs e)
    {
        CHK_RandomGift.Checked = NUD_GiftPercent.Value != 0;
    }

    private void CHK_TypeTheme_CheckedChanged(object sender, EventArgs e)
    {
        CHK_GymTrainers.Enabled = CHK_GymTrainers.Checked = CHK_GymE4Only.Enabled = CHK_TypeTheme.Checked;
        if (!CHK_TypeTheme.Checked)
            CHK_GymTrainers.Checked = CHK_GymE4Only.Checked = false;
    }

    private void CHK_RandomClass_CheckedChanged(object sender, EventArgs e)
    {
        CHK_IgnoreSpecialClass.Enabled = CHK_IgnoreSpecialClass.Checked =
            CHK_OnlySingles.Enabled = CHK_OnlySingles.Checked = CHK_RandomClass.Checked;
    }

    private void ChangeMoveRandomization(object sender, EventArgs e)
    {
        CHK_Damage.Checked = CHK_STAB.Checked =
            CHK_Damage.Enabled = CHK_STAB.Enabled =
                NUD_Damage.Enabled = NUD_STAB.Enabled = CB_Moves.SelectedIndex == 1;

        CHK_ForceHighPower.Enabled = CHK_ForceHighPower.Checked = NUD_ForceHighPower.Enabled =
            CHK_NoFixedDamage.Enabled = CHK_NoFixedDamage.Checked = CB_Moves.SelectedIndex is 1 or 2;
    }
    private void CHK_6PKM_CheckedChanged(object sender, EventArgs e)
    {
        //if (CB_Moves.SelectedIndex == 0)
        //    CHK_6PKM.Checked = false;
    }
}