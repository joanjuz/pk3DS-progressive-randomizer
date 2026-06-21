using pk3DS.Core;
using System;
using System.Linq;
using System.Windows.Forms;
using pk3DS.Core.Structures;

namespace pk3DS.WinForms;

public partial class MoveEditor7 : Form
{
    public MoveEditor7(byte[][] infiles)
    {
        files = infiles;
        movelist[0] = "";
        InitializeComponent();
        Setup();

        AddBalanceMovesButton();
        FixMoveEditorOptionsLayout();
        FixMoveFlagsListLayout();

        RandSettings.GetFormSettings(this, groupBox1.Controls);
    }

    private readonly byte[][] files;
    private Button B_BalanceMoves;
    private readonly string[] types = Main.Config.GetText(TextName.Types);
    private readonly string[] moveflavor = Main.Config.GetText(TextName.MoveFlavor);
    private readonly string[] movelist = Main.Config.GetText(TextName.MoveNames);
    private readonly string[] MoveCategories = ["Status", "Physical", "Special"];
    private readonly string[] StatCategories = ["None", "Attack", "Defense", "Special Attack", "Special Defense", "Speed", "Accuracy", "Evasion", "All",
    ];

    private void AddBalanceMovesButton()
    {
        const int gap = 6;

        B_BalanceMoves = new Button
        {
            Location = new System.Drawing.Point(B_Metronome.Left, B_Metronome.Bottom + gap),
            Name = "B_BalanceMoves",
            Size = new System.Drawing.Size(B_Metronome.Width, 23),
            TabIndex = 999,
            Text = "Balance moves",
            UseVisualStyleBackColor = true,
        };

        B_BalanceMoves.Click += B_BalanceMoves_Click;

        Controls.Add(B_BalanceMoves);
        B_BalanceMoves.BringToFront();

        // Mueve el grupo Options un poco hacia abajo para que no se solape con el nuevo botón.
        int requiredTop = B_BalanceMoves.Bottom + gap;

        if (groupBox1.Top < requiredTop)
            groupBox1.Top = requiredTop;
    }

    private void FixMoveEditorOptionsLayout()
    {
        const int gap = 6;

        B_Table.Location = new System.Drawing.Point(
            B_Table.Left,
            B_Table.Top + 22
        );

        int requiredHeight = B_Table.Bottom + 10;

        if (groupBox1.Height < requiredHeight)
            groupBox1.Height = requiredHeight;
    }
    private void B_BalanceMoves_Click(object sender, EventArgs e)
    {
        if (DialogResult.Yes != WinFormsUtil.Prompt(
            MessageBoxButtons.YesNo,
            "Balance moves?",
            "This will apply the custom move changes from the PDF. Cannot undo."))
        {
            return;
        }

        SetEntry();

        int changed = ApplyBalancedMoves();

        GetEntry();

        WinFormsUtil.Alert(
            "Moves balanced!",
            $"{changed} moves were updated.");
    }
    private sealed class MoveBalancePatch
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
    private int ApplyBalancedMoves()
    {
        int changed = 0;

        foreach (var patch in GetBalancedMovePatches())
        {
            if (patch.Move <= 0 || patch.Move >= files.Length)
                continue;

            byte[] data = files[patch.Move];

            if (data.Length < 0x1E)
                continue;

            if (patch.Power.HasValue)
                data[0x03] = (byte)patch.Power.Value;

            if (patch.Accuracy.HasValue)
                data[0x04] = (byte)patch.Accuracy.Value;

            if (patch.PP.HasValue)
                data[0x05] = (byte)patch.PP.Value;

            if (patch.CriticalStage.HasValue)
                data[0x0E] = (byte)patch.CriticalStage.Value;

            if (patch.Heal.HasValue)
                data[0x13] = (byte)patch.Heal.Value;

            if (patch.Inflict.HasValue)
                Array.Copy(BitConverter.GetBytes((short)patch.Inflict.Value), 0, data, 0x08, 2);

            if (patch.InflictChance.HasValue)
                data[0x0A] = (byte)patch.InflictChance.Value;

            if (patch.ClearStatEffects)
                ClearMoveStatEffects(data);

            if (patch.KingShieldAttackMinusOne)
                SetKingShieldAttackDrop(data);

            files[patch.Move] = data;
            changed++;
        }

        return changed;
    }
    private static void ClearMoveStatEffects(byte[] data)
    {
        // Stat categories: None, Attack, Defense, Sp. Atk, Sp. Def, Speed, Accuracy, Evasion, All.
        data[0x15] = 0;
        data[0x16] = 0;
        data[0x17] = 0;

        // Stat stages.
        data[0x18] = 0;
        data[0x19] = 0;
        data[0x1A] = 0;

        // Stat effect chances.
        data[0x1B] = 0;
        data[0x1C] = 0;
        data[0x1D] = 0;
    }

