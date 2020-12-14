using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using HarmonyLib;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

using AIProject;
using AIProject.Definitions;

using AIChara;
using Manager;

using UnityEngine;

using Map = Manager.Map;

namespace AI_BetterHScenes
{
    [BepInPlugin(nameof(AI_BetterHScenes), nameof(AI_BetterHScenes), VERSION)]
    [BepInProcess("AI-Syoujyo")]
    public class AI_BetterHScenes : BaseUnityPlugin
    {
        private enum ProcMode
        {
            Aibu,
            Houshi,
            Sonyu,
            Spnking,
            Masturbation,
            Peeping,
            Les,
            MultiPlay_F2M1
        }

        public const string VERSION = "2.5.6";

        public new static ManualLogSource Logger;

        private static readonly System.Random rand = new System.Random();

        public static HScene hScene;
        public static HSceneManager manager;
        public static HSceneFlagCtrl hFlagCtrl;
        private static HSceneSprite hSprite;
        private static VirtualCameraController hCamera;

        private static Traverse hSceneTrav;
        private static Traverse listTrav;
        private static Harmony harmony;

        public static List<ChaControl> characters;
        public static List<ChaControl> maleCharacters;
        public static List<ChaControl> femaleCharacters;
        public static List<ChaControl> shouldCleanUp;
        public static List<HMotionEyeNeckMale.EyeNeck> maleMotionList = new List<HMotionEyeNeckMale.EyeNeck>();

        private static GameObject map;
        private static Light sun;
        private static List<SkinnedCollisionHelper> collisionHelpers;

        private static bool OnHStart;
        private static bool activeDraggerUI;
        private static bool activeAnimationUI;
        private static bool patched;

        private static bool cameraShouldLock;
        private static bool oldMapState;
        private static LightShadows oldSunShadowsState;

        public static AnimationOffsets animationOffsets;
        private static bool shouldApplyOffsets;
        public static string currentMotion;

        public static int hProcMode = 0;
        public static bool bBaseReplacement = false;
        public static bool bIdleGlowException = false;
        public static bool bFootJobException = false;
        public static bool bTwoFootException = false;
        public static bool useReplacements = false;

        private static readonly List<string> siriReplaceList = new List<string>() { "ais_f_02", "ais_f_13", "ais_f_31", "ais_f_43", "ait_f_00", "ait_f_07" }; 
        private static readonly List<string> kosiReplaceList = new List<string>() { "ais_f_27", "ais_f_28", "ais_f_29", "ais_f_35", "ais_f_36", "ais_f_37", "ais_f_38"};
        private static readonly List<string> huggingReplaceList = new List<string>() { "h2s_f_12", "h2s_f_13" };
        private static readonly List<string> footReplaceList = new List<string>() { "aih_f_08", "aih_f_24", "aih_f_28" };
        private static readonly List<string> rightKokanReplaceList = new List<string>() { "aia_f_09", "aia_f_14", "aia_f_21" };
        private static readonly List<string> leftKokanReplaceList = new List<string>() { "aia_f_15", "aia_f_20" };
        private static readonly List<string> leftKosiReplaceList = new List<string>() { "aia_f_16" };

        //-- Draggers --//
        private static ConfigEntry<KeyboardShortcut> showDraggerUI { get; set; }
        private static ConfigEntry<KeyboardShortcut> showAnimationUI { get; set; }
        private static ConfigEntry<bool> applySavedOffsets { get; set; }
        public static ConfigEntry<bool> useOneOffsetForAllMotions { get; private set; }
        public static ConfigEntry<string> offsetFile { get; private set; }
        public static ConfigEntry<float> sliderMaxPosition { get; private set; }
        public static ConfigEntry<float> sliderMaxRotation { get; private set; }

