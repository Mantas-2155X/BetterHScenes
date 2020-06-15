using System;
using System.Linq;
using System.Collections.Generic;

using HarmonyLib;

using BepInEx;
using BepInEx.Configuration;

namespace HS2_BetterHScenes
{
    [BepInPlugin(nameof(HS2_BetterHScenes), nameof(HS2_BetterHScenes), VERSION)]
    [BepInProcess("HoneySelect2")]
    public class HS2_BetterHScenes : BaseUnityPlugin
    {
        public const string VERSION = "2.4.0";

        private static HS2_BetterHScenes instance;
        
        private static HScene hScene;
        private static HSceneSprite hSprite;
        private static HSceneFlagCtrl hFlagCtrl;
        private static CameraControl_Ver2 cameraCtrl;
        
        private static Traverse hSceneTrav;
        private static Traverse hFlagCtrlTrav;

        private static Harmony coreHarmony;
        private static Harmony extraHarmony;

        private static readonly List<Tools.ClothesStrip> malesStrip = new List<Tools.ClothesStrip> {0, 0, 0, 0, 0, 0, 0, 0};
        private static readonly List<Tools.ClothesStrip> femalesStrip = new List<Tools.ClothesStrip> {0, 0, 0, 0, 0, 0, 0, 0};
        
        //-- Clothes --//
        private static ConfigEntry<bool> preventDefaultAnimationChangeStrip { get; set; }
        