    private static void SetKingShieldAttackDrop(byte[] data)
    {
        ClearMoveStatEffects(data);

        // Attack = index 1 in StatCategories.
        data[0x15] = 1;

        // -1 Attack.
        data[0x18] = unchecked((byte)-1);

        // 100% chance.
        data[0x1B] = 100;
    }
    private static MoveBalancePatch[] GetBalancedMovePatches()
    {
        const int Confusion = 6;

        return
        [
            // Página 1 del PDF
            // Corte: 70 power, 100 accuracy, 15 PP, critical stage 1.
            new MoveBalancePatch { Move = 15, Power = 70, Accuracy = 100, PP = 15, CriticalStage = 1 },

        // Golpe Roca
        new MoveBalancePatch { Move = 249, Power = 60, Accuracy = 100 },

        // Vuelo
        new MoveBalancePatch { Move = 19, Power = 100, Accuracy = 100 },

        // Espora
        new MoveBalancePatch { Move = 147, Accuracy = 85 },

        // Deslumbrar
        new MoveBalancePatch { Move = 137, Accuracy = 90, PP = 15 },

        // Torm. Diamantes / Diamond Storm: remover boost secundario.
        new MoveBalancePatch { Move = 591, Power = 95, Accuracy = 95, PP = 10, ClearStatEffects = true },

        // Ala Mortífera / Oblivion Wing
        new MoveBalancePatch { Move = 613, Power = 80, Accuracy = 100, Heal = 50 },

        // Cháchara / Chatter
        new MoveBalancePatch { Move = 448, Power = 80, Accuracy = 100, PP = 15, Inflict = Confusion, InflictChance = 15 },

        // Chupavidas / Leech Life
        new MoveBalancePatch { Move = 141, Power = 70, Accuracy = 100, PP = 15 },

        // Bomba Fango / Sludge Bomb
        new MoveBalancePatch { Move = 188, Power = 60, Accuracy = 100 },

        // Danza Llama / Fire Spin
        new MoveBalancePatch { Move = 552, Power = 70, Accuracy = 100 },

        // Shuriken de Agua / Water Shuriken: 20 por golpe.
        new MoveBalancePatch { Move = 594, Power = 20, Accuracy = 100 },

        // PP 7
        new MoveBalancePatch { Move = 182, PP = 7 }, // Protección / Protect
        new MoveBalancePatch { Move = 197, PP = 7 }, // Detección / Detect
        new MoveBalancePatch { Move = 596, PP = 7 }, // Barrera Espinosa / Spiky Shield
        new MoveBalancePatch { Move = 588, PP = 7, KingShieldAttackMinusOne = true }, // Escudo Real / King's Shield
        new MoveBalancePatch { Move = 476, PP = 7 }, // Polvo Ira / Rage Powder
        new MoveBalancePatch { Move = 266, PP = 7 }, // Señuelo / Follow Me
        new MoveBalancePatch { Move = 502, PP = 7 }, // Cambio Banda / Ally Switch
        new MoveBalancePatch { Move = 73,  PP = 7 }, // Drenadoras / Leech Seed

        // Página 2 del PDF
        new MoveBalancePatch { Move = 281, PP = 7 }, // Bostezo / Yawn

        // PP 5
        new MoveBalancePatch { Move = 270, PP = 5 }, // Refuerzo / Helping Hand
        new MoveBalancePatch { Move = 105, PP = 5 }, // Recuperación / Recover
        new MoveBalancePatch { Move = 234, PP = 5 }, // Sol Matinal / Morning Sun
        new MoveBalancePatch { Move = 236, PP = 5 }, // Luz Lunar / Moonlight
        new MoveBalancePatch { Move = 208, PP = 5 }, // Batido / Milk Drink
        new MoveBalancePatch { Move = 303, PP = 5 }, // Relajo / Slack Off
        new MoveBalancePatch { Move = 235, PP = 5 }, // Síntesis / Synthesis
        new MoveBalancePatch { Move = 456, PP = 5 }, // Auxilio / Heal Order
        new MoveBalancePatch { Move = 135, PP = 5 }, // Amortiguador / Soft-Boiled
        new MoveBalancePatch { Move = 355, PP = 5 }, // Respiro / Roost
        new MoveBalancePatch { Move = 505, PP = 5 }, // Pulso Cura / Heal Pulse
    ];
    }
    private void FixMoveFlagsListLayout()
    {
        const int gap = 6;

        CheckedListBox flagsList = null;

        foreach (Control control in Controls)
        {
            if (control is CheckedListBox checkedListBox)
            {
                flagsList = checkedListBox;
                break;
            }
        }

        if (flagsList is null)
            return;

        // Poner la lista debajo del botón Export Table.
        flagsList.Top = B_Table.Bottom + gap;

        // Reducir la altura, manteniendo scroll.
        int bottomMargin = 8;
        int newHeight = ClientSize.Height - flagsList.Top - bottomMargin;

        if (newHeight < 120)
            newHeight = 120;

        flagsList.Height = newHeight;
    }

