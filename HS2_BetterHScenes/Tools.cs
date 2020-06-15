namespace HS2_BetterHScenes
{
    public static class Tools
    {
        public static int mode;
        public static int modeCtrl;

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


            return false;
        }

        public static bool IsInsert()
        {


            return false;
        }
    }
}