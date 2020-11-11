using System;
using System.Linq;
using System.Collections.Generic;

using HarmonyLib;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

using UnityEngine;
using UnityEngine.SceneManagement;

using AIChara;

using Random = System.Random;

namespace HS2_BetterHScenes
{
    [BepInPlugin(nameof(HS2_BetterHScenes), nameof(HS2_BetterHScenes), VERSION)]
    [BepInProcess("HoneySelect2")]
    public class HS2_BetterHScenes : BaseUnityPlugin
    {
        public const string VERSION = "2.5.5";
        
        public new static ManualLogSource Logger;

        private static readonly Random rand = new Random();
        
        private static HScene hScene;
        public static HSceneFlagCtrl hFlagCtrl;
        private static HSceneSprite hSprite;
        private static CameraControl_Ver2 cameraCtrl;

        private static Traverse hSceneTrav;
        private static Harmony harmony;
        
        public static List<ChaControl> characters;


        private static bool activeUI;
        private static bool cameraShouldLock;

        public static AnimationOffsets animationOffsets;
        private static bool shouldApplyOffsets;
        public static string currentMotion;
		public static int maleCount;

        //-- Draggers --//
        private static ConfigEntry<KeyboardShortcut> showDraggerUI { get; set; }
        private static ConfigEntry<bool> applySavedOffsets { get; set; }
        public static ConfigEntry<bool> useOneOffsetForAllMotions { get; private set; }
        public static ConfigEntry<string> offsetFile { get; private set; }
        public static ConfigEntry<float> sliderMaxPosition { get; private set; }
        public static ConfigEntry<float> sliderMaxRotation{ get; private set; }
        
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
            Logger = base.Logger;
            
            showDraggerUI = Config.Bind("QoL > Draggers", "Show draggers UI", new KeyboardShortcut(KeyCode.N));
            (applySavedOffsets = Config.Bind("QoL > Draggers", "Apply saved offsets", true, new ConfigDescription("Apply previously saved character offsets for character pair / position during H"))).SettingChanged += delegate
            {
                if (applySavedOffsets.Value)
                    shouldApplyOffsets = true;
            };
            
            useOneOffsetForAllMotions = Config.Bind("QoL > Draggers", "Use one offset for all motions", true, new ConfigDescription("If disabled, the Save button in the UI will only save the offsets for the current motion of the position.  A Default button will be added to save it for all motions of that position that don't already have an offset."));
            offsetFile = Config.Bind("QoL > Draggers", "Offset File Path", "UserData/BetterHScenesOffsets.xml", new ConfigDescription("Path of the offset file card on disk."));
            sliderMaxPosition = Config.Bind("QoL > Draggers", "Slider min/max position", 2.5f, new ConfigDescription("Maximum limits of the position slider bars."));
            sliderMaxRotation = Config.Bind("QoL > Draggers", "Slider min/max rotation", 45f, new ConfigDescription("Maximum limits of the rotation slider bars."));

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
         
            (countToWeakness = Config.Bind("QoL > Weakness", "Orgasm count until weakness", 3, new ConfigDescription("How many times does the girl have to orgasm to reach weakness", new AcceptableValueRange<int>(1, 999)))).SettingChanged += (s, e) => Tools.SetGotoWeaknessCount(countToWeakness.Value);
            forceTears = Config.Bind("QoL > Weakness", "Force show tears", Tools.OffWeaknessAlways.WeaknessOnly, new ConfigDescription("Make girl cry"));
            forceCloseEyes = Config.Bind("QoL > Weakness", "Force close eyes", Tools.OffWeaknessAlways.Off, new ConfigDescription("Close girl eyes"));
            forceStopBlinking = Config.Bind("QoL > Weakness", "Force stop blinking", Tools.OffWeaknessAlways.Off, new ConfigDescription("Stop blinking"));
            alwaysGaugesHeart = Config.Bind("QoL > Weakness", "Always hit gauge heart", Tools.OffWeaknessAlways.WeaknessOnly, new ConfigDescription("Always hit gauge heart. Will cause progress to increase without having to scroll specific amount"));

            autoFinish = Config.Bind("QoL > Cum", "Auto finish", Tools.AutoFinish.Both, new ConfigDescription("Automatically finish inside when both gauges reach max"));
            autoServicePrefer = Config.Bind("QoL > Cum", "Preferred auto service finish", Tools.AutoServicePrefer.Drink, new ConfigDescription("Preferred auto finish type. Will fall back to any available option if selected is not available"));
            autoInsertPrefer = Config.Bind("QoL > Cum", "Preferred auto insert finish", Tools.AutoInsertPrefer.Same, new ConfigDescription("Preferred auto finish type. Will fall back to any available option if selected is not available"));

            (unlockCamera = Config.Bind("QoL > General", "Unlock camera movement", true, new ConfigDescription("Unlock camera zoom out / distance limit during H"))).SettingChanged += (s, e) =>
            {
                if (cameraCtrl == null)
                    return;
                
                cameraCtrl.isLimitDir = !unlockCamera.Value;
                cameraCtrl.isLimitPos = !unlockCamera.Value;
            };
            