        //-- Animations --//
        public static ConfigEntry<bool> enableAnimationFixer { get; private set; }
        public static ConfigEntry<bool> solveDependenciesFirst { get; private set; }
        public static ConfigEntry<bool> useLastSolutionForMales { get; private set; }
        public static ConfigEntry<bool> useLastSolutionForFemales { get; private set; }
        public static ConfigEntry<bool> fixAttachmentPoints { get; private set; }

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
        public static ConfigEntry<Tools.CleanCum> cleanCumAfterH { get; private set; }
        private static ConfigEntry<bool> increaseBathDesire { get; set; }

        //-- General --//
        private static ConfigEntry<Tools.OffWeaknessAlways> alwaysGaugesHeart { get; set; }
        public static ConfigEntry<bool> keepButtonsInteractive { get; private set; }
        private static ConfigEntry<int> hPointSearchRange { get; set; }
        private static ConfigEntry<bool> unlockCamera { get; set; }

        //-- Performance --//
        private static ConfigEntry<bool> disableMap { get; set; }
        private static ConfigEntry<bool> disableSunShadows { get; set; }
        private static ConfigEntry<bool> optimizeCollisionHelpers { get; set; }

        private void Awake()
        {
            Logger = base.Logger;

            shouldCleanUp = new List<ChaControl>();

            showDraggerUI = Config.Bind("Animations > Draggers", "Show draggers UI", new KeyboardShortcut(KeyCode.N));
            showAnimationUI = Config.Bind("Animations > Draggers", "Show animation UI", new KeyboardShortcut(KeyCode.N, KeyCode.LeftControl), new ConfigDescription("Displays a UI that can be used to change the current animation motion.  Intended to be used with draggers UI to make adjustments.  May break the flow of an HScene so use with caution."));
            (applySavedOffsets = Config.Bind("Animations > Draggers", "Apply saved offsets", true, new ConfigDescription("Apply previously saved character offsets for character pair / position during H"))).SettingChanged += delegate
            {
                if (applySavedOffsets.Value)
                    shouldApplyOffsets = true;
            };
            useOneOffsetForAllMotions = Config.Bind("Animations > Draggers", "Use one offset for all motions", true, new ConfigDescription("If disabled, the Save button in the UI will only save the offsets for the current motion of the position.  A Default button will be added to save it for all motions of that position that don't already have an offset."));
            offsetFile = Config.Bind("Animations > Draggers", "Offset File Path", "UserData/BetterHScenesOffsets.xml", new ConfigDescription("Path of the offset file card on disk."));
            sliderMaxPosition = Config.Bind("Animations > Draggers", "Slider min/max position", 2.5f, new ConfigDescription("Maximum limits of the position slider bars."));
            sliderMaxRotation = Config.Bind("Animations > Draggers", "Slider min/max rotation", 45f, new ConfigDescription("Maximum limits of the rotation slider bars."));

            (solveDependenciesFirst = Config.Bind("Animations > Solver", "Solve Independent Animations First", true, new ConfigDescription("Re-orders animation solving.  If the male animation is dependent on the female animation, the female animation will be solved first.  Some animations have both male and female dependencies.  These ones will run females first, so female dependencies will be broken.  This can be fixed by using last frame (see below)"))).SettingChanged += delegate
            {
                if (hScene != null)
                    SliderUI.UpdateDependentStatus();
            };
            useLastSolutionForFemales = Config.Bind("Animations > Solver", "Use Last Frame Solutions for Females", true, new ConfigDescription("Use Last Frame's result as input to next frame.  This can fix problems when the female animations are solved before the male animations but are dependent on the male animations.  It will add a framerate dependent amount of jitter to the animations, which can be a good thing if your fps is high, or a bad thing if your fps is low."));
            useLastSolutionForMales = Config.Bind("Animations > Solver", "Use Last Frame Solutions for Males", false, new ConfigDescription("Use Last Frame's result as input to next frame.  This can fix problems when the male animations are solved before the female animatiosn but are dependent on the male animations.  It will add a framerate dependent amount of jitter to the animations, which can be a good thing if your fps is high, or a bad thing if your fps is low."));
            enableAnimationFixer = Config.Bind("Animations > Solver", "Enable Animation Fixer", true, new ConfigDescription("Corrects most animations by using the other characters solutions if available. No other solver options will work without this enabled."));
            (fixAttachmentPoints = Config.Bind("Animations > Solver", "Fix broken Animation Tables", true, new ConfigDescription("Corrects certain animations by attaching certain IK points to the correct location instead of leaving them dangling in air."))).SettingChanged += delegate
            {
                if (hScene != null)
                    FixMotionList(hScene.ctrlFlag.nowAnimationInfo.fileFemale);
            };

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
            cleanCumAfterH = Config.Bind("QoL > Cum", "Clean cum on body after H", Tools.CleanCum.All, new ConfigDescription("Clean cum on body after H"));
            increaseBathDesire = Config.Bind("QoL > Cum", "Increase bath desire after H", false, new ConfigDescription("Increase bath desire after H (agents only)"));

            keepButtonsInteractive = Config.Bind("QoL > General", "Keep UI buttons interactive*", false, new ConfigDescription("Keep buttons interactive during certain events like orgasm (WARNING: May cause bugs)"));
            (hPointSearchRange = Config.Bind("QoL > General", "H point search range", 300, new ConfigDescription("Range in which H points are shown when changing location (default 60)", new AcceptableValueRange<int>(1, 999)))).SettingChanged += (s, e) =>
            {
                if (hSprite == null)
                    return;

                hSprite.HpointSearchRange = hPointSearchRange.Value;
            };

            (unlockCamera = Config.Bind("QoL > General", "Unlock camera movement", true, new ConfigDescription("Unlock camera zoom out / distance limit during H"))).SettingChanged += (s, e) =>
            {
                if (hCamera == null)
                    return;

                hCamera.isLimitDir = !unlockCamera.Value;
                hCamera.isLimitPos = !unlockCamera.Value;
            };

            (disableMap = Config.Bind("Performance Improvements", "Disable map", false, new ConfigDescription("Disable map during H scene"))).SettingChanged += (s, e) =>
            {
                if (map == null)
                    return;

                map.SetActive(!disableMap.Value);
            };

            (disableSunShadows = Config.Bind("Performance Improvements", "Disable sun shadows", false, new ConfigDescription("Disable sun shadows during H scene"))).SettingChanged += (s, e) =>
            {
                if (sun == null)
                    return;

                sun.shadows = disableSunShadows.Value ? LightShadows.None : LightShadows.Soft;
            };

            (optimizeCollisionHelpers = Config.Bind("Performance Improvements", "Optimize collisionhelpers", true, new ConfigDescription("Optimize collisionhelpers by letting them update once per frame"))).SettingChanged += (s, e) =>
            {
                if (collisionHelpers == null)
                    return;

                foreach (var helper in collisionHelpers.Where(helper => helper != null))
                {
                    if (!optimizeCollisionHelpers.Value)
                        helper.forceUpdate = true;

                    helper.updateOncePerFrame = optimizeCollisionHelpers.Value;
                }
            };

            shouldApplyOffsets = false;
            animationOffsets = new AnimationOffsets();
            HSceneOffset.LoadOffsetsFromFile();

            harmony = new Harmony(nameof(AI_BetterHScenes));
            harmony.PatchAll(typeof(Transpilers));
        }