    private static readonly string[] TargetingTypes =
    [
        "Single Adjacent Ally/Foe",
        "Any Ally", "Any Adjacent Ally", "Single Adjacent Foe", "Everyone but User", "All Foes",
        "All Allies", "Self", "All Pokémon on Field", "Single Adjacent Foe (2)", "Entire Field",
        "Opponent's Field", "User's Field", "Self",
    ];

    private static readonly string[] InflictionTypes =
    [
        "None",
        "Paralyze", "Sleep", "Freeze", "Burn", "Poison",
        "Confusion", "Attract", "Capture", "Nightmare", "Curse",
        "Taunt", "Torment", "Disable", "Yawn", "Heal Block",
        "?", "Detect", "Leech Seed", "Embargo", "Perish Song",
        "Ingrain", "??? 0x16", "??? 0x17", "Mute",
    ];

    private static readonly string[] MoveQualities =
    [
        "Only DMG",
        "No DMG -> Inflict Status", "No DMG -> -Target/+User Stat", "No DMG | Heal User", "DMG | Inflict Status", "No DMG | STATUS | +Target Stat",
        "DMG | -Target Stat", "DMG | +User Stat", "DMG | Absorbs DMG", "One-Hit KO", "Affects Whole Field",
        "Affect One Side of the Field", "Forces Target to Switch", "Unique Effect",
    ];

    private static readonly string[] ZMoveEffects =
    [
        "None",
        "+1 Attack",
        "+2 Attack",
        "+3 Attack",
        "+1 Defense",
        "+2 Defense",
        "+3 Defense",
        "+1 Special Attack",
        "+2 Special Attack",
        "+3 Special Attack",
        "+1 Special Defense",
        "+2 Special Defense",
        "+3 Special Defense",
        "+1 Speed",
        "+2 Speed",
        "+3 Speed",
        "+1 Accuracy",
        "+2 Accuracy",
        "+3 Accuracy",
        "+1 Evasiveness",
        "+2 Evasiveness",
        "+3 Evasiveness",
        "+1 to all (except Accuracy or Evasiveness)",
        "+2 to all (except Accuracy or Evasiveness)",
        "+3 to all (except Accuracy or Evasiveness)",
        "raises critical-hit ratio two stages",
        "resets lowered stats of the user",
        "recovers all of user's HP",
        "recovers all Hp of the Pokémon switching-in (Memento and Parting Shot)",
        "makes the user the center of attention",
        "only on Curse: recovers all HP if the user's a Ghost type, +1 Attack otherwise",
    ];

    private void Setup()
    {
        char[] ps = ['P', 'S']; // Distinguish Physical/Special Z-Moves
        for (int i = 622; i < 658; i++)
            movelist[i] += $" ({ps[i % 2]})";
        CB_Move.Items.AddRange(movelist);
        CB_Type.Items.AddRange(types);
        CB_Category.Items.AddRange(MoveCategories);
        CB_Stat1.Items.AddRange(StatCategories);
        CB_Stat2.Items.AddRange(StatCategories);
        CB_Stat3.Items.AddRange(StatCategories);
        CB_Targeting.Items.AddRange(TargetingTypes);
        CB_Quality.Items.AddRange(MoveQualities);
        CB_Inflict.Items.AddRange(InflictionTypes);
        CB_ZMove.Items.AddRange(movelist);
        var flagnames = Enum.GetNames(typeof(MoveFlag7)).Skip(1).ToArray();
        CLB_Flags.Items.AddRange(flagnames);
        CB_ZEffect.Items.AddRange(ZMoveEffects);
        CB_Inflict.Items.Add("Special");
        var refreshtypes = Enum.GetNames(typeof(RefreshType));
        CB_AfflictRefresh.Items.AddRange(refreshtypes);

        CB_Move.Items.RemoveAt(0);
        CB_Move.SelectedIndex = 0;
    }

