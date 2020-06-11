using AIChara;
using AIProject;

namespace AI_BetterHScenes
{
    public static class Tools
    {
        public enum CleanCum
        {
            Off,
            MerchantOnly,
            AgentsOnly,
            All
        }
        
        public enum OffHStartAnimChange
        {
            Off,
            OnHStart,
            OnHStartAndAnimChange
        }
        
        public enum OffWeaknessAlways
        {
            Off,
            WeaknessOnly,
            Always
        }

        public enum ClothesStrip
        {
            Off,
            Half,
            All
        }

        public enum AutoFinish
        {
            Off,
            ServiceOnly,
            InsertOnly,
            Both
        }

        public enum AutoServicePrefer
        {
            Drink,
            Spit,
            Outside,
            Random
        }

        public enum AutoInsertPrefer
        {
            Inside,
            Outside,
            Same,
            Random
        }

        public static readonly string[] finishFindTransforms =
        {
            "finishDrinkTex",
            "finishVomitTex",
            "finishOutTex",
            "finishInTex",
            "finishSynchroTex"
        };
        
        public static float Remap(float value, float from1, float to1, float from2, float to2) 
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
        
        public static bool newChangebuttonactive()
        {
            if (AI_BetterHScenes.keepButtonsInteractive.Value && AI_BetterHScenes.hFlagCtrl.nowOrgasm)
                return true;
            
            return !AI_BetterHScenes.hFlagCtrl.nowOrgasm;
        }

        public static bool newUIDisable()
        {
            return AI_BetterHScenes.keepButtonsInteractive.Value;
        }
        
        public static int ChangeSiruIndex()
        {
            switch (AI_BetterHScenes.cleanCumAfterH.Value)
            {
                case CleanCum.Off:
                    return 5;
                case CleanCum.All:
                case CleanCum.MerchantOnly when AI_BetterHScenes.manager != null && AI_BetterHScenes.manager.bMerchant:
                    return 0;
                default:
                    return 0;
            }
        }
        
        public static void CleanUpSiru(AgentStateAction __instance)
        {
            if (AI_BetterHScenes.shouldCleanUp == null || AI_BetterHScenes.shouldCleanUp.Count == 0 || __instance == null || __instance.Owner == null)
                return;
            
            var tree = (__instance.Owner as AgentBehaviorTree);
            if (tree == null)
                return;

            var agent = tree.SourceAgent;
            if (agent == null || agent.ChaControl == null || !AI_BetterHScenes.shouldCleanUp.Contains(agent.ChaControl))
                return;

            for (var i = 0; i < 5; i++)
            {
                var parts = (ChaFileDefine.SiruParts)i;
                agent.ChaControl.SetSiruFlag(parts, 0);
            }

            AI_BetterHScenes.shouldCleanUp.Remove(agent.ChaControl);
        }
    }
}