using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace HS2_BetterHScenes
{
    public static class SliderUI
    {
        private const int uiWidth = 600;
        private const int uiHeight = 256;
        private const int uiDeleteWidth = uiWidth / 2;
        private const int uiDeleteHeight = uiHeight / 4;
        private static Rect window = new Rect(0, 0, uiWidth, uiHeight);
        private static Rect deleteWindow = new Rect(0, 0, uiDeleteWidth, uiDeleteHeight);

        public static int selectedCharacter = 0;
        public static int selectedOffset = 0;

        public static CharacterOffsetLocations[] characterOffsets;
        private static OffsetVectors[][] copyOffsetVectors;
        private static bool[][] copyJointCorrections;
        public static float[] shoeOffsets;
        public static float[] mouthOffsets;

        public static void InitDraggersUI()
        {
            characterOffsets = new CharacterOffsetLocations[HS2_BetterHScenes.characters.Count];
            copyOffsetVectors = new OffsetVectors[HS2_BetterHScenes.characters.Count][];
            copyJointCorrections = new bool[HS2_BetterHScenes.characters.Count][];
            shoeOffsets = new float[HS2_BetterHScenes.characters.Count];
            mouthOffsets = new float[HS2_BetterHScenes.characters.Count];

            for (var charIndex = 0; charIndex < HS2_BetterHScenes.characters.Count; charIndex++)
            {
                characterOffsets[charIndex] = new CharacterOffsetLocations();
                copyOffsetVectors[charIndex] = new OffsetVectors[(int)BodyPart.BodyPartsCount];
                copyJointCorrections[charIndex] = new bool[(int)BodyPart.BodyPartsCount];
                shoeOffsets[charIndex] = 0;
                mouthOffsets[charIndex] = 0;

                characterOffsets[charIndex].LoadCharacterTransforms(HS2_BetterHScenes.characters[charIndex]);
            }

            UpdateUIPositions();
        }

        public static void UpdateUIPositions()
        {
            if (HS2_BetterHScenes.characters.Count <= 0)
                return;

            for (var charIndex = 0; charIndex < characterOffsets.Length; charIndex++)
            {
                if (characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody] == null)
                    continue;

                if (characterOffsets[charIndex].offsetTransforms[(int)BodyPart.WholeBody] == null)
                    continue;

                characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].position = characterOffsets[charIndex].offsetTransforms[(int)BodyPart.WholeBody].localPosition;
                characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].rotation = characterOffsets[charIndex].offsetTransforms[(int)BodyPart.WholeBody].localEulerAngles;

                if (characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].rotation.x > 180)
                    characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].rotation.x -= 360;

                if (characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].rotation.y > 180)
                    characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].rotation.y -= 360;

                if (characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].rotation.z > 180)
                    characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].rotation.z -= 360;
            }
        }

        public static void LoadOffsets(int charIndex, OffsetVectors[] offsetValues, bool[] jointCorrections, float shoeOffset, float mouthOffset)
        {
            if (charIndex >= characterOffsets.Length)
                return;

            characterOffsets[charIndex].offsetVectors = offsetValues;
            characterOffsets[charIndex].jointCorrection = jointCorrections;
            shoeOffsets[charIndex] = shoeOffset;
            mouthOffsets[charIndex] = mouthOffset;
        }

        private static void MoveCharacter(int charIndex, Vector3 position, Vector3 rotation)
        {
            if (charIndex >= HS2_BetterHScenes.characters.Count)
                return;

            characterOffsets[charIndex].offsetTransforms[(int)BodyPart.WholeBody].localPosition = position;
            characterOffsets[charIndex].offsetTransforms[(int)BodyPart.WholeBody].localEulerAngles = rotation;
        }

        private static void SavePosition(bool bAsDefault = false)
        {
            List<string> characterNames = new List<string>();
            List<OffsetVectors[]> offsetsList = new List<OffsetVectors[]>();
            List<float> shoeOffsetList = new List<float>();
            List<float> mouthOffsetList = new List<float>();
            List<bool[]> jointCorrectionsList = new List<bool[]>();

            for (var charIndex = 0; charIndex < HS2_BetterHScenes.characters.Count; charIndex++)
            {
                if (!HS2_BetterHScenes.characters[charIndex].visibleAll)
                    continue;

                characterNames.Add(HS2_BetterHScenes.characters[charIndex].fileParam.fullname);
                offsetsList.Add(characterOffsets[charIndex].offsetVectors);
                jointCorrectionsList.Add(characterOffsets[charIndex].jointCorrection);
                shoeOffsetList.Add(shoeOffsets[charIndex]);
                mouthOffsetList.Add(mouthOffsets[charIndex]);
            }

            HSceneOffset.SaveCharacterGroupOffsets(characterNames, offsetsList, jointCorrectionsList, shoeOffsetList, mouthOffsetList, bAsDefault);
        }

        private static void CopyPositions()
        {
            for (var charIndex = 0; charIndex < copyOffsetVectors.Length; charIndex++)
            {
                for (var bodyPart = 0; bodyPart < copyOffsetVectors[charIndex].Length; bodyPart++)
                {
                    copyOffsetVectors[charIndex][bodyPart] = new OffsetVectors(
                        characterOffsets[charIndex].offsetVectors[bodyPart].position,
                        characterOffsets[charIndex].offsetVectors[bodyPart].rotation,
                        characterOffsets[charIndex].offsetVectors[bodyPart].hint,
                        characterOffsets[charIndex].offsetVectors[bodyPart].digitRotation);
                    copyJointCorrections[charIndex][bodyPart] = characterOffsets[charIndex].jointCorrection[bodyPart];
                }
            }
        }

        private static void PastePositions()
        {
            for (var charIndex = 0; charIndex < copyOffsetVectors.Length; charIndex++)
            {
                for (var bodyPart = 0; bodyPart < copyOffsetVectors[charIndex].Length; bodyPart++)
                {
                    characterOffsets[charIndex].offsetVectors[bodyPart] = new OffsetVectors(
                        copyOffsetVectors[charIndex][bodyPart].position, 
                        copyOffsetVectors[charIndex][bodyPart].rotation, 
                        copyOffsetVectors[charIndex][bodyPart].hint,
                        copyOffsetVectors[charIndex][bodyPart].digitRotation);
                    characterOffsets[charIndex].jointCorrection[bodyPart] = copyJointCorrections[charIndex][bodyPart];
                }
            }

            ApplyPositionsAndCorrections();
        }

        public static void ResetPositions()
        {
            for (var charIndex = 0; charIndex < characterOffsets.Length; charIndex++)
            {
                for (var offset = 0; offset < characterOffsets[charIndex].offsetVectors.Length; offset++)
                {
                    characterOffsets[charIndex].offsetVectors[offset] = new OffsetVectors(Vector3.zero, Vector3.zero, Vector3.zero, 0f);
                    characterOffsets[charIndex].jointCorrection[offset] = HS2_BetterHScenes.defaultJointCorrection.Value;
                }
            }

            ApplyPositionsAndCorrections();
        }

        private static void DeleteCurrentPositionOffsets()
        {
            List<string> characterNames = new List<string>();

            for (var charIndex = 0; charIndex < HS2_BetterHScenes.characters.Count; charIndex++)
            {
                if (!HS2_BetterHScenes.characters[charIndex].visibleAll)
                    continue;

                characterNames.Add(HS2_BetterHScenes.characters[charIndex].fileParam.fullname);
            }

            HSceneOffset.DeleteCharacterGroupOffsets(characterNames);

            ResetPositions();
        }

        public static void ApplyPositionsAndCorrections()
        {
            for (var charIndex = 0; charIndex < characterOffsets.Length; charIndex++)
            {
                MoveCharacter(charIndex, characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].position, characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].rotation);
                characterOffsets[charIndex].ApplyJointCorrections();
            }
        }

        public static void ApplyLimbOffsets(int charIndex, bool useLastFramesSolution, bool useReplacementTransforms, bool leftFootJob, bool rightFootJob, bool shoeOffset, bool kissOffset)
        {
            if (charIndex >= characterOffsets.Length)
                return;

            characterOffsets[charIndex].ApplyLimbOffsets(useLastFramesSolution, useReplacementTransforms, leftFootJob, rightFootJob, shoeOffset, shoeOffsets[charIndex]);

            if (!kissOffset || charIndex != 0)
                return;

            Transform femaleMouth = characterOffsets[HS2_BetterHScenes.maleCharacters.Count]?.mouthTransform;
            if (femaleMouth == null)
                return;

            characterOffsets[charIndex].ApplyKissOffset(femaleMouth);
        }

        public static void ApplyMouthOffset(int charIndex, FBSCtrlMouth mouthControl)
        {
            if (charIndex >= characterOffsets.Length || mouthOffsets[charIndex] <= 0 || mouthControl == null || mouthControl.FBSTarget.Length == 0)
                return;

            foreach (FBSTargetInfo fbstargetInfo in mouthControl.FBSTarget)
            {
                SkinnedMeshRenderer skinnedMeshRenderer = fbstargetInfo.GetSkinnedMeshRenderer();
                if (skinnedMeshRenderer.name != "o_head")
                    continue;

                var mouthShape = skinnedMeshRenderer.GetBlendShapeWeight(46);
                if (mouthShape <= 0)
                    continue;

                skinnedMeshRenderer.SetBlendShapeWeight(53, mouthShape * mouthOffsets[charIndex]);
            }
        }

        public static void UpdateDependentStatus()
        {
            for (var charIndex = 0; charIndex < characterOffsets.Length && charIndex < HS2_BetterHScenes.characters.Count; charIndex++)
                characterOffsets[charIndex].UpdateDependentStatus(HS2_BetterHScenes.characters[charIndex]);
        }

        private static void DrawWindow(int id)
        {
            GUIStyle lineStyle = new GUIStyle("box");
            lineStyle.border.top = lineStyle.border.bottom = 1;
            lineStyle.margin.top = lineStyle.margin.bottom = 1;
            lineStyle.padding.top = lineStyle.padding.bottom = 1;

            GUIStyle gridStyle = new GUIStyle("Button");
            gridStyle.onNormal.background = Texture2D.whiteTexture;
            gridStyle.onNormal.textColor = Color.black;
            gridStyle.onHover.background = Texture2D.whiteTexture;
            gridStyle.onHover.textColor = Color.black;
            gridStyle.onActive.background = Texture2D.whiteTexture;
            gridStyle.onActive.textColor = Color.black;

            string[] characterNames = new string[HS2_BetterHScenes.characters.Count];
            string[] offsetNames = new string[] { "Whole Body", "Left Hand", "Right Hand", "Left Foot", "Right Foot" };
            for (var charIndex = 0; charIndex < HS2_BetterHScenes.characters.Count; charIndex++)
            {
                characterNames[charIndex] = HS2_BetterHScenes.characters[charIndex].fileParam.fullname;
            }

            using (GUILayout.VerticalScope guiVerticalScope = new GUILayout.VerticalScope("box"))
            {
                selectedCharacter = GUILayout.SelectionGrid(selectedCharacter, characterNames, HS2_BetterHScenes.characters.Count, gridStyle, GUILayout.Height(30));
                GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
                using (GUILayout.HorizontalScope linkScope = new GUILayout.HorizontalScope("box"))
                {
                    selectedOffset = GUILayout.SelectionGrid(selectedOffset, offsetNames, offsetNames.Length, gridStyle, GUILayout.Height(30));
                }
                using (GUILayout.HorizontalScope linkScope = new GUILayout.HorizontalScope("box"))
                {
                    if (selectedOffset == (int)BodyPart.WholeBody)
                    {
                        GUILayout.Label("Mouth Size: ", GUILayout.Width((2 * uiWidth / 15) - 10));
                        mouthOffsets[selectedCharacter] = GUILayout.HorizontalSlider(mouthOffsets[selectedCharacter], 0, 1, GUILayout.Width((4 * uiWidth / 15) - 10));

                        GUILayout.FlexibleSpace();

                        GUILayout.Label("Heel Offset: ", GUILayout.Width((2 * uiWidth / 15) - 10));
                        shoeOffsets[selectedCharacter] = Convert.ToSingle(GUILayout.TextField(shoeOffsets[selectedCharacter].ToString("0.000"), GUILayout.Width((2 * uiWidth / 15) - 10)));
                    }
                    else
                    {
                        if (GUILayout.Button("Mirror Limb"))
                            MirrorActiveLimb();

                        bool[] lastCorrection = new bool[(int)BodyPart.BodyPartsCount];
                        for (var part = (int)BodyPart.LeftHand; part < (int)BodyPart.BodyPartsCount; part++)
                            lastCorrection[part] = characterOffsets[selectedCharacter].jointCorrection[part];

                        characterOffsets[selectedCharacter].jointCorrection[(int)BodyPart.LeftHand] = 
                            GUILayout.Toggle(characterOffsets[selectedCharacter].jointCorrection[(int)BodyPart.LeftHand], " Correction");
                        characterOffsets[selectedCharacter].jointCorrection[(int)BodyPart.RightHand] = 
                            GUILayout.Toggle(characterOffsets[selectedCharacter].jointCorrection[(int)BodyPart.RightHand], " Correction");
                        characterOffsets[selectedCharacter].jointCorrection[(int)BodyPart.LeftFoot] = 
                            GUILayout.Toggle(characterOffsets[selectedCharacter].jointCorrection[(int)BodyPart.LeftFoot], " Correction");
                        characterOffsets[selectedCharacter].jointCorrection[(int)BodyPart.RightFoot] = 
                            GUILayout.Toggle(characterOffsets[selectedCharacter].jointCorrection[(int)BodyPart.RightFoot], " Correction");

                        bool correctionChanged = false;
                        for (var part = (int)BodyPart.LeftHand; part < (int)BodyPart.BodyPartsCount; part++)
                        {
                            if (lastCorrection[part] == characterOffsets[selectedCharacter].jointCorrection[part])
                                continue;

                            correctionChanged = true;
                            break;
                        }

                        if (correctionChanged)
                            characterOffsets[selectedCharacter].ApplyJointCorrections();
                    }
                }

                GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1f));

                string LabelName = "Hint";
                float sliderMaxHint = HS2_BetterHScenes.sliderMaxHintPosition.Value;
                float sliderMaxRotation = HS2_BetterHScenes.sliderMaxLimbRotation.Value;
                float sliderMaxPosition = HS2_BetterHScenes.sliderMaxLimbPosition.Value;
                Vector3 lastPosition = Vector3.zero;
                Vector3 lastRotation = Vector3.zero;

                if (selectedOffset == (int)BodyPart.WholeBody)
                {
                    LabelName = "Head Rotation";
                    sliderMaxHint = HS2_BetterHScenes.sliderMaxBodyRotation.Value;
                    sliderMaxRotation = HS2_BetterHScenes.sliderMaxBodyRotation.Value;
                    sliderMaxPosition = HS2_BetterHScenes.sliderMaxBodyPosition.Value;
                    lastPosition = new Vector3(
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.x, 
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.y, 
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.z);
                    lastRotation = new Vector3(
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.x, 
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.y, 
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.z);
                }

                using (GUILayout.HorizontalScope positionScope = new GUILayout.HorizontalScope("box"))
                {
                    using (GUILayout.VerticalScope verticalScopeX = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeX = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Position X");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.x = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.x = 
                            GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.x, -sliderMaxPosition, sliderMaxPosition);
                    }

                    using (GUILayout.VerticalScope verticalScopeY = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeY = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Position Y");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.y = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.y = 
                            GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.y, -sliderMaxPosition, sliderMaxPosition);
                    }

                    using (GUILayout.VerticalScope verticalScopeZ = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeZ = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Position Z");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.z = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.z = 
                            GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.z, -sliderMaxPosition, sliderMaxPosition);
                    }
                }

                using (GUILayout.HorizontalScope rotationScope = new GUILayout.HorizontalScope("box"))
                {
                    using (GUILayout.VerticalScope verticalScopeX = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeX = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Rotation X");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.x = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.x = 
                            GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.x, -sliderMaxRotation, sliderMaxRotation);
                    }

                    using (GUILayout.VerticalScope verticalScopeY = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeY = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Rotation Y");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.y = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.y = 
                            GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.y, -sliderMaxRotation, sliderMaxRotation);
                    }

                    using (GUILayout.VerticalScope verticalScopeZ = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeZ = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Rotation Z");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.z = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.z = 
                            GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.z, -sliderMaxRotation, sliderMaxRotation);
                    }
                }

                using (GUILayout.HorizontalScope rotationScope = new GUILayout.HorizontalScope("box"))
                {
                    using (GUILayout.VerticalScope verticalScopeX = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeX = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label($"{LabelName} X");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hint.x = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hint.x = 
                            GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hint.x, -sliderMaxHint, sliderMaxHint);
                    }

                    using (GUILayout.VerticalScope verticalScopeY = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeY = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label($"{LabelName} Y");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hint.y = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hint.y = 
                            GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hint.y, -sliderMaxHint, sliderMaxHint);
                    }

                    using (GUILayout.VerticalScope verticalScopeZ = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeZ = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label($"{LabelName} Z");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hint.z = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hint.z = 
                            GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hint.z, -sliderMaxHint, sliderMaxHint);
                    }
                }

                if (selectedOffset != (int)BodyPart.WholeBody)
                {
                    if (selectedOffset <= (int)BodyPart.RightHand)
                        sliderMaxRotation /= 4;

                    using (GUILayout.HorizontalScope rotationScope = new GUILayout.HorizontalScope("box"))
                    {
                        GUILayout.FlexibleSpace();

                        using (GUILayout.VerticalScope verticalScopeZ = new GUILayout.VerticalScope("box"))
                        {
                            using (GUILayout.HorizontalScope horizontalScopeZ = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label($"Digit Rotation");

                                if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                    characterOffsets[selectedCharacter].offsetVectors[selectedOffset].digitRotation = 0;
                            }

                            characterOffsets[selectedCharacter].offsetVectors[selectedOffset].digitRotation =
                                GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].digitRotation, -sliderMaxRotation, sliderMaxRotation);
                        }

                        GUILayout.FlexibleSpace();
                    }
                }

                if (selectedOffset == (int)BodyPart.WholeBody && 
                    (lastPosition != characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position || 
                     lastRotation != characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation))
                    ApplyPositionsAndCorrections();

                using (GUILayout.HorizontalScope controlScope = new GUILayout.HorizontalScope("box"))
                {
                    if (GUILayout.Button("Copy"))
                        CopyPositions();

                    if (GUILayout.Button("Paste"))
                        PastePositions();

                    if (GUILayout.Button("Reset All"))
                        ResetPositions();

                    if (GUILayout.Button("Reload"))
                        HSceneOffset.ApplyCharacterOffsets();

                    if (HS2_BetterHScenes.useOneOffsetForAllMotions.Value == false && GUILayout.Button("Save This"))
                        SavePosition(false);

                    if (GUILayout.Button(HS2_BetterHScenes.useUniqueOffsetForWeak.Value && HS2_BetterHScenes.currentMotion.StartsWith("D_") ? "Save Weak" : "Save Default"))
                        SavePosition(true);

                    if (GUILayout.Button("Delete All"))
                        HS2_BetterHScenes.activeConfirmDeleteUI = true;
                }
            }

            GUI.DragWindow();
        }

        private static void DrawConfirmDeleteWindow(int id)
        {
            GUIStyle lineStyle = new GUIStyle("box");
            lineStyle.border.top = lineStyle.border.bottom = 1;
            lineStyle.margin.top = lineStyle.margin.bottom = 1;
            lineStyle.padding.top = lineStyle.padding.bottom = 1;

            GUIStyle gridStyle = new GUIStyle("Button");
            gridStyle.onNormal.background = Texture2D.whiteTexture;
            gridStyle.onNormal.textColor = Color.black;
            gridStyle.onHover.background = Texture2D.whiteTexture;
            gridStyle.onHover.textColor = Color.black;
            gridStyle.onActive.background = Texture2D.whiteTexture;
            gridStyle.onActive.textColor = Color.black;

            using (GUILayout.VerticalScope guiVerticalScope = new GUILayout.VerticalScope("box"))
            {
                using (GUILayout.HorizontalScope horizontalScopeZ = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("This will delete all saved offsets for all animations for this position.  Are you certain?", new GUILayoutOption(GUILayoutOption.Type.alignMiddle, true));
                }

                using (GUILayout.HorizontalScope horizontalScopeZ = new GUILayout.HorizontalScope())
                {

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Yes", GUILayout.MaxWidth(uiWidth / 12)))
                    {
                        DeleteCurrentPositionOffsets();
                        HS2_BetterHScenes.activeConfirmDeleteUI = false;
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Cancel", GUILayout.MaxWidth(uiWidth / 12)))
                    {
                        HS2_BetterHScenes.activeConfirmDeleteUI = false;
                    }

                    GUILayout.FlexibleSpace();
                }
            }

            GUI.DragWindow();
        }

        private static void MirrorActiveLimb()
        {
            int mirroredOffset;

            switch (selectedOffset)
            {
                case (int)BodyPart.LeftHand:
                    mirroredOffset = (int)BodyPart.RightHand;
                    break;
                case (int)BodyPart.RightHand:
                    mirroredOffset = (int)BodyPart.LeftHand;
                    break;
                case (int)BodyPart.LeftFoot:
                    mirroredOffset = (int)BodyPart.RightFoot;
                    break;
                case (int)BodyPart.RightFoot:
                    mirroredOffset = (int)BodyPart.LeftFoot;
                    break;
                default:
                    return;
            }

            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].position.x = -characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.x;
            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].position.y = characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.y;
            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].position.z = characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.z;
            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].rotation.x = characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.x;
            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].rotation.y = -characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.y;
            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].rotation.z = -characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.z;
            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].hint.x = -characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hint.x;
            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].hint.y = characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hint.y;
            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].hint.z = characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hint.z;
            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].digitRotation = characterOffsets[selectedCharacter].offsetVectors[selectedOffset].digitRotation;
            characterOffsets[selectedCharacter].jointCorrection[mirroredOffset] = characterOffsets[selectedCharacter].jointCorrection[selectedOffset];
        }

        public static void SaveBasePoints(bool useReplacementTransforms)
        {
            for (var charIndex = 0; charIndex < characterOffsets.Length; charIndex++)
                characterOffsets[charIndex].SaveBasePoints(useReplacementTransforms);
        }

        public static void SetBaseReplacement(int charIndex, int offset, Transform basePoint)
        {
            if (charIndex < characterOffsets.Count())
                characterOffsets[charIndex].SetBaseReplacement(offset, basePoint);
        }

        public static void ClearBaseReplacements()
        {
            for (var charIndex = 0; charIndex < characterOffsets.Length; charIndex++)
                characterOffsets[charIndex].ClearBaseReplacements();
        }

        public static void DrawDraggersUI() => window = GUILayout.Window(789456123, window, DrawWindow, "Character Dragger UI", GUILayout.Width(uiWidth), GUILayout.Height(uiHeight));

        public static void DrawConfirmDeleteUI()
        {
            deleteWindow = new Rect(window.x + (window.width - uiDeleteWidth) / 2, window.y + window.height, uiDeleteWidth, uiDeleteHeight);
            deleteWindow = GUILayout.Window(789456125, deleteWindow, DrawConfirmDeleteWindow, "Offset Delete Confirmation", GUILayout.Width(uiDeleteWidth), GUILayout.Height(uiDeleteHeight));
        }
    }
}