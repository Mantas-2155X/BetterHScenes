using System.Linq;
using System.Collections.Generic;

using HarmonyLib;

using BepInEx;
using BepInEx.Harmony;
using BepInEx.Configuration;

using Manager;

namespace HS2_BetterHScenes
{
    [BepInPlugin(nameof(HS2_BetterHScenes), nameof(HS2_BetterHScenes), VERSION)]
    [BepInProcess("HoneySelect2")]
    public class HS2_BetterHScenes : BaseUnityPlugin
    {
        public const string VERSION = "2.3.0";

        private static HScene hScene;
        private static HSceneSprite hSprite;
        private static HSceneFlagCtrl hFlagCtrl;

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
        
        //-- Cum --//
        private static ConfigEntry<Tools.AutoFinish> autoFinish { get; set; }
        private static ConfigEntry<Tools.AutoServicePrefer> autoServicePrefer { get; set; }
        private static ConfigEntry<Tools.AutoInsertPrefer> autoInsertPrefer { get; set; }
        
        //-- General --//
        private static ConfigEntry<Tools.OffWeaknessAlways> alwaysGaugesHeart { get; set; }
        private static ConfigEntry<bool> unlockCamera { get; set; }

        private void Awake()
        {
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

            stripFemaleClothes = Config.Bind("QoL > Clothes", "Should strip female clothes", Tools.OffHStartAnimChange.OnHStartAndAnimChange, new ConfigDescription("Should strip female clothes during H"));
            stripFemaleTop = Config.Bind("QoL > Clothes", "Strip female top", Tools.ClothesStrip.Half, new ConfigDescription("Strip female top during H"));
            stripFemaleBottom = Config.Bind("QoL > Clothes", "Strip female bottom", Tools.ClothesStrip.Half, new ConfigDescription("Strip female bottom during H"));
            stripFemaleBra = Config.Bind("QoL > Clothes", "Strip female bra", Tools.ClothesStrip.Half, new ConfigDescription("Strip female bra during H"));
            stripFemalePanties = Config.Bind("QoL > Clothes", "Strip female panties", Tools.ClothesStrip.Half, new ConfigDescription("Strip female panties during H"));
            stripFemaleGloves = Config.Bind("QoL > Clothes", "Strip female gloves", Tools.ClothesStrip.Off, new ConfigDescription("Strip female gloves during H"));
            stripFemalePantyhose = Config.Bind("QoL > Clothes", "Strip female pantyhose", Tools.ClothesStrip.Half, new ConfigDescription("Strip female pantyhose during H"));
            stripFemaleSocks = Config.Bind("QoL > Clothes", "Strip female socks", Tools.ClothesStrip.Off, new ConfigDescription("Strip female socks during H"));
            stripFemaleShoes = Config.Bind("QoL > Clothes", "Strip female shoes", Tools.ClothesStrip.Off, new ConfigDescription("Strip female shoes during H"));
         
            countToWeakness = Config.Bind("QoL > Weakness", "Orgasm count until weakness", 3, new ConfigDescription("How many times does the girl have to orgasm to reach weakness", new AcceptableValueRange<int>(1, 999)));
            forceTears = Config.Bind("QoL > Weakness", "Tears when weakness is reached", Tools.OffWeaknessAlways.WeaknessOnly, new ConfigDescription("Make girl cry when weakness is reached during H"));
            forceCloseEyes = Config.Bind("QoL > Weakness", "Close eyes when weakness is reached", Tools.OffWeaknessAlways.Off, new ConfigDescription("Close girl eyes when weakness is reached during H"));
            forceStopBlinking = Config.Bind("QoL > Weakness", "Stop blinking when weakness is reached", Tools.OffWeaknessAlways.Off, new ConfigDescription("Stop blinking when weakness is reached during H"));

            autoFinish = Config.Bind("QoL > Cum", "Auto finish", Tools.AutoFinish.Off, new ConfigDescription("Automatically finish inside when both gauges reach max"));
            autoServicePrefer = Config.Bind("QoL > Cum", "Preferred auto service finish", Tools.AutoServicePrefer.Drink, new ConfigDescription("Preferred auto finish type. Will fall back to any available option if selected is not available"));
            autoInsertPrefer = Config.Bind("QoL > Cum", "Preferred auto insert finish", Tools.AutoInsertPrefer.Same, new ConfigDescription("Preferred auto finish type. Will fall back to any available option if selected is not available"));

            alwaysGaugesHeart = Config.Bind("QoL > General", "Always hit gauge heart", Tools.OffWeaknessAlways.WeaknessOnly, new ConfigDescription("Always hit gauge heart. Will cause progress to increase without having to scroll specific amount"));
            unlockCamera = Config.Bind("QoL > General", "Unlock camera movement", true, new ConfigDescription("Unlock camera zoom out / distance limit during H"));

            countToWeakness.SettingChanged += delegate
            {
                if (!HSceneManager.isHScene || hFlagCtrl == null)
                    return;
                
                Traverse.Create(hFlagCtrl).Field("gotoFaintnessCount").SetValue(countToWeakness.Value);
            };
            
            unlockCamera.SettingChanged += delegate
            {
                if (!HSceneManager.isHScene || hFlagCtrl == null || hFlagCtrl.cameraCtrl == null)
                    return;
                
                hFlagCtrl.cameraCtrl.isLimitDir = !unlockCamera.Value;
                hFlagCtrl.cameraCtrl.isLimitPos = !unlockCamera.Value;
            };
            
            HarmonyWrapper.PatchAll(typeof(HS2_BetterHScenes));
        }