        //-- Draw chara draggers UI --//
        private void OnGUI()
        {
            if (activeDraggerUI && hScene != null)
                SliderUI.DrawDraggersUI();
                
            if (activeAnimationUI && hScene != null)
                AnimationUI.DrawAnimationUI();
        }

        //-- Patch & unpatch cause illusion don't do scenemanager anymore --//
        //-- Apply chara offsets --//
        //-- Auto finish, togle chara draggers UI --//
        private void Update()
        {
            var isHScene = HSceneManager.isHScene;

            if (isHScene && !patched)
                HScene_sceneLoaded(true);
            else if (!isHScene && patched)
                HScene_sceneLoaded(false);

            if (hScene == null)
                return;

            if (showDraggerUI.Value.IsDown())
                activeDraggerUI = !activeDraggerUI;

            if (showAnimationUI.Value.IsDown())
                activeAnimationUI = !activeAnimationUI;

            if (shouldApplyOffsets && !hScene.NowChangeAnim)
            {
                HSceneOffset.ApplyCharacterOffsets();
                SliderUI.UpdateDependentStatus();
                FixMotionList(hScene.ctrlFlag.nowAnimationInfo.fileFemale);
                shouldApplyOffsets = false;
            }

            if (autoFinish.Value == Tools.AutoFinish.Off)
                return;

            if (hFlagCtrl.feel_m >= 0.98f && Tools.IsService() && autoFinish.Value != Tools.AutoFinish.InsertOnly)
            {
                var drink = hSprite.IsFinishVisible(4);
                var vomit = hSprite.IsFinishVisible(5);
                var onbody = hSprite.IsFinishVisible(0);

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
                var inside = hSprite.IsFinishVisible(2);
                var outside = hSprite.IsFinishVisible(0);
                var same = hSprite.IsFinishVisible(1);

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
        public static bool SolverManager_PreLateUpdate(RootMotion.SolverManager __instance)
        {
            if (hScene == null || __instance == null)
                return true;

            ChaControl character = __instance.GetComponentInParent<ChaControl>();

            if (character == null)
                return true;

            int characterIndex = 0;
            if (!character.isPlayer)
            {
                characterIndex = 1;
                if (femaleCharacters.Count > 1 && femaleCharacters[1] != null && character.loadNo == femaleCharacters[1].loadNo)
                    characterIndex = 2;
            }

            if (enableAnimationFixer.Value && solveDependenciesFirst.Value && character.isPlayer && SliderUI.characterOffsets[characterIndex].dependentAnimation)
                return false;

            if (!character.isPlayer)
            {
                bool leftFootJob = bFootJobException && (!bTwoFootException || currentMotion.Contains("Idle") || currentMotion.Contains("WLoop"));
                bool rightFootJob = bFootJobException && bTwoFootException && currentMotion.Contains("O");
                SliderUI.ApplyLimbOffsets(characterIndex, useLastSolutionForFemales.Value, useReplacements, leftFootJob, rightFootJob);
            }
            else
            {
                SliderUI.ApplyLimbOffsets(characterIndex, useLastSolutionForMales.Value, useReplacements, false, false);
            }

            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(RootMotion.SolverManager), "LateUpdate")]
        public static void SolverManager_PostLateUpdate(RootMotion.SolverManager __instance)
        {
            if (hScene == null || !enableAnimationFixer.Value || !solveDependenciesFirst.Value)
                return;

            ChaControl character = __instance.GetComponentInParent<ChaControl>();
            if (character == null || character.isPlayer || (femaleCharacters.Count > 1 && femaleCharacters[1] != null && femaleCharacters[1].loadNo == character.loadNo))
                return;

            for (var charIndex = 0; charIndex < maleCharacters.Count; charIndex++)
            {
                if (SliderUI.characterOffsets[charIndex].dependentAnimation)
                {
                    SliderUI.ApplyLimbOffsets(charIndex, useLastSolutionForMales.Value, useReplacements, false, false);
                    maleCharacters[charIndex].fullBodyIK.UpdateSolverExternal();
                }
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(H_Lookat_dan), "LateUpdate")]
        public static void H_Lookat_dan_PostLateUpdate()
        {
            if (hScene != null)
                SliderUI.SaveBasePoints(useReplacements);
        }

        //-- Start of HScene --//
        //-- Remove hcamera movement limit --//
        //-- Change H point search range --//
        //-- Strip clothes when starting H --//
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "SetStartVoice")]
        public static void HScene_SetStartVoice_Patch(HScene __instance, HSceneSprite ___sprite, HSceneManager ___hSceneManager)
        {
            hScene = __instance;
            hSprite = ___sprite;
            manager = ___hSceneManager;

            if (hScene == null || hSprite == null || manager == null)
                return;

            hFlagCtrl = hScene.ctrlFlag;
            if (hFlagCtrl == null)
                return;

            hCamera = hFlagCtrl.cameraCtrl;
            if (hCamera == null)
                return;

            hSceneTrav = Traverse.Create(hScene);

            listTrav = Traverse.Create(hScene.ctrlEyeNeckMale[0]);
            maleMotionList = listTrav?.Field("lstEyeNeck").GetValue<List<HMotionEyeNeckMale.EyeNeck>>();

            map = GameObject.Find("map00_Beach");
            if (map == null)
                map = GameObject.Find("map_01_data");

            if (map == null)
                return;

            var sunObj = GameObject.Find("CommonSpace/MapRoot/MapSimulation(Clone)/EnviroSkyGroup(Clone)/Enviro Directional Light");
            if (sunObj == null)
                return;

            cameraShouldLock = true;
            sun = sunObj.GetComponent<Light>();

            collisionHelpers = new List<SkinnedCollisionHelper>();
            characters = new List<ChaControl>();
            maleCharacters = new List<ChaControl>();
            ChaControl[] males = __instance.GetMales();
            foreach (var male in males.Where(male => male != null))
            {
                maleCharacters.Add(male);
                characters.Add(male);
            }

            femaleCharacters = new List<ChaControl>();
            ChaControl[] females = __instance.GetFemales();
            foreach (var female in females.Where(female => female != null))
            {
                femaleCharacters.Add(female);
                characters.Add(female);
            }

            if (characters == null)
                return;

            oldMapState = map.activeSelf;
            oldSunShadowsState = sun.shadows;

            if (disableMap.Value)
                map.SetActive(false);

            if (disableSunShadows.Value)
                sun.shadows = LightShadows.None;

            if (unlockCamera.Value)
            {
                hCamera.isLimitDir = false;
                hCamera.isLimitPos = false;
            }

            if (hPointSearchRange.Value != 60 && hSprite != null)
                hSprite.HpointSearchRange = hPointSearchRange.Value;

            Tools.hFlagCtrlTrav = Traverse.Create(hFlagCtrl);

            Tools.SetGotoWeaknessCount(countToWeakness.Value);

            SliderUI.InitDraggersUI();
        }

