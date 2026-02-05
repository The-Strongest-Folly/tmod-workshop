// What libraries we use in the code
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.GameContent.Creative;

namespace PrototypeMod.Content.Items.Materials // Where your code is located
{
    public class Asherite : ModItem // Your item name (Asherite) and type (ModItem)
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25; // How many items need for research in Journey Mode
        }

        public override void SetDefaults()
        {
            Item.width = 24; // Width of an item sprite
            Item.height = 28; // Height of an item sprite
            Item.maxStack = 9999; // How many items can be in one inventory slot
            Item.value = 1100; // Item sell price in copper coins
            Item.rare = ItemRarityID.Orange; // The color of item's name in game. Check https://terraria.wiki.gg/wiki/Rarity
        }
    }
}