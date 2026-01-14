using RiskOfRoutes.StageModifiers.CombatModifiers;
using RiskOfRoutes.StageModifiers.OtherModifiers;
using RiskOfRoutes.StageModifiers.SceneModifiers;
using RoR2;
using SamplePlugin.StageModifiers;
using SamplePlugin.StageModifiers.BossRewardModifiers;
using SamplePlugin.StageModifiers.CombatModifiers;
using SamplePlugin.StageModifiers.OtherModifiers;
using SamplePlugin.StageModifiers.SceneModifiers;

namespace RiskOfRoutes.StageModifiers
{
    public static class StageModifierCatalog
    {
        public static StageModifier StageModifierFromDef(string def,int stack = 1)
        {
            switch (def)
            {
                case "Aurelionite": return new AurelioniteRewardModifier();
                case "BossRedItem": return new BossRedItemModifier();
                case "BossYellowItem": return new BossYellowItemModifier();
                case "DroneBossGreen": return new DroneBossRewardModifier(false);
                case "DroneBossRed": return new DroneBossRewardModifier(true);
                case "MoreElites": return new MoreElitesModifier(stack);
                case "MoreDrones": return new MoreDronesModifier(stack);
                case "LunarEnemies": return new LunarEnemiesModifier(stack);
                case "VoidEnemies": return new VoidEnemiesModifier(stack);
                case "YellowPrinter": return new YellowPrinterModifier(stack);
                case "RedPrinter": return new RedPrinterModifier(stack);
                case "OnlyFlying": return new OnlyFlyingEnemiesModifier();
                case "PowerfulElites": return new Tier2ElitesEarlierModifier();
                case "Mystery": return new MysteryModifier();
                case "Repair": return new RepairConsumedItemsModifier();
                case "WanderingChef": return new WanderingChefModifier();
                case "Mountain": return new SaveMountainEffectModifier();
                case "SoulCost":return new StackingCurseModifier(stack);
                case "Doppelganger":return new DoppelgangerAttackModifier();

                //артефакты
                case "FriendlyFire": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.FriendlyFire, false);
                case "Command": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Command, true);
                case "Delusion": return new ArtifactModifier(RoR2.CU8Content.Artifacts.Delusion, true);
                case "Devotion": return new ArtifactModifier(RoR2.CU8Content.Artifacts.Devotion, false);
                case "MixEnemy": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.MixEnemy, false);
                case "Enigma": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Enigma, false);
                case "MonsterTeamGainsItems": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.MonsterTeamGainsItems, false);
                case "WeakAssKnees": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.WeakAssKnees, false);
                case "Glass": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Glass, false);
                case "EliteOnly": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.EliteOnly, false);
                case "SingleMonsterType": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.SingleMonsterType, false);
                case "RandomSurvivorOnRespawn": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.RandomSurvivorOnRespawn, false);
                case "Rebirth": return new ArtifactModifier(RoR2.DLC2Content.Artifacts.Rebirth, true);
                case "Sacrifice": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Sacrifice, false);
                case "WispOnDeath": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.WispOnDeath, false);
                case "Bomb": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Bomb, false);
                case "Swarms": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.Swarms, false);
                case "ShadowClone": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.ShadowClone, false);
                case "TeamDeath": return new ArtifactModifier(RoR2.RoR2Content.Artifacts.TeamDeath, false);



                default:
                    {
                        if (def.Contains("Printer"))
                        {
                            string itemName = def.Split(' ')[0];
                            ItemIndex itemIndex = RoR2.ItemCatalog.FindItemIndex(itemName);
                            if (itemIndex == ItemIndex.None)
                            {
                                Log.Error($"Could not find item by the name {itemName}");
                                return null;
                            }
                            PrinterItemModifier mod = new PrinterItemModifier(itemName);
                            return mod;
                        }
                        else
                        {
                            return null;
                        }
                    }
            }
        }
    }
}