        private void Update()
        {
            if (autoFinish.Value == Tools.AutoFinish.Off || hFlagCtrl == null || hSprite == null || hSprite.categoryFinish == null) 
                return;
            
            var mode = Traverse.Create(hScene).Field("mode").GetValue<int>();
            var modeCtrl = Traverse.Create(hScene).Field("modeCtrl").GetValue<int>();
            
            if (hFlagCtrl.feel_m >= 0.98f && (autoFinish.Value == Tools.AutoFinish.ServiceOnly || autoFinish.Value == Tools.AutoFinish.Both) && (mode == 1 || (mode == 7 || mode == 8) && modeCtrl == 2))
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

                        var rand = new System.Random();
                        hFlagCtrl.click = random[rand.Next(random.Count)];

                        break;
                    default:
                        hFlagCtrl.click = drink ? HSceneFlagCtrl.ClickKind.FinishDrink : vomit ? HSceneFlagCtrl.ClickKind.FinishVomit : onbody ? HSceneFlagCtrl.ClickKind.FinishOutSide : HSceneFlagCtrl.ClickKind.None;
                        break;
                }
            }
            else if (hFlagCtrl.feel_f >= 0.98f && hFlagCtrl.feel_m >= 0.98f && (autoFinish.Value == Tools.AutoFinish.InsertOnly || autoFinish.Value == Tools.AutoFinish.Both) && (mode == 2 || mode == 7 && (modeCtrl == 3 || modeCtrl == 4) || mode == 8 && modeCtrl == 3))
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

                        var rand = new System.Random();
                        hFlagCtrl.click = random[rand.Next(random.Count)];

