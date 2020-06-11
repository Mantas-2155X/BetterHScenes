namespace HS2_BetterHScenes
{
    public static class Tools
    {
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
    }
}