    private int entry = -1;

    private void ChangeEntry(object sender, EventArgs e)
    {
        SetEntry();
        entry = Array.IndexOf(movelist, CB_Move.Text);
        GetEntry();
    }

    private void GetEntry()
    {
        if (entry < 1) return;
        byte[] data = files[entry];
        {
            RTB.Text = moveflavor[entry].Replace("\\n", Environment.NewLine);

            CB_Type.SelectedIndex = data[0x00];
            CB_Quality.SelectedIndex = data[0x01];
            CB_Category.SelectedIndex = data[0x02];
            NUD_Power.Value = data[0x3];
            NUD_Accuracy.Value = data[0x4];
            NUD_PP.Value = data[0x05];
            NUD_Priority.Value = (sbyte)data[0x06];
            NUD_HitMin.Value = data[0x7] & 0xF;
            NUD_HitMax.Value = data[0x7] >> 4;
            short inflictVal = BitConverter.ToInt16(data, 0x08);
            CB_Inflict.SelectedIndex = inflictVal < 0 ? CB_Inflict.Items.Count - 1 : inflictVal;
            NUD_Inflict.Value = data[0xA];
            NUD_0xB.Value = data[0xB]; // 0xB ~ Something to deal with skipImmunity
            NUD_TurnMin.Value = data[0xC];
            NUD_TurnMax.Value = data[0xD];
            NUD_CritStage.Value = data[0xE];
            NUD_Flinch.Value = data[0xF];
            NUD_Effect.Value = BitConverter.ToUInt16(data, 0x10);
            NUD_Recoil.Value = (sbyte)data[0x12];
            NUD_Heal.Value = data[0x13];

            CB_Targeting.SelectedIndex = data[0x14];
            CB_Stat1.SelectedIndex = data[0x15];
            CB_Stat2.SelectedIndex = data[0x16];
            CB_Stat3.SelectedIndex = data[0x17];
            NUD_Stat1.Value = (sbyte)data[0x18];
            NUD_Stat2.Value = (sbyte)data[0x19];
            NUD_Stat3.Value = (sbyte)data[0x1A];
            NUD_StatP1.Value = data[0x1B];
            NUD_StatP2.Value = data[0x1C];
            NUD_StatP3.Value = data[0x1D];

            var move = new Move7(data);
            CB_ZMove.SelectedIndex = move.ZMove;
            NUD_ZPower.Value = move.ZPower;
            CB_ZEffect.SelectedIndex = move.ZEffect;
            CB_AfflictRefresh.SelectedIndex = (int)move.RefreshAfflictType;
            NUD_RefreshAfflictPercent.Value = move.RefreshAfflictPercent;

            var flags = (uint)move.Flags;
            for (int i = 0; i < CLB_Flags.Items.Count; i++)
                CLB_Flags.SetItemChecked(i, ((flags >> i) & 1) == 1);
        }
    }