                        break;
                    default:
                        hFlagCtrl.click = inside ? HSceneFlagCtrl.ClickKind.FinishInSide : outside ? HSceneFlagCtrl.ClickKind.FinishOutSide : same ? HSceneFlagCtrl.ClickKind.FinishSame : HSceneFlagCtrl.ClickKind.None;
                        break;
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "SetStartAnimationInfo")]
        public static void HScene_SetStartAnimationInfo_Patch(HScene __instance, HSceneSprite ___sprite)
        {
            hScene = __instance;
            hSprite = ___sprite;
            hFlagCtrl = hScene.ctrlFlag;
            
            if (hFlagCtrl.cameraCtrl != null && unlockCamera.Value)
            {
                hFlagCtrl.cameraCtrl.isLimitDir = false;
                hFlagCtrl.cameraCtrl.isLimitPos = false;
            }

            Traverse.Create(hFlagCtrl).Field("gotoFaintnessCount").SetValue(countToWeakness.Value);
            
            HScene_StripClothes(stripMaleClothes.Value == Tools.OffHStartAnimChange.OnHStart || stripFemaleClothes.Value == Tools.OffHStartAnimChange.OnHStart);
        }
        
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "EndProc")]
        public static void HScene_EndProc_Patch()
        {
            hScene = null;
            hSprite = null;
            hFlagCtrl = null;
        }
        
        //-- Prevent default animation change clothes strip --//
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "SetClothStateStartMotion")]
        public static bool HScene_SetClothStateStartMotion_PreventDefaultClothesStrip()
        {
            return !HSceneManager.isHScene || !preventDefaultAnimationChangeStrip.Value;
        }
        
        //-- Always gauges heart --//
        [HarmonyPostfix, HarmonyPatch(typeof(FeelHit), "isHit")]
        public static void FeelHit_isHit_AlwaysGaugesHeart(ref bool __result)
        {
            if(HSceneManager.isHScene && alwaysGaugesHeart.Value == Tools.OffWeaknessAlways.Always || alwaysGaugesHeart.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl != null && hFlagCtrl.isFaintness)
                __result = true;
        }
        
        //-- Tears, close eyes, stop blinking --//
        [HarmonyPrefix, HarmonyPatch(typeof(HVoiceCtrl), "SetFace")]
        public static void HVoiceCtrl_SetFace_ForceTearsOnWeakness(ref HVoiceCtrl.FaceInfo _face)
        {
            if (!HSceneManager.isHScene || _face == null)
                return;

            if(forceTears.Value == Tools.OffWeaknessAlways.Always || forceTears.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness) 
                _face.tear = 1f;

            if(forceCloseEyes.Value == Tools.OffWeaknessAlways.Always || forceCloseEyes.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness)
                _face.openEye = 0.05f;
            
            if(forceStopBlinking.Value == Tools.OffWeaknessAlways.Always || forceStopBlinking.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness)
                _face.blink = false;
        }
        
        //-- Strip clothes when changing animation --//
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "ChangeAnimation")]
        private static void HScene_ChangeAnimation_StripClothes() => HScene_StripClothes(stripMaleClothes.Value == Tools.OffHStartAnimChange.OnHStartAndAnimChange || stripFemaleClothes.Value == Tools.OffHStartAnimChange.OnHStartAndAnimChange);

        private static void HScene_StripClothes(bool shouldStrip)
        {
            if (!HSceneManager.isHScene || !shouldStrip || hScene == null)
                return;

            var stripMales = stripMaleClothes.Value != Tools.OffHStartAnimChange.Off;
            var stripFemales = stripFemaleClothes.Value != Tools.OffHStartAnimChange.Off;

            var males = hScene.GetMales();
            var females = hScene.GetFemales();
            
            if (stripMales && males != null && males.Length > 0)
            {
                var stripAmounts = new Dictionary<int, Tools.ClothesStrip>
                {
                    {0, stripMaleTop.Value},
                    {1, stripMaleBottom.Value},
                    {2, stripMaleBra.Value},
                    {3, stripMalePanties.Value},
                    {4, stripMaleGloves.Value},
                    {5, stripMalePantyhose.Value},
                    {6, stripMaleSocks.Value},
                    {7, stripMaleShoes.Value}
                };

                foreach (var male in males.Where(male => male != null))
                    foreach (var strip in stripAmounts.Where(strip => strip.Value > 0 && male.IsClothesStateKind(strip.Key) && male.fileStatus.clothesState[strip.Key] != 2))
                        male.SetClothesState(strip.Key, (byte)strip.Value);
            }
            
            if (stripFemales && females != null && females.Length > 0)
            {
                var stripAmounts = new Dictionary<int, Tools.ClothesStrip>
                {
                    {0, stripFemaleTop.Value},
                    {1, stripFemaleBottom.Value},
                    {2, stripFemaleBra.Value},
                    {3, stripFemalePanties.Value},
                    {4, stripFemaleGloves.Value},
                    {5, stripFemalePantyhose.Value},
                    {6, stripFemaleSocks.Value},
                    {7, stripFemaleShoes.Value}
                };

                foreach (var female in females.Where(female => female != null))
                    foreach (var strip in stripAmounts.Where(strip => strip.Value > 0 && female.IsClothesStateKind(strip.Key) && female.fileStatus.clothesState[strip.Key] != 2))
                        female.SetClothesState(strip.Key, (byte)strip.Value);
            }
        }
    }
}