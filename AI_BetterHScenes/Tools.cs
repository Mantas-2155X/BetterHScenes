using AIChara;
using AIProject;
using HarmonyLib;

namespace AI_BetterHScenes
{
    public static class Tools
    {
        public static int mode;
        public static int modeCtrl;
        
        public static Traverse hFlagCtrlTrav;
        
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
            OnAnimChange,
            Both
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

        public static bool newChangebuttonactive()
        {
            if (AI_BetterHScenes.hFlagCtrl == null)
                return false;
            
            if (AI_BetterHScenes.keepButtonsInteractive.Value && AI_BetterHScenes.hFlagCtrl.nowOrgasm)
                return true;
            
            return !AI_BetterHScenes.hFlagCtrl.nowOrgasm;
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
            if (AI_BetterHScenes.shouldCleanUp == null || AI_BetterHScenes.shouldCleanUp.Count == 0 || __instance.Owner == null)
                return;
            
            var tree = __instance.Owner as AgentBehaviorTree;
            if (tree == null)
                return;

            var agent = tree.SourceAgent;
            if (agent == null || agent.ChaControl == null || !AI_BetterHScenes.shouldCleanUp.Contains(agent.ChaControl))
                return;

            for (var i = 0; i < 5; i++)
                agent.ChaControl.SetSiruFlag((ChaFileDefine.SiruParts)i, 0);

            AI_BetterHScenes.shouldCleanUp.Remove(agent.ChaControl);
        }
        
        public static bool IsService()
        {
            // Houshi
            if (mode == 1) 
                return true;

            // FFM
            if (mode == 7 && (modeCtrl == 1 || modeCtrl == 2)) 
                return true;
            
            return false;
        }

        public static bool IsInsert()
        {
            // Sonyu
            if (mode == 2) 
                return true;

            // FFM
            if (mode == 7 && (modeCtrl == 3 || modeCtrl == 4)) 
                return true;
            
            return false;
        }
        
        public static float Remap(float value, float from1, float to1, float from2, float to2) => (value - from1) / (to1 - from1) * (to2 - from2) + from2;

        public static void SetGotoWeaknessCount(int num) => hFlagCtrlTrav?.Field("gotoFaintnessCount").SetValue(num);
        
        public static bool newUIDisable() => AI_BetterHScenes.keepButtonsInteractive.Value;
    }
}