            shouldApplyOffsets = false;
            animationOffsets = new AnimationOffsets();
            HSceneOffset.LoadOffsetsFromFile();
            
            harmony = new Harmony(nameof(HS2_BetterHScenes));
            
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        //-- Draw chara draggers UI --//
        private void OnGUI()
        {
            if (activeUI && hScene != null)
                SliderUI.DrawDraggersUI();
        }
        
        //-- Apply chara offsets --//
        //-- Auto finish, togle chara draggers UI --//
        private void Update()
        {
            if (hScene == null)
                return;
            
            if (showDraggerUI.Value.IsDown())
                activeUI = !activeUI;

            if (shouldApplyOffsets && !hScene.NowChangeAnim)
            {
                HSceneOffset.ApplyCharacterOffsets();
                shouldApplyOffsets = false;
            }
            
            if (autoFinish.Value == Tools.AutoFinish.Off) 
                return;
            
            if (hFlagCtrl.feel_m >= 0.98f && Tools.IsService() && autoFinish.Value != Tools.AutoFinish.InsertOnly)
            {
                // same mode as OnBody only for some reason
                var drink = hSprite.IsFinishVisible(1) && Tools.modeCtrl != 0; 
                var vomit = hSprite.IsFinishVisible(3);
                // same mode as drink for some reason
                var onbody = hSprite.IsFinishVisible(4) || (hSprite.IsFinishVisible(1) && Tools.modeCtrl == 0);

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

                        hFlagCtrl.click = random[rand.Next(random.Count)];
                        break;
                }
            }
            else if (hFlagCtrl.feel_f >= 0.98f && hFlagCtrl.feel_m >= 0.98f && Tools.IsInsert() && autoFinish.Value != Tools.AutoFinish.ServiceOnly)
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
                        