    private void SetEntry()
    {
        if (entry < 1) return;
        byte[] data = files[entry];
        {
            data[0x00] = (byte)CB_Type.SelectedIndex;
            data[0x01] = (byte)CB_Quality.SelectedIndex;
            data[0x02] = (byte)CB_Category.SelectedIndex;
            data[0x03] = (byte)NUD_Power.Value;
            data[0x04] = (byte)NUD_Accuracy.Value;
            data[0x05] = (byte)NUD_PP.Value;
            data[0x06] = (byte)(int)NUD_Priority.Value;
            data[0x07] = (byte)((byte)NUD_HitMin.Value | ((byte)NUD_HitMax.Value << 4));
            int inflictval = CB_Inflict.SelectedIndex; if (inflictval == CB_Inflict.Items.Count) inflictval = -1;
            Array.Copy(BitConverter.GetBytes((short)inflictval), 0, data, 0x08, 2);
            data[0x0A] = (byte)NUD_Inflict.Value;
            data[0x0B] = (byte)NUD_0xB.Value;
            data[0x0C] = (byte)NUD_TurnMin.Value;
            data[0x0D] = (byte)NUD_TurnMax.Value;
            data[0x0E] = (byte)NUD_CritStage.Value;
            data[0x0F] = (byte)NUD_Flinch.Value;
            Array.Copy(BitConverter.GetBytes((ushort)NUD_Effect.Value), 0, data, 0x10, 2);
            data[0x12] = (byte)(int)NUD_Recoil.Value;
            data[0x13] = (byte)NUD_Heal.Value;
            data[0x14] = (byte)CB_Targeting.SelectedIndex;
            data[0x15] = (byte)CB_Stat1.SelectedIndex;
            data[0x16] = (byte)CB_Stat2.SelectedIndex;
            data[0x17] = (byte)CB_Stat3.SelectedIndex;
            data[0x18] = (byte)(int)NUD_Stat1.Value;
            data[0x19] = (byte)(int)NUD_Stat2.Value;
            data[0x1A] = (byte)(int)NUD_Stat3.Value;
            data[0x1B] = (byte)NUD_StatP1.Value;
            data[0x1C] = (byte)NUD_StatP2.Value;
            data[0x1D] = (byte)NUD_StatP3.Value;

            var move = new Move7(data)
            {
                ZMove = CB_ZMove.SelectedIndex,
                ZPower = (int)NUD_ZPower.Value,
                ZEffect = CB_ZEffect.SelectedIndex,
                RefreshAfflictPercent = (int)NUD_RefreshAfflictPercent.Value,
                RefreshAfflictType = (RefreshType)CB_AfflictRefresh.SelectedIndex,
            };

            uint flagval = 0;
            for (int i = 0; i < CLB_Flags.Items.Count; i++)
                flagval |= CLB_Flags.GetItemChecked(i) ? 1u << i : 0;
            move.Flags = (MoveFlag7)flagval;
        }
        files[entry] = data;
    }

    private void CloseForm(object sender, FormClosingEventArgs e)
    {
        SetEntry();
        RandSettings.SetFormSettings(this, groupBox1.Controls);
    }

    private void B_Table_Click(object sender, EventArgs e)
    {
        var items = files.Select(z => new Move7(z));
        Clipboard.SetText(TableUtil.GetTable(items, movelist));
        System.Media.SystemSounds.Asterisk.Play();
    }

    private void B_RandAll_Click(object sender, EventArgs e)
    {
        if (!CHK_Category.Checked && !CHK_Type.Checked)
        {
            WinFormsUtil.Alert("Cannot randomize Moves.", "Please check any of the options on the right to randomize Moves.");
            return;
        }

        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Randomize Moves? Cannot undo.", "Double check options on the right before continuing.") != DialogResult.Yes) return;
        Random rnd = Util.Rand;
        for (int i = 0; i < CB_Move.Items.Count; i++)
        {
            CB_Move.SelectedIndex = i; // Get new Move
            if (i is 165 or 174) continue; // Don't change Struggle or Curse

            // Change Damage Category if Not Status
            if (CB_Category.SelectedIndex > 0 && CHK_Category.Checked) // Not Status
                CB_Category.SelectedIndex = rnd.Next(1, 3);

            // Change Move Type
            if (CHK_Type.Checked)
                CB_Type.SelectedIndex = rnd.Next(0, 18);
        }
        WinFormsUtil.Alert("All Moves have been randomized!");
    }

    private void B_Metronome_Click(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(MessageBoxButtons.YesNo, "Play using Metronome Mode?", "This will set the Base PP for every other Move to 0!") != DialogResult.Yes) return;

        for (int i = 0; i < CB_Move.Items.Count; i++)
        {
            CB_Move.SelectedIndex = i;
            if (CB_Move.SelectedIndex is not (117 and 32))
                NUD_PP.Value = 0;
            if (CB_Move.SelectedIndex == 117)
                NUD_PP.Value = 40;
            if (CB_Move.SelectedIndex == 32)
                NUD_PP.Value = 1;
        }
        CB_Move.SelectedIndex = 0;
        WinFormsUtil.Alert("All Moves have had their Base PP values modified!");
    }
}