        //-- Enable map, simulation after H if disabled previously, disable dragger UI --//
        //-- Set bath desire after h --//
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "EndProc")]
        public static void HScene_EndProc_Patch()
        {
            if (map != null)
                map.SetActive(oldMapState);

            if (sun != null)
                sun.shadows = oldSunShadowsState;

            activeDraggerUI = false;
            activeAnimationUI = false;

            OnHStart = false;

            if (!increaseBathDesire.Value || manager.bMerchant)
                return;

            // Prone to errors from previous versions, trying to be safe
            try
            {
                var agentTable = Singleton<Map>.Instance.AgentTable;
                if (agentTable == null)
                    return;

                foreach (var female in hScene.GetFemales().Where(female => female != null))
                {
                    var agent = agentTable.FirstOrDefault(pair => pair.Value != null && pair.Value.ChaControl != null && pair.Value.ChaControl == female).Value;
                    if (agent == null)
                        continue;

                    var bathDesireType = Desire.GetDesireKey(Desire.Type.Bath);
                    var lewdDesireType = Desire.GetDesireKey(Desire.Type.H);

                    var clampedReason = Tools.Remap(agent.GetFlavorSkill(FlavorSkill.Type.Reason), 0, 99999f, 0, 100f);
                    var clampedDirty = Tools.Remap(agent.GetFlavorSkill(FlavorSkill.Type.Dirty), 0, 99999f, 0, 100f);
                    var clampedLewd = agent.GetDesire(lewdDesireType) ?? 0;
                    var newBathDesire = 100f + (clampedReason * 1.25f) - clampedDirty - clampedLewd * 1.5f;

                    agent.SetDesire(bathDesireType, Mathf.Clamp(newBathDesire, 0f, 100f));
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage("HScene_EndProc_Patch error!");
                Logger.LogWarning("HScene_EndProc_Patch error!");

                Console.WriteLine(ex);
            }
        }

        //-- Strip on start of H scene --//
        //-- fuck you illusion for giving me 21 headaches over this when it's supposed to work everywhere else I patched. Why the fuck is it working for females but not males in the same fucking line of code, why do I have to pick other places to patch. Fuck you, fuck you and FUCK YOU!! --//
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "SyncAnimation")]
        public static void HScene_SyncAnimation_StripClothes()
        {
            if (!OnHStart)
            {
                HScene_StripClothes(
                    stripMaleClothes.Value == Tools.OffHStartAnimChange.OnHStart || stripMaleClothes.Value == Tools.OffHStartAnimChange.Both,
                    stripFemaleClothes.Value == Tools.OffHStartAnimChange.OnHStart || stripFemaleClothes.Value == Tools.OffHStartAnimChange.Both
                );
                OnHStart = true;
            }
        }

        //-- Always gauges heart --//
        [HarmonyPostfix, HarmonyPatch(typeof(FeelHit), "isHit")]
        public static void FeelHit_isHit_AlwaysGaugesHeart(ref bool __result)
        {
            if (alwaysGaugesHeart.Value == Tools.OffWeaknessAlways.Always || (alwaysGaugesHeart.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness))
                __result = true;
        }

        //-- Disable camera control when dragger ui open --//
        [HarmonyPrefix, HarmonyPatch(typeof(VirtualCameraController), "LateUpdate")]
        public static bool VirtualCameraController_LateUpdate_DisableCameraControl(VirtualCameraController __instance)
        {
            if (!cameraShouldLock || !activeDraggerUI)
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

            if (forceTears.Value == Tools.OffWeaknessAlways.Always || (forceTears.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness))
                _face.tear = 1f;

            if (forceCloseEyes.Value == Tools.OffWeaknessAlways.Always || (forceCloseEyes.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness))
                _face.openEye = 0.05f;

            if (forceStopBlinking.Value == Tools.OffWeaknessAlways.Always || (forceStopBlinking.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness))
                _face.blink = false;
        }

        //-- Fix for the massive FPS drop during HScene insert/service positions --//
        [HarmonyPostfix, HarmonyPatch(typeof(SkinnedCollisionHelper), "Init")]
        public static void SkinnedCollisionHelper_Init_UpdateOncePerFrame(SkinnedCollisionHelper __instance)
        {
            if (collisionHelpers == null)
                return;

            collisionHelpers.Add(__instance);

            if (optimizeCollisionHelpers.Value)
                __instance.updateOncePerFrame = true;
        }

        //-- Add character to the shouldCleanUp list --//
        [HarmonyPostfix, HarmonyPatch(typeof(SiruPasteCtrl), "Proc")]
        public static void SiruPasteCtrl_Proc_PopulateList(ChaControl ___chaFemale)
        {
            if (cleanCumAfterH.Value < Tools.CleanCum.AgentsOnly)
                return;

            var chara = ___chaFemale;
            if (chara == null || chara.isPlayer || shouldCleanUp.Contains(chara) || manager.bMerchant)
                return;

            var agent = Singleton<Map>.Instance.AgentTable.Values.FirstOrDefault(actor => actor != null && actor.ChaControl != null && actor.ChaControl == chara);
            if (agent == null)
                return;

            for (var i = 0; i < 5; i++)
            {
                if (chara.GetSiruFlag((ChaFileDefine.SiruParts)i) == 0)
                    continue;

                shouldCleanUp.Add(chara);
                break;
            }
        }

        //-- Cache current animation mode --//
        //-- Strip clothes when changing animation --//
        //-- Prevent default animation change clothes strip --//
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "SetClothStateStartMotion")]
        public static bool HScene_SetClothStateStartMotion_PreventDefaultClothesStrip(int ___mode, int ___modeCtrl)
        {
            Tools.mode = ___mode;
            Tools.modeCtrl = ___modeCtrl;

            HScene_StripClothes(stripMaleClothes.Value > Tools.OffHStartAnimChange.OnHStart, stripFemaleClothes.Value > Tools.OffHStartAnimChange.OnHStart);

            return !preventDefaultAnimationChangeStrip.Value;
        }

        //-- Clean up chara after bath if retaining cum effect --//
        [HarmonyPostfix, HarmonyPatch(typeof(Bath), "OnCompletedStateTask")]
        public static void Bath_OnCompletedStateTask_CleanUpCum(Bath __instance) => Tools.CleanUpSiru(__instance);

        //-- Clean up chara after changing if retaining cum effect --//
        [HarmonyPostfix, HarmonyPatch(typeof(ClothChange), "OnCompletedStateTask")]
        public static void ClothChange_OnCompletedStateTask_CleanUpCum(ClothChange __instance) => Tools.CleanUpSiru(__instance);

        //-- Set apply offsets --//
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "ChangeAnimation")]
        private static void HScene_PreChangeAnimation()
        {
            if (applySavedOffsets.Value)
                shouldApplyOffsets = true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "ChangeAnimation")]
        private static void HScene_PostChangeAnimation()
        {
            if (hScene == null)
                return;

            maleMotionList = listTrav?.Field("lstEyeNeck").GetValue<List<HMotionEyeNeckMale.EyeNeck>>();
            hProcMode = hSceneTrav.Field("mode").GetValue<int>();
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
        private static void ChaControl_PostSetPlay(ChaControl __instance, string _strAnmName)
        {
            if (__instance == null || _strAnmName.IsNullOrEmpty() || currentMotion == _strAnmName)
                return;

            currentMotion = _strAnmName;
            useReplacements = bBaseReplacement && !bFootJobException && (!bIdleGlowException || (!currentMotion.Contains("Idle") && !currentMotion.Contains("_A")));

            if (useOneOffsetForAllMotions.Value)
                return;

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

        public static void SwitchAnimations(string playAnimation)
        {
            HItemCtrl ctrlItem = hSceneTrav?.Field("ctrlItem").GetValue<HItemCtrl>();

            if (femaleCharacters == null || femaleCharacters[0] == null)
                return;

            femaleCharacters[0].setPlay(playAnimation, 0);
            MotionIK motionIK = femaleCharacters[0].GetComponent<MotionIK>();
            if (motionIK != null)
                motionIK.Calc(playAnimation);

            if (hProcMode == (int)ProcMode.MultiPlay_F2M1 || hProcMode == (int)ProcMode.Les)
            {
                if (femaleCharacters[1].visibleAll && femaleCharacters[1].objTop != null)
                {
                    femaleCharacters[1].setPlay(playAnimation, 0);
                    motionIK = femaleCharacters[1].GetComponent<MotionIK>();
                    if (motionIK != null)
                        motionIK.Calc(playAnimation);
                }

            }

            if (hProcMode != (int)ProcMode.Masturbation && hProcMode != (int)ProcMode.Les)
            {
                if (maleCharacters[0].objTop != null)
                {
                    maleCharacters[0].setPlay(playAnimation, 0);
                    motionIK = maleCharacters[0].GetComponent<MotionIK>();
                    if (motionIK != null)
                        motionIK.Calc(playAnimation);
                }

                if (ctrlItem != null)
                    ctrlItem.setPlay(playAnimation);
            }
        }

        public static void FixMotionList(string fileFemale)
        {
            SliderUI.ClearBaseReplacements();
            bBaseReplacement = false;
            bIdleGlowException = false;
            bFootJobException = false;
            bTwoFootException = false;

            if (!fixAttachmentPoints.Value || maleCharacters == null || maleCharacters[0] == null || femaleCharacters == null || femaleCharacters[0] == null)
                return;

            if (siriReplaceList.Contains(fileFemale))
            {
                Transform leftContact = femaleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("k_f_siriL_00")).FirstOrDefault();
                Transform rightContact = femaleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("k_f_siriR_00")).FirstOrDefault();

                if (leftContact != null)
                    SliderUI.SetBaseReplacement(0, (int)BodyPart.LeftHand, leftContact);

                if (rightContact != null)
                    SliderUI.SetBaseReplacement(0, (int)BodyPart.RightHand, rightContact);

                bBaseReplacement = true;
            }
            else if (kosiReplaceList.Contains(fileFemale))
            {
                Transform leftContact = femaleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("k_f_kosi02_00")).FirstOrDefault();
                Transform rightContact = femaleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("k_f_kosi02_01")).FirstOrDefault();

                if (leftContact != null)
                    SliderUI.SetBaseReplacement(0, (int)BodyPart.LeftHand, leftContact);

                if (rightContact != null)
                    SliderUI.SetBaseReplacement(0, (int)BodyPart.RightHand, rightContact);

                bBaseReplacement = true;
            }
            else if (huggingReplaceList.Contains(fileFemale))
            {
                Transform leftContact = maleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("k_f_spine03_00")).FirstOrDefault();
                Transform rightContact = femaleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("k_f_armlowL_00")).FirstOrDefault();

                if (leftContact != null)
                    SliderUI.SetBaseReplacement(maleCharacters.Count, (int)BodyPart.LeftHand, leftContact);

                if (rightContact != null)
                    SliderUI.SetBaseReplacement(maleCharacters.Count, (int)BodyPart.RightHand, rightContact);

                bBaseReplacement = true;
            }
            else if (footReplaceList.Contains(fileFemale))
            {
                Transform leftAnkleReference = femaleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("f_k_foot_L")).FirstOrDefault();
                Transform leftDanReference = maleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("k_m_dansao00_00")).FirstOrDefault();

                if (leftAnkleReference != null)
                    SliderUI.SetBaseReplacement(maleCharacters.Count, (int)BodyPart.LeftFoot, leftAnkleReference);
                if (leftDanReference != null)
                    SliderUI.SetBaseReplacement(maleCharacters.Count, (int)BodyPart.LeftHand, leftDanReference);

                bFootJobException = true;

                if (fileFemale != footReplaceList[0])
                {
                    Transform rightAnkleReference = femaleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("f_k_foot_R")).FirstOrDefault();
                    Transform rightDanReference = maleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("k_m_dansao00_01")).FirstOrDefault();


                    if (rightAnkleReference != null)
                        SliderUI.SetBaseReplacement(maleCharacters.Count, (int)BodyPart.RightFoot, rightAnkleReference);
                    if (rightDanReference != null)
                        SliderUI.SetBaseReplacement(maleCharacters.Count, (int)BodyPart.RightHand, rightDanReference);

                    bTwoFootException = true;
                }
            }
            else if (rightKokanReplaceList.Contains(fileFemale))
            {
                Transform rightContact = femaleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("k_f_kokan_00")).FirstOrDefault();
                if (rightContact != null)
                    SliderUI.SetBaseReplacement(0, (int)BodyPart.RightHand, rightContact);

                bBaseReplacement = true;
                bIdleGlowException = true;
            }
            else if (leftKokanReplaceList.Contains(fileFemale))
            {
                Transform leftContact = femaleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("k_f_kokan_00")).FirstOrDefault();
                if (leftContact != null)
                    SliderUI.SetBaseReplacement(0, (int)BodyPart.LeftHand, leftContact);

                bBaseReplacement = true;
                bIdleGlowException = true;
            }
            else if (leftKosiReplaceList.Contains(fileFemale))
            {
                Transform leftContact = femaleCharacters[0].GetComponentsInChildren<Transform>().Where(x => x.name.Contains("k_f_kosi02_00")).FirstOrDefault();
                if (leftContact != null)
                    SliderUI.SetBaseReplacement(0, (int)BodyPart.LeftHand, leftContact);

                bBaseReplacement = true;
                bIdleGlowException = true;
            }

            useReplacements = bBaseReplacement && !bFootJobException && (!bIdleGlowException || (!currentMotion.Contains("Idle") && !currentMotion.Contains("_A")));
        }

        private static void HScene_sceneLoaded(bool loaded)
        {
            patched = loaded;

            if (loaded)
                harmony.PatchAll(typeof(AI_BetterHScenes));
            else
                harmony.UnpatchAll(nameof(AI_BetterHScenes));
        }
    }
}