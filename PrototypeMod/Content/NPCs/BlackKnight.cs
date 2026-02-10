// using ExampleMod.Content.Biomes;
// using ExampleMod.Content.Buffs;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace PrototypeMod.Content.NPCs
{
	// To learn how to further adapt vanilla NPC behaviors, see https://github.com/tModLoader/tModLoader/wiki/Advanced-Vanilla-Code-Adaption#example-npc-npc-clone-with-modified-projectile-hoplite
	public class BlackKnight : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[8]; // Learning, will document when fully understood

			NPCID.Sets.ShimmerTransformToNPC[Type] = -1; // Defaults to -1 anyway, but this indicates who the NPC transforms to in Shimmer. There is also a ShimmerTransformToItem[Type] value which turns an NPC into the indicated item.

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}

		public override void SetDefaults() {
			NPC.width = 48;
			NPC.height = 72;
			NPC.damage = 40;
			NPC.defense = 30;
			NPC.lifeMax = 800;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 10000f;
			NPC.knockBackResist = 0.01f;
			NPC.aiStyle = NPCAIStyleID.Fighter; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic

            NPC.despawnEncouraged = false; // Should prevent Black Knight from naturally despawning
            NPC.GravityIgnoresLiquid = true; // Black Knight is heavily armored, so he should not float

			// AIType = NPCID.Zombie; // Use vanilla zombie's type when executing AI code. (This also means it will try to despawn during daytime)
			// AnimationType = NPCID.Zombie; // Use vanilla zombie's type when executing animation code. Important to also match Main.npcFrameCount[NPC.type] in SetStaticDefaults.
			// Banner = Item.NPCtoBanner(NPCID.Zombie); // Makes this NPC get affected by the normal zombie banner.
			// BannerItem = Item.BannerToItem(Banner); // Makes kills of this NPC go towards dropping the banner it's associated with.
			// SpawnModBiomes = [ModContent.GetInstance<ExampleSurfaceBiome>().Type]; // Associates this NPC with the ExampleSurfaceBiome in Bestiary
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			// Since Party Zombie is essentially just another variation of Zombie, we'd like to mimic the Zombie drops.
			// To do this, we can either (1) copy the drops from the Zombie directly or (2) just recreate the drops in our code.
			// (1) Copying the drops directly means that if Terraria updates and changes the Zombie drops, your ModNPC will also inherit the changes automatically.
			// (2) Recreating the drops can give you more control if desired but requires consulting the wiki, bestiary, or source code and then writing drop code.

			// (1) This example shows copying the drops directly. For consistency and mod compatibility, we suggest using the smallest positive NPCID when dealing with npcs with many variants and shared drop pools.
			var zombieDropRules = Main.ItemDropsDB.GetRulesForNPCID(NPCID.Zombie, false); // false is important here
			foreach (var zombieDropRule in zombieDropRules) {
				// In this foreach loop, we simple add each drop to the PartyZombie drop pool.
				npcLoot.Add(zombieDropRule);
			}

			// (2) This example shows recreating the drops. This code is commented out because we are using the previous method instead.
			// npcLoot.Add(ItemDropRule.Common(ItemID.Shackle, 50)); // Drop shackles with a 1 out of 50 chance.
			// npcLoot.Add(ItemDropRule.Common(ItemID.ZombieArm, 250)); // Drop zombie arm with a 1 out of 250 chance.

			// Finally, we can add additional drops. Many Zombie variants have their own unique drops: https://terraria.fandom.com/wiki/Zombie
			npcLoot.Add(ItemDropRule.Common(ItemID.Confetti, 100)); // 1% chance to drop Confetti
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.OverworldNightMonster.Chance * 0.2f; // Spawn with 1/5th the chance of a regular zombie.
		}

		public override void AI() {
			if (NPC.wet) {
				if (NPC.honeyWet) { // Removes the effects of honey's fall rate making the NPC fall normally in honey
					NPC.GravityMultiplier /= NPC.GravityWetMultipliers[LiquidID.Honey];
					NPC.MaxFallSpeedMultiplier /= NPC.MaxFallSpeedWetMultipliers[LiquidID.Honey];
				}
				else if (!NPC.lavaWet && !NPC.shimmerWet) { // Removes water falls speed effects, then adds honey falls speed effects, making the NPC fall at the honey rate in water
					NPC.GravityMultiplier *= NPC.GravityWetMultipliers[LiquidID.Honey] / NPC.GravityWetMultipliers[LiquidID.Water];
					NPC.MaxFallSpeedMultiplier *= NPC.MaxFallSpeedWetMultipliers[LiquidID.Honey] / NPC.MaxFallSpeedWetMultipliers[LiquidID.Water];
				}
			}
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			// We can use AddRange instead of calling Add multiple times in order to add multiple items at once
			bestiaryEntry.Info.AddRange([
				// Sets the spawning conditions of this NPC that is listed in the bestiary.
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,

				// Sets the description of this NPC that is listed in the bestiary.
				new FlavorTextBestiaryInfoElement("Mods.ExampleMod.Bestiary.PartyZombie"),

				// By default the last added IBestiaryBackgroundImagePathAndColorProvider will be used to show the background image.
				// The ExampleSurfaceBiome ModBiomeBestiaryInfoElement is automatically populated into bestiaryEntry.Info prior to this method being called
				// so we use this line to tell the game to prioritize a specific InfoElement for sourcing the background image.
				new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<ExampleSurfaceBiome>().ModBiomeBestiaryInfoElement),
			]);
		}

		public override void HitEffect(NPC.HitInfo hit) {
			// Spawn confetti when this zombie is hit.

			for (int i = 0; i < 10; i++) {
				int dustType = Main.rand.Next(139, 143);
				var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);

				dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
				dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);

				dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			// Here we can make things happen if this NPC hits a player via its hitbox (not projectiles it shoots, this is handled in the projectile code usually)
			// Common use is applying buffs/debuffs:

			int buffType = ModContent.BuffType<AnimatedBuff>();
			// Alternatively, you can use a vanilla buff: int buffType = BuffID.Slow;

			int timeToAdd = 5 * 60; // This makes it 5 seconds, one second is 60 ticks
			target.AddBuff(buffType, timeToAdd);
		}

		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
			if (modifiers.DamageType.CountsAsClass(DamageClass.Magic)) {
				// This example shows how PartyZombie reduces magic damage by 75%. We use FinalDamage here rather than SourceDamage since we are affecting how the npc reacts to the damage.
				// Conceptually, the source dealing the damage isn't interpreted as weaker, but rather this NPC has a resistance to this damage source.
				modifiers.FinalDamage *= 0.25f;
			}
		}

        public override void AI()
        {
            // AI operates via state machine
            switch (NPC.ai[0])
            {
                case 0: // Wander State
                    NPC.velocity.X = 0.5f;
                    if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) <= 30f)
                    {
                        NPC.ai[1] = 0f;
                        NPC.ai[0] = 1f;
                    }
                    break;
                case 1: // Attack State
                    NPC.velocity.X = 1.0f;
                    if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 40f)
                    {
                        NPC.ai[0] = 0f;
                    }
                    /** Attack types:
                    * If close: attack with Alondite (cooldown 0.75 seconds)
                    * If too far from player to attack for 15 seconds: Warp Powder (teleport directly next to player and attack)
                    */
                    if (Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) <= 5f)
                    {
                        // Alondite attack code
                    } else
                    {
                        NPC.ai[1]++; // Terraria operates at a constant 60fps and counts frames for time. Therefore, 60 = 1 second.
                        if (NPC.ai[1] >= 900f)
                        {
                            // Teleport Black Knight to player
                            if (NPC.AI_AttemptToFindTeleportSpot(ref Vector2(NPC.currentPosition)))
                            NPC.Teleport
                            // Reset timer
                            NPC.ai[1] = 0f;
                        }
                    }
                    break;
                    
            }
        }
	}
}