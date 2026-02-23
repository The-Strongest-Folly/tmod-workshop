// using ExampleMod.Content.Biomes;
// using ExampleMod.Content.Buffs;

using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using PrototypeMod.Content.Items;
using PrototypeMod.Content.Items.Materials;

// Reminder: Assume distance = pixels; 1 block = 16 x 16 pixels

namespace PrototypeMod.Content.NPCs
{
	// To learn how to further adapt vanilla NPC behaviors, see https://github.com/tModLoader/tModLoader/wiki/Advanced-Vanilla-Code-Adaption#example-npc-npc-clone-with-modified-projectile-hoplite
	public class BlackKnight : ModNPC
	{
		const int RADIUS = 72;
		const int BLOCK = 32; // Block length/height; multiply by this to convert pixels to blocks



		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 6; // static int[] that determines number of animation frames the NPC has

			NPCID.Sets.ShimmerTransformToNPC[Type] = -1; // Defaults to -1 anyway, but this indicates who the NPC transforms to in Shimmer. There is also a ShimmerTransformToItem[Type] value which turns an NPC into the indicated item.

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}

		public override void SetDefaults() {
			NPC.width = 48;
			NPC.height = 72;
			NPC.damage = 60;
			NPC.defense = 20;
			NPC.lifeMax = 2000;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 10000f;
			NPC.knockBackResist = 0.01f;
			NPC.aiStyle = NPCAIStyleID.Fighter; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic
			// NPC.ai[0] - State; NPC.ai[1] - Time since Alondite attack while in Attack State

			NPC.rarity = 1; // This is the simplest way to make an enemy not be able to pick up coins. If rarity is higher than 0, then the enemy will not be able to pick up coins on expert mode. This also means that Black Knight will be detected by the Lifeform Analyzer.

			NPC.teleportTime = 90f; // Delay for a teleport to take place

			NPC.netUpdate = true; // Need this for the 1/5 Alondite prock

            NPC.despawnEncouraged = false; // Should prevent Black Knight from naturally despawning
            NPC.GravityIgnoresLiquid = true; // Black Knight is heavily armored, so he should not float


			Banner = Item.NPCtoBanner(NPCID.Paladin); // Makes this NPC get affected by the normal paladin banner.
			BannerItem = Item.BannerToItem(Banner); // Makes kills of this NPC go towards dropping the banner it's associated with.
			// SpawnModBiomes = [ModContent.GetInstance<ExampleSurfaceBiome>().Type]; // Associates this NPC with the ExampleSurfaceBiome in Bestiary
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Asherite>(), // Reference required for modded item drops since ID is generated at runtime
			1, // Item chance denominator
			5, 5 // Item drop min/max. Assumedly, each value in the range can occur equally.
			));
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.OverworldNightMonster.Chance; // Debug
			return SpawnCondition.OverworldNightMonster.Chance * (NPC.downedQueenBee ? 0.01f : 0.0f); // Spawn with 1/100th the chance of a regular zombie if Queen Bee was killed.
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange([
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,

				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement("Mods.PrototypeMod.Bestiary.BlackKnight"),

				// By default the last added IBestiaryBackgroundImagePathAndColorProvider will be used to show the background image.
				// The ExampleSurfaceBiome ModBiomeBestiaryInfoElement is automatically populated into bestiaryEntry.Info prior to this method being called
				// so we use this line to tell the game to prioritize a specific InfoElement for sourcing the background image.
				// new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<ExampleSurfaceBiome>().ModBiomeBestiaryInfoElement),
			]);
		}

		public override void HitEffect(NPC.HitInfo hit) {
			// // Spawn confetti when this zombie is hit.

			// for (int i = 0; i < 10; i++) {
			// 	int dustType = Main.rand.Next(139, 143);
			// 	var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);

			// 	dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
			// 	dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);

			// 	dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
			// }
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			if(target.statLife <= 0)
			{
				NPC.ai[0] = 0; // Set state to wander to stop attacks
				NPC.Teleport(NPC.position,0,7); // Teleport to current position to make it look like teleporting away
				NPC.active = false; // Deactivate NPC
        		NPC.netUpdate = true; // Sync with multiplayer
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			ModContent.GetInstance<PrototypeMod>().Logger.Info(modifiers);
			if (Main.rand.NextBool(2)) // 1/2 chance to proc Luna, since Black Knight is skilled with this technique
			{
				// Luna
				//int newDefense = (int) target.DefenseStat / 2; // Ignore half of the target's defense
				//modifiers = modifiers.ToHurtInfo(modifiers.damage, newDefense, modifiers.defenseEffectiveness, modifiers.knockback, modifiers.knockbackImmune); // Rebuild modifiers with newDefense, then assign it back to the reference point
				Dust.NewDust(target.position, // Position to spawn
						target.width, target.height, //Width and height of hitbox; area to spawn dust in
						DustID.GemRuby, // Types of default dust: https://terraria.wiki.gg/wiki/Dust_IDs
						0.5f, 0.5f, // Speed X and Speed Y of dust (speed will have "some randomization", unsure if it's additive or multiplicative)
						125); // Dust transparency from 0 to 255
			}
		}

		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
			// Black Knight's armor is blessed by the goddess Ashera. While in his original game, this would mean that he could not be damaged unless attacked with the Ragnell sword, that would be very problematic here! Instead, he'll reduce all incoming damage by 50%. The effect is similar to current meta-defining skills in the mobile game Fire Emblem Heroes, where he also appears.
			modifiers.FinalDamage *= 0.5f;
		}

		public override void FindFrame(int frameHeight)
		{
			// Count up and change the frame appropriately
			NPC.frameCounter++;

			if(NPC.frameCounter >= 6.0f)
			{
				if (frameHeight >= 360)
					NPC.frame.Y = 0;
				else
					NPC.frame.Y += frameHeight;
			}

		}

        public override void AI()
        {
            // AI operates via state machine
            switch (NPC.ai[0])
            {
                case 0: // Wander State
                    NPC.velocity.X = 0.5f;
                    if (Microsoft.Xna.Framework.Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) <= 30f * BLOCK) // If a player is 30 blocks away or less:
                    {
                        NPC.ai[1] = 0f; // Reset warp timer
                        NPC.ai[0] = 1f; // Set state to Attack State
                    }
                    break;
                case 1: // Attack State
                    NPC.velocity.X = 1.0f;
                    if (Microsoft.Xna.Framework.Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 40f * BLOCK) // If the target gets more than 40 blocks away:
                    {
                        NPC.ai[0] = 0f; // Set state to Wander State
                    }
                    /** Attack types:
                    * If close: attack with Alondite (cooldown 0.75 seconds)
                    * If too far from player to attack for 15 seconds: Warp Powder (teleport directly next to player and attack)
                    */
                    if (Microsoft.Xna.Framework.Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) <= 5f * BLOCK)
                    {
                        // Alondite attack code
                    } else
                    {
                        NPC.ai[1]++; // Terraria operates at a constant 60fps and counts frames for time. Therefore, 60 = 1 second.
                        if (NPC.ai[1] >= 900f)
                        {
                            // Attempt to teleport Black Knight to player
							Microsoft.Xna.Framework.Vector2 position = Main.player[NPC.target].Center;

							int spawnRadius = 3 * BLOCK;

                            if (NPC.AI_AttemptToFindTeleportSpot(ref position, // Source for teleport
							0 * BLOCK, 5 * BLOCK, // Displacement from source
							spawnRadius, // Radius from target that NPC can spawn in
							72, // Distance to prevent telefragging (no overlaping entities in this radius that could be IK'd)
							72, // Radius for checking solid blocks to prevent clipping
							false, // "solidTileCheckCentered" may mean whether the solid tile check is centered on the target or the NPC
							true // Determines if NPC can teleport onto air rather than a solid tile
							)) 
							{
								NPC.velocity.X = 0.0f;

								Microsoft.Xna.Framework.Vector2 upwarp = new Microsoft.Xna.Framework.Vector2((float)0.0f, (float)5.0f * BLOCK);

								Microsoft.Xna.Framework.Vector2 center = Main.player[NPC.target].Center;

								NPC.Teleport(center + upwarp, // Teleport destination
								0, // Default teleport
								7 // Teleport with Demon Conch vfx
								);
								// Reset timer
								NPC.ai[1] = 0f;
								NPC.velocity.X = 1.0f;
							}
                        }
                    }
                    break;
                    
            }
        }
	}
}