                        hFlagCtrl.click = random[rand.Next(random.Count)];
                        break;
                }
            }
        }

        //-- IK Solver Patch --//
        [HarmonyPrefix, HarmonyPatch(typeof(RootMotion.SolverManager), "LateUpdate")]
        public static void HScene_LateUpdate(RootMotion.SolverManager __instance)
        {
            if (hScene == null)
                return;

            ChaControl character = __instance.GetComponentInParent<ChaControl>();

            if (character == null)
                return;

            int charIndex = 1;

            if (character.chaID == 0 || character.chaID == 1)
                charIndex = maleCount + character.chaID;
            else if (character.chaID == 99)
                charIndex = 0;

            SliderUI.ApplyLimbOffsets(charIndex);
        }

        //-- Start of HScene --//
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "SetStartAnimationInfo")]
        public static void HScene_SetStartAnimationInfo_Patch(HScene __instance, HSceneSprite ___sprite)
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
            Tools.hFlagCtrlTrav = Traverse.Create(hFlagCtrl);

            if (unlockCamera.Value)
            {
                cameraCtrl.isLimitDir = false;
                cameraCtrl.isLimitPos = false;
            }

            cameraShouldLock = true;
            
            characters = new List<ChaControl>();

            List<ChaControl> maleCharacters = new List<ChaControl>();
            maleCharacters.AddRange(__instance.GetMales());
            foreach (var chara in maleCharacters.Where(chara => chara != null))
                characters.Add(chara);

            maleCount = characters.Count;

            List<ChaControl> femaleCharacters = new List<ChaControl>();
            femaleCharacters.AddRange(__instance.GetFemales());
            foreach (var chara in femaleCharacters.Where(chara => chara != null))
                characters.Add(chara);

            if (characters == null)
                return;

            SliderUI.InitDraggersUI();
            
            Tools.SetGotoWeaknessCount(countToWeakness.Value);
        }
        
        //-- End of HScene --//
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "EndProc")]
        public static void HScene_EndProc_Patch()
        {
            activeUI = false;

            if (characters != null)
            {
                characters.Clear();
                characters = null;
            }

            hScene = null;
            hSprite = null;
            hFlagCtrl = null;
        }
        
        //-- Strip on start of H scene --//
        //-- fuck you illusion for giving me 21 headaches over this when it's supposed to work everywhere else I patched. Why the fuck is it working for females but not males in the same fucking line of code, why do I have to pick other places to patch. Fuck you, fuck you and FUCK YOU!! --//
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "SetStartVoice")]
        public static void HScene_SetStartVoice_StripClothes()
        {
            HScene_StripClothes(
                stripMaleClothes.Value == Tools.OffHStartAnimChange.OnHStart || stripMaleClothes.Value == Tools.OffHStartAnimChange.Both, 
                stripFemaleClothes.Value == Tools.OffHStartAnimChange.OnHStart || stripFemaleClothes.Value == Tools.OffHStartAnimChange.Both
            );
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
            if(alwaysGaugesHeart.Value == Tools.OffWeaknessAlways.Always || (alwaysGaugesHeart.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness))
                __result = true;
        }
        
        //-- Disable camera control when dragger ui open --//
        [HarmonyPrefix, HarmonyPatch(typeof(CameraControl_Ver2), "LateUpdate")]
        public static bool CameraControlVer2_LateUpdate_DisableCameraControl(CameraControl_Ver2 __instance)
        {
            if (!cameraShouldLock || !activeUI)
                return true;

            Traverse.Create(__instance).Property("isControlNow").SetValue(false);
            return false;
        }
        
        //-- Tears, close eyes, stop blinking --//
        [HarmonyPrefix, HarmonyPatch(typeof(HVoiceCtrl), "SetFace")]
        public static void HVoiceCtrl_SetFace_ForceTearsOnWeakness(ref HVoiceCtrl.FaceInfo _face)
        {
            if (_face == null)
                return;

            if(forceTears.Value == Tools.OffWeaknessAlways.Always || (forceTears.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness))
                _face.tear = 1f;

            if(forceCloseEyes.Value == Tools.OffWeaknessAlways.Always || (forceCloseEyes.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness))
                _face.openEye = 0.05f;
            
            if(forceStopBlinking.Value == Tools.OffWeaknessAlways.Always || (forceStopBlinking.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness))
                _face.blink = false;
        }

        //-- Prevent default animation change clothes strip --//
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "SetClothStateStartMotion")]
        public static bool HScene_SetClothStateStartMotion_PreventDefaultClothesStrip() => !preventDefaultAnimationChangeStrip.Value;

        //-- Strip clothes when changing animation --//
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "ChangeAnimVoiceFlag")]
        public static void HScene_ChangeAnimVoiceFlag_StripClothes() => HScene_StripClothes(stripMaleClothes.Value > Tools.OffHStartAnimChange.OnHStart, stripFemaleClothes.Value > Tools.OffHStartAnimChange.OnHStart);

        //-- Set apply offsets --//
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "ChangeAnimation")]
        private static void HScene_ChangeAnimation()
        {
            if (applySavedOffsets.Value)
                shouldApplyOffsets = true;
        }

        //-- Set apply offsets --//
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "SetMovePositionPoint")]
        private static void HScene_SetMovePositionPoint()
        {
            if (applySavedOffsets.Value)
                shouldApplyOffsets = true;
        }

        //-- Save current motion --//
        //-- Set apply offsets --//
        [HarmonyPostfix, HarmonyPatch(typeof(ChaControl), "setPlay")]
        private static void HScene_ChangeMotion(ChaControl __instance, string _strAnmName)
        {
            if (useOneOffsetForAllMotions.Value || __instance == null || _strAnmName.IsNullOrEmpty() || currentMotion == _strAnmName)
                return;

            currentMotion = _strAnmName;

            if (applySavedOffsets.Value)
                shouldApplyOffsets = true;
        }

        private static void HScene_StripClothes(bool stripMales, bool stripFemales)
        {
            if (stripMales)
            {
                var malesStrip = new List<Tools.ClothesStrip>
                {
                    stripMaleTop.Value, 
                    stripMaleBottom.Value, 
                    stripMaleBra.Value, 
                    stripMalePanties.Value, 
                    stripMaleGloves.Value, 
                    stripMalePantyhose.Value, 
                    stripMaleSocks.Value,  
                    stripMaleShoes.Value, 
                };
                
                foreach (var male in hScene.GetMales().Where(male => male != null))
                    foreach (var item in malesStrip.Select((x, i) => new { x, i }))
                        if (item.x > 0 && male.IsClothesStateKind(item.i) && male.fileStatus.clothesState[item.i] != 2)
                            male.SetClothesState(item.i, (byte)item.x);
            }

            if (stripFemales)
            {
                var femalesStrip = new List<Tools.ClothesStrip>
                {
                    stripFemaleTop.Value, 
                    stripFemaleBottom.Value, 
                    stripFemaleBra.Value, 
                    stripFemalePanties.Value, 
                    stripFemaleGloves.Value, 
                    stripFemalePantyhose.Value, 
                    stripFemaleSocks.Value,  
                    stripFemaleShoes.Value, 
                };
                
                foreach (var female in hScene.GetFemales().Where(female => female != null))
                    foreach (var item in femalesStrip.Select((x, i) => new { x, i }))
                        if (item.x > 0 && female.IsClothesStateKind(item.i) && female.fileStatus.clothesState[item.i] != 2)
                            female.SetClothesState(item.i, (byte)item.x);
            }
        }
        
        private static void SceneManager_sceneLoaded(Scene scene, LoadSceneMode lsm)
        {
            if (lsm != LoadSceneMode.Single) 
                return;

            if (scene.name == "HScene")
                harmony.PatchAll(typeof(HS2_BetterHScenes));
            else
                harmony.UnpatchAll(nameof(HS2_BetterHScenes));
        }
    }
}