        private static ConfigEntry<Tools.OffHStartAnimChange> stripMaleClothes { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripMaleTop { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripMaleBottom { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripMaleBra { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripMalePanties { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripMaleGloves { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripMalePantyhose { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripMaleSocks { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripMaleShoes { get; set; }
        
        private static ConfigEntry<Tools.OffHStartAnimChange> stripFemaleClothes { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripFemaleTop { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripFemaleBottom { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripFemaleBra { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripFemalePanties { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripFemaleGloves { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripFemalePantyhose { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripFemaleSocks { get; set; }
        private static ConfigEntry<Tools.ClothesStrip> stripFemaleShoes { get; set; }
        
        //-- Weakness --//
        private static ConfigEntry<int> countToWeakness { get; set; }
        private static ConfigEntry<Tools.OffWeaknessAlways> forceTears { get; set; }
        private static ConfigEntry<Tools.OffWeaknessAlways> forceCloseEyes { get; set; }
        private static ConfigEntry<Tools.OffWeaknessAlways> forceStopBlinking { get; set; }
        private static ConfigEntry<Tools.OffWeaknessAlways> alwaysGaugesHeart { get; set; }

        //-- Cum --//
        private static ConfigEntry<Tools.AutoFinish> autoFinish { get; set; }
        private static ConfigEntry<Tools.AutoServicePrefer> autoServicePrefer { get; set; }
        private static ConfigEntry<Tools.AutoInsertPrefer> autoInsertPrefer { get; set; }
        
        //-- General --//
        private static ConfigEntry<bool> unlockCamera { get; set; }
        
        private void Awake()
        {
            instance = this;
            
            preventDefaultAnimationChangeStrip = Config.Bind("QoL > Clothes", "Prevent default animationchange strip", true, new ConfigDescription("Prevent default animation change clothes strip (pants, panties, top half state)"));
            
            stripMaleClothes = Config.Bind("QoL > Clothes", "Should strip male clothes", Tools.OffHStartAnimChange.OnHStart, new ConfigDescription("Should strip male clothes during H"));
            stripMaleTop = Config.Bind("QoL > Clothes", "Strip male top", Tools.ClothesStrip.All, new ConfigDescription("Strip male top during H"));
            stripMaleBottom = Config.Bind("QoL > Clothes", "Strip male bottom", Tools.ClothesStrip.All, new ConfigDescription("Strip male bottom during H"));
            stripMaleBra = Config.Bind("QoL > Clothes", "Strip male bra", Tools.ClothesStrip.Half, new ConfigDescription("Strip male (futa) bra during H"));
            stripMalePanties = Config.Bind("QoL > Clothes", "Strip male panties", Tools.ClothesStrip.Half, new ConfigDescription("Strip male (futa) panties during H"));
            stripMaleGloves = Config.Bind("QoL > Clothes", "Strip male gloves", Tools.ClothesStrip.Off, new ConfigDescription("Strip male gloves during H"));
            stripMalePantyhose = Config.Bind("QoL > Clothes", "Strip male pantyhose", Tools.ClothesStrip.Half, new ConfigDescription("Strip male (futa) pantyhose during H"));
            stripMaleSocks = Config.Bind("QoL > Clothes", "Strip male socks", Tools.ClothesStrip.Off, new ConfigDescription("Strip male (futa) socks during H"));
            stripMaleShoes = Config.Bind("QoL > Clothes", "Strip male shoes", Tools.ClothesStrip.Off, new ConfigDescription("Strip male shoes during H"));

            stripFemaleClothes = Config.Bind("QoL > Clothes", "Should strip female clothes", Tools.OffHStartAnimChange.Both, new ConfigDescription("Should strip female clothes during H"));
            stripFemaleTop = Config.Bind("QoL > Clothes", "Strip female top", Tools.ClothesStrip.Half, new ConfigDescription("Strip female top during H"));
            stripFemaleBottom = Config.Bind("QoL > Clothes", "Strip female bottom", Tools.ClothesStrip.Half, new ConfigDescription("Strip female bottom during H"));
            stripFemaleBra = Config.Bind("QoL > Clothes", "Strip female bra", Tools.ClothesStrip.Half, new ConfigDescription("Strip female bra during H"));
            stripFemalePanties = Config.Bind("QoL > Clothes", "Strip female panties", Tools.ClothesStrip.Half, new ConfigDescription("Strip female panties during H"));
            stripFemaleGloves = Config.Bind("QoL > Clothes", "Strip female gloves", Tools.ClothesStrip.Off, new ConfigDescription("Strip female gloves during H"));
            stripFemalePantyhose = Config.Bind("QoL > Clothes", "Strip female pantyhose", Tools.ClothesStrip.Half, new ConfigDescription("Strip female pantyhose during H"));
            stripFemaleSocks = Config.Bind("QoL > Clothes", "Strip female socks", Tools.ClothesStrip.Off, new ConfigDescription("Strip female socks during H"));
            stripFemaleShoes = Config.Bind("QoL > Clothes", "Strip female shoes", Tools.ClothesStrip.Off, new ConfigDescription("Strip female shoes during H"));
         
            countToWeakness = Config.Bind("QoL > Weakness", "Orgasm count until weakness", 3, new ConfigDescription("How many times does the girl have to orgasm to reach weakness", new AcceptableValueRange<int>(1, 999)));
            forceTears = Config.Bind("QoL > Weakness", "Force show tears", Tools.OffWeaknessAlways.WeaknessOnly, new ConfigDescription("Make girl cry"));
            forceCloseEyes = Config.Bind("QoL > Weakness", "Force close eyes", Tools.OffWeaknessAlways.Off, new ConfigDescription("Close girl eyes"));
            forceStopBlinking = Config.Bind("QoL > Weakness", "Force stop blinking", Tools.OffWeaknessAlways.Off, new ConfigDescription("Stop blinking"));
            alwaysGaugesHeart = Config.Bind("QoL > Weakness", "Always hit gauge heart", Tools.OffWeaknessAlways.WeaknessOnly, new ConfigDescription("Always hit gauge heart. Will cause progress to increase without having to scroll specific amount"));

            autoFinish = Config.Bind("QoL > Cum", "Auto finish", Tools.AutoFinish.Both, new ConfigDescription("Automatically finish inside when both gauges reach max"));
            autoServicePrefer = Config.Bind("QoL > Cum", "Preferred auto service finish", Tools.AutoServicePrefer.Drink, new ConfigDescription("Preferred auto finish type. Will fall back to any available option if selected is not available"));
            autoInsertPrefer = Config.Bind("QoL > Cum", "Preferred auto insert finish", Tools.AutoInsertPrefer.Same, new ConfigDescription("Preferred auto finish type. Will fall back to any available option if selected is not available"));

            unlockCamera = Config.Bind("QoL > General", "Unlock camera movement", true, new ConfigDescription("Unlock camera zoom out / distance limit during H"));

            stripMaleTop.SettingChanged += delegate { malesStrip[0] = stripMaleTop.Value; };
            stripMaleBottom.SettingChanged += delegate { malesStrip[1] = stripMaleBottom.Value; };
            stripMaleBra.SettingChanged += delegate { malesStrip[2] = stripMaleBra.Value; };
            stripMalePanties.SettingChanged += delegate { malesStrip[3] = stripMalePanties.Value; };
            stripMaleGloves.SettingChanged += delegate { malesStrip[4] = stripMaleGloves.Value; };
            stripMalePantyhose.SettingChanged += delegate { malesStrip[5] = stripMalePantyhose.Value; };
            stripMaleSocks.SettingChanged += delegate { malesStrip[6] = stripMaleSocks.Value; };
            stripMaleShoes.SettingChanged += delegate { malesStrip[7] = stripMaleShoes.Value; };

            stripFemaleTop.SettingChanged += delegate { femalesStrip[0] = stripFemaleTop.Value; };
            stripFemaleBottom.SettingChanged += delegate { femalesStrip[1] = stripFemaleBottom.Value; };
            stripFemaleBra.SettingChanged += delegate { femalesStrip[2] = stripFemaleBra.Value; };
            stripFemalePanties.SettingChanged += delegate { femalesStrip[3] = stripFemalePanties.Value; };
            stripFemaleGloves.SettingChanged += delegate { femalesStrip[4] = stripFemaleGloves.Value; };
            stripFemalePantyhose.SettingChanged += delegate { femalesStrip[5] = stripFemalePantyhose.Value; };
            stripFemaleSocks.SettingChanged += delegate { femalesStrip[6] = stripFemaleSocks.Value; };
            stripFemaleShoes.SettingChanged += delegate { femalesStrip[7] = stripFemaleShoes.Value; };
            
            countToWeakness.SettingChanged += delegate
            {
                if (hFlagCtrlTrav == null)
                    return;
                
                hFlagCtrlTrav.Field("gotoFaintnessCount").SetValue(countToWeakness.Value);
            };
            
            unlockCamera.SettingChanged += delegate
            {
                if (cameraCtrl == null)
                    return;
                
                cameraCtrl.isLimitDir = !unlockCamera.Value;
                cameraCtrl.isLimitPos = !unlockCamera.Value;
            };
            
            coreHarmony = new Harmony("HS2_BetterHScenes_Core");
            extraHarmony = new Harmony("HS2_BetterHScenes_Extra");

            coreHarmony.Patch(
                typeof(HScene).GetMethod("SetStartAnimationInfo"), 
                null, 
                new HarmonyMethod(typeof(HS2_BetterHScenes).GetMethod("HScene_SetStartAnimationInfo_CorePostfix")));
            
            coreHarmony.Patch(
                typeof(HScene).GetMethod("EndProc"), 
                null, 
                new HarmonyMethod(typeof(HS2_BetterHScenes).GetMethod("HScene_EndProc_CorePostfix")));

            enabled = false;
        }

        //-- Autofinish --//
        private void Update()
        {
            if (autoFinish.Value == Tools.AutoFinish.Off) 
                return;
            
            if (hFlagCtrl.feel_m >= 0.98f && Tools.IsService() && autoFinish.Value != (Tools.AutoFinish)2)
            {
                var drink = hSprite.IsFinishVisible(1);
                var vomit = hSprite.IsFinishVisible(3);
                var onbody = hSprite.IsFinishVisible(4);

                switch (autoServicePrefer.Value)
                {
                    case Tools.AutoServicePrefer.Drink when drink:
                        hFlagCtrl.click = HSceneFlagCtrl.ClickKind.FinishDrink;
                        break;
                    case Tools.AutoServicePrefer.Spit when vomit:
                        hFlagCtrl.click = HSceneFlagCtrl.ClickKind.FinishVomit;
                        break;
                    case Tools.AutoServicePrefer.Outside when onbody:
                        hFlagCtrl.click = HSceneFlagCtrl.ClickKind.FinishOutSide;
                        break;
                    case Tools.AutoServicePrefer.Random:
                        var random = new List<HSceneFlagCtrl.ClickKind>();
                        if (drink)
                            random.Add(HSceneFlagCtrl.ClickKind.FinishDrink);
                        if (vomit)
                            random.Add(HSceneFlagCtrl.ClickKind.FinishVomit);
                        if (onbody)
                            random.Add(HSceneFlagCtrl.ClickKind.FinishOutSide);

                        if (random.Count < 1)
                            break;

                        var rand = new Random();
                        hFlagCtrl.click = random[rand.Next(random.Count)];
                        break;
                }
            }
            else if (hFlagCtrl.feel_f >= 0.98f && hFlagCtrl.feel_m >= 0.98f && Tools.IsInsert() && autoFinish.Value != (Tools.AutoFinish)1)
            {
                var inside = hSprite.IsFinishVisible(1);
                var outside = hSprite.IsFinishVisible(5);
                var same = hSprite.IsFinishVisible(2);

                switch (autoInsertPrefer.Value)
                {
                    case Tools.AutoInsertPrefer.Inside when inside:
                        hFlagCtrl.click = HSceneFlagCtrl.ClickKind.FinishInSide;
                        break;
                    case Tools.AutoInsertPrefer.Outside when outside:
                        hFlagCtrl.click = HSceneFlagCtrl.ClickKind.FinishOutSide;
                        break;
                    case Tools.AutoInsertPrefer.Same when same:
                        hFlagCtrl.click = HSceneFlagCtrl.ClickKind.FinishSame;
                        break;
                    case Tools.AutoInsertPrefer.Random:
                        var random = new List<HSceneFlagCtrl.ClickKind>();
                        if (inside)
                            random.Add(HSceneFlagCtrl.ClickKind.FinishInSide);
                        if (outside)
                            random.Add(HSceneFlagCtrl.ClickKind.FinishOutSide);
                        if (same)
                            random.Add(HSceneFlagCtrl.ClickKind.FinishSame);

                        if (random.Count < 1)
                            break;

                        var rand = new Random();
                        hFlagCtrl.click = random[rand.Next(random.Count)];
                        break;
                }
            }
        }
        
        //-- Start of HScene --//
        public static void HScene_SetStartAnimationInfo_CorePostfix(HScene __instance, HSceneSprite ___sprite)
        {
            hScene = __instance;
            hSprite = ___sprite;
            
            if (hScene == null || hSprite == null)
                return;
            
            hFlagCtrl = hScene.ctrlFlag;
            if (hFlagCtrl == null)
                return;
            
            cameraCtrl = hFlagCtrl.cameraCtrl;
            if (cameraCtrl == null)
                return;

            hSceneTrav = Traverse.Create(hScene);
            hFlagCtrlTrav = Traverse.Create(hFlagCtrl);
            
            extraHarmony.PatchAll(typeof(HS2_BetterHScenes));
            
            instance.enabled = true;
            
            if (unlockCamera.Value)
            {
                cameraCtrl.isLimitDir = false;
                cameraCtrl.isLimitPos = false;
            }

            hFlagCtrlTrav.Field("gotoFaintnessCount").SetValue(countToWeakness.Value);
            
            HScene_StripClothes(
                stripMaleClothes.Value == Tools.OffHStartAnimChange.OnHStart || stripMaleClothes.Value == Tools.OffHStartAnimChange.Both, 
                stripFemaleClothes.Value == Tools.OffHStartAnimChange.OnHStart || stripMaleClothes.Value == Tools.OffHStartAnimChange.Both
                );
        }
        
        //-- End of HScene --//
        public static void HScene_EndProc_CorePostfix()
        {
            hScene = null;
            hSprite = null;
            hFlagCtrl = null;
            
            instance.enabled = false;
            
            extraHarmony.UnpatchAll("HS2_BetterHScenes_Extra");
        }
        
        //-- Cache current animation mode --//
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "setCameraLoad")]
        public static void HScene_setCameraLoad_CacheMode()
        {
            Tools.mode = hSceneTrav.Field("mode").GetValue<int>();
            Tools.modeCtrl = hSceneTrav.Field("modeCtrl").GetValue<int>();
        }
        
        //-- Always gauges heart --//
        [HarmonyPostfix, HarmonyPatch(typeof(FeelHit), "isHit")]
        public static void FeelHit_isHit_AlwaysGaugesHeart(ref bool __result)
        {
            if(alwaysGaugesHeart.Value == Tools.OffWeaknessAlways.Always || alwaysGaugesHeart.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness)
                __result = true;
        }
        
        //-- Tears, close eyes, stop blinking --//
        [HarmonyPrefix, HarmonyPatch(typeof(HVoiceCtrl), "SetFace")]
        public static void HVoiceCtrl_SetFace_ForceTearsOnWeakness(ref HVoiceCtrl.FaceInfo _face)
        {
            if (_face == null)
                return;

            if(forceTears.Value == Tools.OffWeaknessAlways.Always || forceTears.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness) 
                _face.tear = 1f;

            if(forceCloseEyes.Value == Tools.OffWeaknessAlways.Always || forceCloseEyes.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness)
                _face.openEye = 0.05f;
            
            if(forceStopBlinking.Value == Tools.OffWeaknessAlways.Always || forceStopBlinking.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness)
                _face.blink = false;
        }

        //-- Prevent default animation change clothes strip --//
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "SetClothStateStartMotion")]
        public static bool HScene_SetClothStateStartMotion_PreventDefaultClothesStrip() => !preventDefaultAnimationChangeStrip.Value;
        
        //-- Strip clothes when changing animation --//
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "ChangeAnimVoiceFlag")]
        public static void HScene_ChangeAnimVoiceFlag_StripClothes() => HScene_StripClothes(stripMaleClothes.Value > (Tools.OffHStartAnimChange)1, stripFemaleClothes.Value > (Tools.OffHStartAnimChange)1);

        private static void HScene_StripClothes(bool stripMales, bool stripFemales)
        {
            if (stripMales)
            {
                var males = hScene.GetMales();
                if (males != null && males.Length > 0)
                    foreach (var male in males.Where(male => male != null))
                        for (var i = 0; i < malesStrip.Count; i++)
                            if(malesStrip[i] > 0 && male.IsClothesStateKind(i) && male.fileStatus.clothesState[i] != 2)
                                male.SetClothesState(i, (byte)malesStrip[i]);
            }
            
            if (stripFemales)
            {
                var females = hScene.GetFemales();
                if (females != null && females.Length > 0)
                    foreach (var female in females.Where(female => female != null))
                        for (var i = 0; i < femalesStrip.Count; i++)
                            if(femalesStrip[i] > 0 && female.IsClothesStateKind(i) && female.fileStatus.clothesState[i] != 2)
                                female.SetClothesState(i, (byte)femalesStrip[i]);
            }
        }
    }
}