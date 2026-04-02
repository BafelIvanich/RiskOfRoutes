using RoR2;
using SamplePlugin.StageModifiers;

namespace RiskOfRoutes.StageModifiers.OtherModifiers
{
    public class UpgradeDronesModifier : StageModifier
    {
        public UpgradeDronesModifier(int stack)
        {
            negative = false;
            this.stack = stack;
        }

        public override void OnEnd()
        {
            Log.Info("Droen upgrade end");
        }

        public override void OnStart()
        {
            foreach (CharacterMaster cm in CharacterMaster.readOnlyInstancesList)
            {
                if (cm == null) continue;
                CharacterBody cb = cm.GetBody();
                MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(cm.netId);

                if (minionGroup == null)
                {
                    continue;
                }
                MinionOwnership[] members = minionGroup.members;
                if (members == null || members.Length == 0) continue;
                foreach (MinionOwnership minionOwnership in members)
                {
                    if (!minionOwnership)
                    {
                        continue;
                    }
                    CharacterMaster master = minionOwnership.GetComponent<CharacterMaster>();
                    if (master && master.inventory)
                    {
                        master.inventory.GiveItemPermanent(DLC3Content.Items.DroneUpgradeHidden, stack);
                    }
                }
            }
        }
    }
}
