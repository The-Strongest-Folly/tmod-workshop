using PrototypeMod.Content.Items; // Access the Items folder
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace PrototypeMod.Content.Items.Weapons.Melee
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class Ragnell : ModItem
	{
		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.PrototypeMod.hjson' file.
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			// Visual properties
			Item.width = 40;
			Item.height = 40;
			Item.scale = 1f;
			Item.rare = ItemRarityID.Orange;

			Item.damage = 50;
			Item.DamageType = DamageClass.Melee;
			
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.knockBack = 6;
			Item.autoReuse = true;
			
			Item.value = Item.buyPrice(silver: 1);
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item1;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient<Items.Materials.Asherite>(15)
				.AddIngredient(ItemID.Emerald, 1)
				.AddIngredient(ItemID.PlatinumBar, 5)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
