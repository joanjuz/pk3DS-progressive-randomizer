using pk3DS.Core.Structures;

namespace pk3DS.WinForms;

internal static class EconomyFixer
{
    // Item IDs are stable in Gen 6/7 item data.
    private const int UltraBall = 2;
    private const int GreatBall = 3;
    private const int PokeBall = 4;
    private const int SuperRepel = 76;
    private const int MaxRepel = 77;
    private const int Repel = 79;

    internal static int Apply(byte[][] files)
    {
        int changed = 0;

        // Technical Machines / Hidden Machines used by pk3DS as the protected TM/HM set.
        foreach (int itemID in MartEditor7.BannedItems)
            changed += SetBuyPrice(files, itemID, 1000);

        changed += SetBuyPrice(files, PokeBall, 100);
        changed += SetBuyPrice(files, GreatBall, 150);
        changed += SetBuyPrice(files, UltraBall, 200);

        changed += SetBuyPrice(files, Repel, 50);
        changed += SetBuyPrice(files, SuperRepel, 50);
        changed += SetBuyPrice(files, MaxRepel, 50);

        return changed;
    }

    private static int SetBuyPrice(byte[][] files, int itemID, int buyPrice)
    {
        if ((uint)itemID >= (uint)files.Length)
            return 0;

        if (files[itemID] is not { Length: > 0 })
            return 0;

        var item = new Item(files[itemID]);

        if (item.BuyPrice == buyPrice)
            return 0;

        item.BuyPrice = buyPrice;
        files[itemID] = item.Write();
        return 1;
    }
}
