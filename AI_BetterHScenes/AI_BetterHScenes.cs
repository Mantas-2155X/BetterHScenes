using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;

using HarmonyLib;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Harmony;
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
        public const string VERSION = "2.3.2";

        public new static ManualLogSource Logger;

        private static bool inHScene;

        private static HScene hScene;
        private static HScene.AnimationListInfo currentAnimation;
        private static string currentMotion;
        public static HSceneManager manager;
        public static HSceneFlagCtrl hFlagCtrl;
        private static HSceneSprite hSprite;

        private static VirtualCameraController hCamera;

        public static List<ChaControl> characters;
        public static List<ChaControl> shouldCleanUp;
        private static List<GameObject> finishObjects;

        public static AnimationOffsets animationOffsets;

        private static GameObject map;
        private static Light sun;
        private static List<SkinnedCollisionHelper> collisionHelpers;

        private static bool activeUI;

        public static bool cameraShouldLock;                   // compatibility with other plugins which might disable the camera control
        public static bool oldMapState;                        // compatibility with other plugins which might disable the map
        public static LightShadows oldSunShadowsState;         // compatibility with other plugins which might disable the map simulation

        private static bool shouldApplyOffsets; // compatibility with other plugins which might disable the map simulation

        //-- Draggers --//
        private static ConfigEntry<KeyboardShortcut> showDraggerUI { get; set; }
        private static ConfigEntry<bool> applySavedOffsets { get; set; }
        private static ConfigEntry<bool> useOneOffsetForAllMotions { get; set; }

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

            showDraggerUI = Config.Bind("QoL > Draggers", "Show draggers UI", new KeyboardShortcut(KeyCode.M));
            applySavedOffsets = Config.Bind("QoL > Draggers", "Apply saved offsets", true, new ConfigDescription("Apply previously saved character offsets for character pair / position during H"));
            useOneOffsetForAllMotions = Config.Bind("QoL > Draggers", "Use one offset for all motions", true, new ConfigDescription("If disabled, the Save button in the UI will only save the offsets for the current motion of the position.  A Default button will be added to save it for all motions of that position that don't already have an offset."));

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
            cleanCumAfterH = Config.Bind("QoL > Cum", "Clean cum on body after H", Tools.CleanCum.All, new ConfigDescription("Clean cum on body after H"));
            increaseBathDesire = Config.Bind("QoL > Cum", "Increase bath desire after H", false, new ConfigDescription("Increase bath desire after H (agents only)"));

            alwaysGaugesHeart = Config.Bind("QoL > General", "Always hit gauge heart", Tools.OffWeaknessAlways.WeaknessOnly, new ConfigDescription("Always hit gauge heart. Will cause progress to increase without having to scroll specific amount"));
            keepButtonsInteractive = Config.Bind("QoL > General", "Keep UI buttons interactive*", false, new ConfigDescription("Keep buttons interactive during certain events like orgasm (WARNING: May cause bugs)"));
            hPointSearchRange = Config.Bind("QoL > General", "H point search range", 300, new ConfigDescription("Range in which H points are shown when changing location (default 60)", new AcceptableValueRange<int>(1, 999)));
            unlockCamera = Config.Bind("QoL > General", "Unlock camera movement", true, new ConfigDescription("Unlock camera zoom out / distance limit during H"));

            disableMap = Config.Bind("Performance Improvements", "Disable map", false, new ConfigDescription("Disable map during H scene"));
            disableSunShadows = Config.Bind("Performance Improvements", "Disable sun shadows", false, new ConfigDescription("Disable sun shadows during H scene"));
            optimizeCollisionHelpers = Config.Bind("Performance Improvements", "Optimize collisionhelpers", true, new ConfigDescription("Optimize collisionhelpers by letting them update once per frame"));

            countToWeakness.SettingChanged += delegate
            {
                if (!inHScene || hFlagCtrl == null)
                    return;

                Traverse.Create(hFlagCtrl).Field("gotoFaintnessCount").SetValue(countToWeakness.Value);
            };

            hPointSearchRange.SettingChanged += delegate
            {
                if (!inHScene || hSprite == null)
                    return;

                hSprite.HpointSearchRange = hPointSearchRange.Value;
            };

            unlockCamera.SettingChanged += delegate
            {
                if (!inHScene || hCamera == null)
                    return;

                hCamera.isLimitDir = !unlockCamera.Value;
                hCamera.isLimitPos = !unlockCamera.Value;
            };

            disableMap.SettingChanged += delegate
            {
                if (!inHScene || map == null)
                    return;

                map.SetActive(!disableMap.Value);
            };

            disableSunShadows.SettingChanged += delegate
            {
                if (!inHScene || sun == null)
                    return;

                sun.shadows = disableSunShadows.Value ? LightShadows.None : LightShadows.Soft;
            };

            optimizeCollisionHelpers.SettingChanged += delegate
            {
                if (!inHScene || collisionHelpers == null)
                    return;

                foreach (var helper in collisionHelpers.Where(helper => helper != null))
                {
                    if (!optimizeCollisionHelpers.Value)
                        helper.forceUpdate = true;

                    helper.updateOncePerFrame = optimizeCollisionHelpers.Value;
                }
            };

            applySavedOffsets.SettingChanged += delegate
            {
                if (!inHScene || hCamera == null)
                    return;

                if (applySavedOffsets.Value == true)
                {
                    shouldApplyOffsets = true;
                }
            };

            shouldApplyOffsets = false;
            animationOffsets = new AnimationOffsets();
            LoadOffsetsFromFile();

            HarmonyWrapper.PatchAll(typeof(Transpilers));
            HarmonyWrapper.PatchAll(typeof(AI_BetterHScenes));
        }

        //-- Draw chara draggers UI --//
        private void OnGUI()
        {
            if (inHScene && activeUI)
                UI.DrawDraggersUI();
        }

        //-- Auto finish, togle chara draggers UI --//
        private void Update()
        {
            if (!inHScene)
                return;

            if (showDraggerUI.Value.IsDown())
                activeUI = !activeUI;

            if (shouldApplyOffsets && !hScene.NowChangeAnim)
            {
                HScene_ApplyCharacterOffsets();
                shouldApplyOffsets = false;
            }
            if (autoFinish.Value == Tools.AutoFinish.Off || hFlagCtrl == null || finishObjects == null || finishObjects.Count == 0)
                return;

            var mode = Traverse.Create(hScene).Field("mode").GetValue<int>();
            switch (mode)
            {
                case 1 when hFlagCtrl.feel_m >= 0.98f && (autoFinish.Value == Tools.AutoFinish.ServiceOnly || autoFinish.Value == Tools.AutoFinish.Both): // Houshi
                    {
                        var drink = finishObjects[0].activeSelf;
                        var vomit = finishObjects[1].activeSelf;
                        var onbody = finishObjects[2].activeSelf;

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

                        break;
                    }
                case 2 when hFlagCtrl.feel_f >= 0.98f && hFlagCtrl.feel_m >= 0.98f && (autoFinish.Value == Tools.AutoFinish.InsertOnly || autoFinish.Value == Tools.AutoFinish.Both): // Sonyu
                    {
                        var inside = finishObjects[3].activeSelf;
                        var outside = finishObjects[2].activeSelf;
                        var same = finishObjects[4].activeSelf;

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

                        break;
                    }
            }
        }

        //-- Disable map, simulation to improve performance --//
        //-- Remove hcamera movement limit --//
        //-- Change H point search range --//
        //-- Strip clothes when starting H --//
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "SetStartVoice")]
        public static void HScene_SetStartVoice_Patch(HScene __instance)
        {
            inHScene = true;

            SetupVariables(__instance);

            if (map != null)
            {
                oldMapState = map.activeSelf;

                if (disableMap.Value)
                    map.SetActive(false);
            }

            if (sun != null)
            {
                oldSunShadowsState = sun.shadows;

                if (disableSunShadows.Value)
                    sun.shadows = LightShadows.None;
            }

            if (hCamera != null && unlockCamera.Value)
            {
                hCamera.isLimitDir = false;
                hCamera.isLimitPos = false;
            }

            if (hPointSearchRange.Value != 60 && hSprite != null)
                hSprite.HpointSearchRange = hPointSearchRange.Value;

            HScene_StripClothes(stripMaleClothes.Value == Tools.OffHStartAnimChange.OnHStart || stripFemaleClothes.Value == Tools.OffHStartAnimChange.OnHStart);
        }

        //-- Enable map, simulation after H if disabled previously, disable dragger UI --//
        //-- Set bath desire after h --//
        [HarmonyPostfix, HarmonyPatch(typeof(HScene), "EndProc")]
        public static void HScene_EndProc_Patch()
        {
            inHScene = false;

            if (map != null)
                map.SetActive(oldMapState);

            if (sun != null)
                sun.shadows = oldSunShadowsState;

            activeUI = false;

            if (!increaseBathDesire.Value || manager != null && manager.bMerchant)
                return;

            var females = hScene.GetFemales();
            var agentTable = Singleton<Map>.Instance.AgentTable;

            if (females == null || agentTable == null)
                return;

            foreach (var female in females.Where(female => female != null))
            {
                var agent = agentTable.FirstOrDefault(pair => pair.Value != null && pair.Value.ChaControl == female).Value;
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

        //-- Prevent default animation change clothes strip --//
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "SetClothStateStartMotion")]
        public static bool HScene_SetClothStateStartMotion_PreventDefaultClothesStrip()
        {
            return !inHScene || !preventDefaultAnimationChangeStrip.Value;
        }

        //-- Always gauges heart --//
        [HarmonyPostfix, HarmonyPatch(typeof(FeelHit), "isHit")]
        public static void FeelHit_isHit_AlwaysGaugesHeart(ref bool __result)
        {
            if (inHScene && alwaysGaugesHeart.Value == Tools.OffWeaknessAlways.Always || alwaysGaugesHeart.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl != null && hFlagCtrl.isFaintness)
                __result = true;
        }

        //-- Disable camera control when dragger ui open --//
        [HarmonyPrefix, HarmonyPatch(typeof(VirtualCameraController), "LateUpdate")]
        public static bool VirtualCameraController_LateUpdate_DisableCameraControl(VirtualCameraController __instance)
        {
            if (!inHScene || !cameraShouldLock || !activeUI || __instance == null)
                return true;

            Traverse.Create(__instance).Property("isControlNow").SetValue(false);
            return false;
        }

        //-- Tears, close eyes, stop blinking --//
        [HarmonyPrefix, HarmonyPatch(typeof(HVoiceCtrl), "SetFace")]
        public static void HVoiceCtrl_SetFace_ForceTearsOnWeakness(ref HVoiceCtrl.FaceInfo _face)
        {
            if (!inHScene || _face == null)
                return;

            if (forceTears.Value == Tools.OffWeaknessAlways.Always || forceTears.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness)
                _face.tear = 1f;

            if (forceCloseEyes.Value == Tools.OffWeaknessAlways.Always || forceCloseEyes.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness)
                _face.openEye = 0.05f;

            if (forceStopBlinking.Value == Tools.OffWeaknessAlways.Always || forceStopBlinking.Value == Tools.OffWeaknessAlways.WeaknessOnly && hFlagCtrl.isFaintness)
                _face.blink = false;
        }

        //-- Fix for the massive FPS drop during HScene insert/service positions --//
        [HarmonyPostfix, HarmonyPatch(typeof(SkinnedCollisionHelper), "Init")]
        public static void SkinnedCollisionHelper_Init_UpdateOncePerFrame(SkinnedCollisionHelper __instance)
        {
            if (!inHScene || __instance == null || collisionHelpers == null)
                return;

            collisionHelpers.Add(__instance);

            if (optimizeCollisionHelpers.Value)
                __instance.updateOncePerFrame = true;
        }

        //-- Add character to the shouldCleanUp list --//
        [HarmonyPostfix, HarmonyPatch(typeof(SiruPasteCtrl), "Proc")]
        public static void SiruPasteCtrl_Proc_PopulateList(ChaControl ___chaFemale)
        {
            if (!inHScene || cleanCumAfterH.Value == Tools.CleanCum.Off || cleanCumAfterH.Value == Tools.CleanCum.MerchantOnly || shouldCleanUp == null)
                return;

            var chara = ___chaFemale;
            if (chara == null || chara.isPlayer || shouldCleanUp.Contains(chara) || (manager != null && manager.bMerchant))
                return;

            var agent = Singleton<Map>.Instance.AgentTable.Values.FirstOrDefault(actor => actor != null && actor.ChaControl == chara);
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

        //-- Clean up chara after bath if retaining cum effect --//
        [HarmonyPostfix, HarmonyPatch(typeof(Bath), "OnCompletedStateTask")]
        public static void Bath_OnCompletedStateTask_CleanUpCum(Bath __instance) => Tools.CleanUpSiru(__instance);

        //-- Clean up chara after changing if retaining cum effect --//
        [HarmonyPostfix, HarmonyPatch(typeof(ClothChange), "OnCompletedStateTask")]
        public static void ClothChange_OnCompletedStateTask_CleanUpCum(ClothChange __instance) => Tools.CleanUpSiru(__instance);

        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "SetMovePositionPoint")]
        private static void HScene_SetMovePositionPoint()
        {
            if (applySavedOffsets.Value == true)
                shouldApplyOffsets = true;
        }

        //-- Strip clothes when changing animation --//
        //-- Save current animation --//
        [HarmonyPrefix, HarmonyPatch(typeof(HScene), "ChangeAnimation")]
        private static void HScene_ChangeAnimation(HScene.AnimationListInfo _info, bool _isForceResetCamera, bool _isForceLoopAction = false, bool _UseFade = true)
        {
            currentAnimation = _info;
            HScene_StripClothes(stripMaleClothes.Value == Tools.OffHStartAnimChange.OnHStartAndAnimChange || stripFemaleClothes.Value == Tools.OffHStartAnimChange.OnHStartAndAnimChange);
        }

        //-- Save current motion --//
        //-- Apply the current offsets --//
        [HarmonyPostfix, HarmonyPatch(typeof(H_Lookat_dan), "setInfo")]
        private static void HScene_ChangeMotion(H_Lookat_dan __instance)
        {
            if (__instance.strPlayMotion == null)
                return;

            currentMotion = __instance.strPlayMotion;

            if (applySavedOffsets.Value == true)
                HScene_ApplyCharacterOffsets();
        }
        private static void HScene_StripClothes(bool shouldStrip)
        {
            if (!inHScene || !shouldStrip || hScene == null)
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

        //-- Apply character offsets for current animation, if they can be found --//
        private static void HScene_ApplyCharacterOffsets()
        {
            if (currentAnimation == null || currentAnimation.nameAnimation == null || currentMotion == null)
            {
                AI_BetterHScenes.Logger.LogMessage("null Animation");
            }
            else
            {
                string characterPairName = null;
                foreach (var character in AI_BetterHScenes.characters.Where(character => character != null))
                {
                    if (characterPairName == null)
                        characterPairName = character.fileParam.fullname;
                    else
                        characterPairName += "_" + character.fileParam.fullname;
                }

                AnimationsList animationList = animationOffsets.Animations.Find(x => x.AnimationName == currentAnimation.nameAnimation);
                if (animationList != null && characterPairName != null)
                {
                    MotionList motionList;
                    if(UseOneOffsetForAllMotions())
                    {
                        motionList = animationList.MotionList.Find(x => x.MotionName == "default");
                    }
                    else
                    { 
                        motionList = animationList.MotionList.Find(x => x.MotionName == currentMotion);
                        if (motionList == null)
                            motionList = animationList.MotionList.Find(x => x.MotionName == "default");
                    }

                    if (motionList != null)
                    {
                        CharacterPairList characterPair = motionList.CharacterPairList.Find(x => x.CharacterPairName == characterPairName);

                        if (characterPair != null)
                        {
                            foreach (var character in AI_BetterHScenes.characters.Where(character => character != null))
                            {
                                CharacterOffsets characterOffsets = characterPair.CharacterOffsets.Find(x => x.CharacterName == character.fileParam.fullname);

                                if (characterOffsets != null)
                                {
                                    Vector3 positionOffset = new Vector3(characterOffsets.PositionOffsetX, characterOffsets.PositionOffsetY, characterOffsets.PositionOffsetZ);
                                    Vector3 rotationOffset = new Vector3(characterOffsets.RotationOffsetP, characterOffsets.RotationOffsetY, characterOffsets.RotationOffsetR);
                                    character.SetPosition(positionOffset);
                                    character.SetRotation(rotationOffset);
                                }
                            }
                        }
                    }
                }
            }
        }

        //-- Save the character pair of offsets to the xml file, overwriting if necessary --//
        public static void SaveCharacterPairPosition(CharacterPairList characterPair, bool isDefault = false)
        {

            if (currentAnimation.nameAnimation == null || characterPair == null || currentMotion == null)
                return;

            string animationName = currentAnimation.nameAnimation;
            string motion = currentMotion;
            if (isDefault)
                motion = "default";

            AI_BetterHScenes.Logger.LogMessage("Saving Offsets for " + currentAnimation.nameAnimation + " Motion " + motion + " for characters " + characterPair);

            AnimationsList animation = animationOffsets.Animations.Find(x => x.AnimationName == animationName);

            if (animation != null)
            {
                animationOffsets.Animations.Remove(animation);

                MotionList motionList = animation.MotionList.Find(x => x.MotionName == motion);
                if (motionList != null)
                {
                    animation.MotionList.Remove(motionList);
                    CharacterPairList existingCharacterPair = motionList.CharacterPairList.Find(x => x.CharacterPairName == characterPair.CharacterPairName);
                    if (existingCharacterPair != null)
                        motionList.CharacterPairList.Remove(existingCharacterPair);
                    motionList.CharacterPairList.Add(characterPair);
                    animation.MotionList.Add(motionList);
                }
                else
                {
                    motionList = new MotionList(motion);
                    motionList.CharacterPairList.Add(characterPair);
                    animation.MotionList.Add(motionList);
                }

                animationOffsets.AddCharacterAnimationsList(animation);
            }
            else
            {
                animation = new AnimationsList(animationName);
                MotionList motionList = new MotionList(motion);
                motionList.CharacterPairList.Add(characterPair);
                animation.MotionList.Add(motionList);
                animationOffsets.AddCharacterAnimationsList(animation);
            }

            SaveOffsetsToFile();
        }

        public static void SaveOffsetsToFile()
        {
            if (animationOffsets == null)
                return;

            // Create an XML serializer so we can store the offset configuration in an XML file
            XmlSerializer serializer = new XmlSerializer(typeof(AnimationOffsets));

            // Create a new file stream in which the offset will be stored
            StreamWriter OffsetFile;
            try
            {
                // Store the setup data
                OffsetFile = new StreamWriter("UserData/BetterHScenesOffsets.xml");
                serializer.Serialize(OffsetFile, animationOffsets);
                // serializer.Serialize(fileStream, offsets);
            }
            catch
            {
                AI_BetterHScenes.Logger.LogMessage("save exception!");
                return;
            }

            // Close the file
            OffsetFile.Flush();
            OffsetFile.Close();

            AI_BetterHScenes.Logger.LogMessage("Offsets Saved");

            return;
        }

        public static void LoadOffsetsFromFile()
        {
            // Create an XML serializer so we can read the offset configuration in an XML file
            XmlSerializer serializer = new XmlSerializer(typeof(AnimationOffsets));

            Stream OffsetFile;
            try
            {
                // Read in the data
                OffsetFile = new FileStream("UserData/BetterHScenesOffsets.xml", FileMode.Open);
                animationOffsets = (AnimationOffsets)serializer.Deserialize(OffsetFile);
            }
            catch
            {
                AI_BetterHScenes.Logger.LogMessage("read error!");
                return;
            }

            // Close the file
            OffsetFile.Close();

            return;
        }

        private static void SetupVariables(HScene __instance)
        {
            map = GameObject.Find("map00_Beach");
            if (map == null)
                map = GameObject.Find("map_01_data");

            sun = GameObject.Find("CommonSpace/MapRoot/MapSimulation(Clone)/EnviroSkyGroup(Clone)/Enviro Directional Light").GetComponent<Light>();
            collisionHelpers = new List<SkinnedCollisionHelper>();

            hScene = __instance;
            var hTrav = Traverse.Create(__instance);

            hFlagCtrl = __instance.ctrlFlag;
            manager = hTrav.Field("hSceneManager").GetValue<HSceneManager>();
            hSprite = hTrav.Field("sprite").GetValue<HSceneSprite>();

            characters = new List<ChaControl>();
            characters.AddRange(__instance.GetMales());
            characters.AddRange(__instance.GetFemales());

            currentAnimation = __instance.StartAnimInfo;
            cameraShouldLock = true;

            if (__instance.ctrlFlag != null)
            {
                Traverse.Create(__instance.ctrlFlag).Field("gotoFaintnessCount").SetValue(countToWeakness.Value);

                if (__instance.ctrlFlag.cameraCtrl != null)
                    hCamera = __instance.ctrlFlag.cameraCtrl;
            }

            finishObjects = new List<GameObject>();
            foreach (var name in Tools.finishFindTransforms)
                finishObjects.Add(hSprite.categoryFinish.transform.Find(name).gameObject);

            UI.InitDraggersUI();
        }

        public static bool UseOneOffsetForAllMotions()
        {
            return useOneOffsetForAllMotions.Value;
        }
    }
}