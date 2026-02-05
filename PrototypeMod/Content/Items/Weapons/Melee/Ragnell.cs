using PrototypeMod.Content.Items; // Access the Items folder
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using System;

using Microsoft.Xna.Framework; // tMod uses MonoGame, a continuation of Microsoft XNA that uses the same namespaces

namespace PrototypeMod.Content.Items.Weapons.Melee // Location for the code
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class Ragnell : ModItem
	{
		private bool aetherProc = false; // Bool value for when the Sol portion of Aether procs

		// The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.PrototypeMod.hjson' file.
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1; // Item num needed for Journey mode research
		}

		public override void SetDefaults()
		{
			// Visual properties
			Item.width = 40;
			Item.height = 40;
			Item.scale = 1.25f; // Item size
			Item.rare = ItemRarityID.Orange; // Color for items for directly before the boss "Wall of Flesh"

			// Combat properties
			Item.damage = 50;
			Item.DamageType = DamageClass.Melee; // What type of damage item is deals, Melee, Ranged, Magic, Summon, Generic (takes bonuses from all damage multipliers), Default (doesn't take bonuses from any damage multipliers)
            
			// useTime and useAnimation often use the same value, but there are examples where they don't use the same values
			Item.useTime = 30; // How long the swing hitbox lasts in ticks (60 ticks = 1 second)
			Item.useAnimation = 30; // How long the swing animation lasts in ticks (60 ticks = 1 second)
			Item.knockBack = 6f; // How far the enemies hit get launched; 20 is the maximum value
			Item.autoReuse = false; // Whether the item can auto swing by holding the attack button
			
			Item.value = Item.buyPrice(gold: 10, silver: 1) / 2; // Item sell price + buy price (buy price seems to return value in copper)
			Item.useStyle = ItemUseStyleID.Swing; // How the item is held
			Item.UseSound = SoundID.Item1; // The sound used (can be default or imported)
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient<Materials.Asherite>(15)
				.AddIngredient(ItemID.Emerald, 3)
				.AddRecipeGroup("GoldBar", 5)
				.AddTile(TileID.Anvils)
				.Register();

		}

		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) 
		{
			if (Main.rand.NextBool(5)) // 1/5 chance to proc Aether
			{
				// Ignore half of the target's base defense
        		modifiers.ArmorPenetration += target.defDefense / 2; // Luna

				aetherProc = true; // Set to true for Sol in OnHitNPC
			} else
			{
				aetherProc = false;
			}
		}

		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) // You can attack an entity without overriding this, but for status ailments/buffs or something that needs the final damage calculation override
		{
			if(aetherProc)
			{
				// ...spawning particle, referred to as "dust"
				if (aetherProc)
				{
					Dust.NewDust(target.position, // Position to spawn
						target.width, target.height, //Width and height of hitbox; area to spawn dust in
						DustID.BlueTorch, // Types of default dust: https://terraria.wiki.gg/wiki/Dust_IDs
						0.5f, 0.5f, // Speed X and Speed Y of dust (speed will have "some randomization", unsure if it's additive or multiplicative)
						0); // Dust transparency from 0 to 255
				}
				player.Heal(damageDone); // Sol
				aetherProc = false; // Turn off aetherProc to prevent dust system from triggering
			}
		}
	}
}
