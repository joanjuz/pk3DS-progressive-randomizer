using System;
using System.Drawing;
using System.Windows.Forms;

namespace pk3DS.WinForms;

public partial class StaticEncounterEditor7
{
    private const int ArcanineSpecies = 59;
    private Button B_GiftsArcanineBaseline;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        AddArcanineGiftButton();
    }

    private void AddArcanineGiftButton()
    {
        if (B_GiftsArcanineBaseline != null)
            return;

        B_GiftsArcanineBaseline = new Button
        {
            Location = new Point(249, 280),
            Name = "B_GiftsArcanineBaseline",
            Size = new Size(250, 30),
            Text = "All Gifts -> Arcanine (BST 555)",
            UseVisualStyleBackColor = true,
        };
        B_GiftsArcanineBaseline.Click += B_GiftsArcanineBaseline_Click;

        Tab_Gifts.Controls.Add(B_GiftsArcanineBaseline);
        B_GiftsArcanineBaseline.BringToFront();
    }

    private void B_GiftsArcanineBaseline_Click(object sender, EventArgs e)
    {
        if (WinFormsUtil.Prompt(
            MessageBoxButtons.YesNo,
            "Set all Gift Pokemon to Arcanine?",
            "This sets Species to Arcanine and Form to 0 for every Gift entry, including the three starter slots. Other Gift data is left unchanged.",
            "Use this as a common BST 555 baseline before randomizing by BST.") != DialogResult.Yes)
        {
            return;
        }

        // Persist edits currently visible in the selected Gift before applying the batch change.
        SetGift();

        for (int i = 0; i < Gifts.Length; i++)
        {
            var gift = Gifts[i];
            gift.Species = ArcanineSpecies;
            gift.Form = 0;
            LB_Gift.Items[i] = GetEntryText(gift, i);
        }

        // Refresh the currently selected Gift without rebuilding the ListBox selection.
        GetGift();

        WinFormsUtil.Alert(
            "Gift Pokemon updated!",
            $"{Gifts.Length} Gift entries now use Arcanine as their BST 555 baseline.");
    }
}
