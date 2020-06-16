using HarmonyLib;

namespace HS2_BetterHScenes
{
    public static class Tools
    {
        public static int mode;
        public static int modeCtrl;
        
        public static Traverse hFlagCtrlTrav;

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

        public static bool IsService()
        {
            // Houshi
            if (mode == 1) 
                return true;

            // FFM & MMF
            if ((mode == 7 || mode == 8) && (modeCtrl == 1 || modeCtrl == 2)) 
                return true;
            
            return false;
        }

        public static bool IsInsert()
        {
            // Sonyu
            if (mode == 2) 
                return true;

            // FFM & MMF
            if ((mode == 7 || mode == 8) && (modeCtrl == 3 || modeCtrl == 4)) 
                return true;
            
            return false;
        }
        
        public static void SetGotoWeaknessCount(int num) => hFlagCtrlTrav?.Field("gotoFaintnessCount").SetValue(num);
    }
}