using System;
using System.Linq;

using UnityEngine;

namespace HS2_BetterHScenes
{
    public static class SliderUI
    {
        private const int uiWidth = 600;
        private const int uiHeight = 256;
        private static Rect window = new Rect(0, 0, uiWidth, uiHeight);

        public static int selectedCharacter = 0;
        public static int selectedOffset = 0;

        public static CharacterOffsetLocations[] characterOffsets;
        private static OffsetVectors[][] copyOffsetVectors;
        public static float[] shoeOffsets;

        public static void InitDraggersUI()
        {
            characterOffsets = new CharacterOffsetLocations[HS2_BetterHScenes.characters.Count];
            copyOffsetVectors = new OffsetVectors[HS2_BetterHScenes.characters.Count][];
            shoeOffsets = new float[HS2_BetterHScenes.characters.Count];

            for (var charIndex = 0; charIndex < HS2_BetterHScenes.characters.Count; charIndex++)
            {
                characterOffsets[charIndex] = new CharacterOffsetLocations();
                copyOffsetVectors[charIndex] = new OffsetVectors[(int)BodyPart.BodyPartsCount];
                shoeOffsets[charIndex] = 0;

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

        public static void LoadOffsets(int charIndex, OffsetVectors[] offsetValues, float shoeOffset)
        {
            if (charIndex >= characterOffsets.Length)
                return;

            characterOffsets[charIndex].offsetVectors = offsetValues;

            if (shoeOffset != 0.0)
                shoeOffsets[charIndex] = shoeOffset;
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
            string characterPairName = null;
            foreach (var character in HS2_BetterHScenes.characters.Where(character => character != null && character.visibleAll))
            {
                if (characterPairName == null)
                    characterPairName = character.fileParam.fullname;
                else
                    characterPairName += "_" + character.fileParam.fullname;
            }

            if (characterPairName == null)
                return;

            var characterPair = new CharacterPairList(characterPairName);

            for (var charIndex = 0; charIndex < HS2_BetterHScenes.characters.Count; charIndex++)
            {
                if (!HS2_BetterHScenes.characters[charIndex].visibleAll)
                    continue;

                var characterName = HS2_BetterHScenes.characters[charIndex].fileParam.fullname;
                var characterOffsetParams = new CharacterOffsets(characterName, characterOffsets[charIndex].offsetVectors, shoeOffsets[charIndex]);

                characterPair.AddCharacterOffset(characterOffsetParams);
            }

            HSceneOffset.SaveCharacterPairPosition(characterPair, bAsDefault);
        }

        private static void CopyPositions()
        {
            for (var charIndex = 0; charIndex < copyOffsetVectors.Length; charIndex++)
            {
                for (var bodyPart = 0; bodyPart < copyOffsetVectors[charIndex].Length; bodyPart++)
                {
                    copyOffsetVectors[charIndex][bodyPart] = new OffsetVectors(characterOffsets[charIndex].offsetVectors[bodyPart].position, characterOffsets[charIndex].offsetVectors[bodyPart].rotation, characterOffsets[charIndex].offsetVectors[bodyPart].hintPosition);
                }
            }
        }

        private static void PastePositions()
        {
            for (var charIndex = 0; charIndex < copyOffsetVectors.Length; charIndex++)
            {
                for (var bodyPart = 0; bodyPart < copyOffsetVectors[charIndex].Length; bodyPart++)
                {
                    characterOffsets[charIndex].offsetVectors[bodyPart] = new OffsetVectors(copyOffsetVectors[charIndex][bodyPart].position, copyOffsetVectors[charIndex][bodyPart].rotation, copyOffsetVectors[charIndex][bodyPart].hintPosition);
                }
            }

            ApplyPositions();
        }

        public static void ResetPositions()
        {
            for (var charIndex = 0; charIndex < characterOffsets.Length; charIndex++)
            {
                for (var offset = 0; offset < characterOffsets[charIndex].offsetVectors.Length; offset++)
                {
                    characterOffsets[charIndex].offsetVectors[offset] = new OffsetVectors(Vector3.zero, Vector3.zero, Vector3.zero);
                }
            }

            ApplyPositions();
        }

        public static void ApplyPositions()
        {
            for (var charIndex = 0; charIndex < characterOffsets.Length; charIndex++)
                MoveCharacter(charIndex, characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].position, characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].rotation);
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
                selectedOffset = GUILayout.SelectionGrid(selectedOffset, offsetNames, offsetNames.Length, gridStyle, GUILayout.Height(30));
                using (GUILayout.HorizontalScope linkScope = new GUILayout.HorizontalScope("box"))
                {
                    GUILayout.Label("Heel Offset: ", GUILayout.Width((uiWidth / 5) - 10));
                    shoeOffsets[selectedCharacter] = Convert.ToSingle(GUILayout.TextField(shoeOffsets[selectedCharacter].ToString("0.000"), GUILayout.Width((uiWidth / 5) - 10)));
                    if (GUILayout.Button("Mirror Active Limb"))
                        MirrorActiveLimb();
                }

                GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1f));

                float sliderMaxRotation = HS2_BetterHScenes.sliderMaxLimbRotation.Value;
                float sliderMaxPosition = HS2_BetterHScenes.sliderMaxLimbPosition.Value;
                Vector3 lastPosition = Vector3.zero;
                Vector3 lastRotation = Vector3.zero;
                if (selectedOffset == (int)BodyPart.WholeBody)
                {
                    sliderMaxRotation = HS2_BetterHScenes.sliderMaxBodyRotation.Value;
                    sliderMaxPosition = HS2_BetterHScenes.sliderMaxBodyPosition.Value;
                    lastPosition = new Vector3(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.x, characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.y, characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.z);
                    lastRotation = new Vector3(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.x, characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.y, characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.z);
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
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.x = GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.x, -sliderMaxPosition, sliderMaxPosition);
                    }

                    using (GUILayout.VerticalScope verticalScopeY = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeY = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Position Y");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.y = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.y = GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.y, -sliderMaxPosition, sliderMaxPosition);
                    }

                    using (GUILayout.VerticalScope verticalScopeZ = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeZ = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Position Z");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.z = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.z = GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.z, -sliderMaxPosition, sliderMaxPosition);
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
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.x = GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.x, -sliderMaxRotation, sliderMaxRotation);
                    }

                    using (GUILayout.VerticalScope verticalScopeY = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeY = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Rotation Y");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.y = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.y = GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.y, -sliderMaxRotation, sliderMaxRotation);
                    }

                    using (GUILayout.VerticalScope verticalScopeZ = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeZ = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Rotation Z");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.z = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.z = GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation.z, -sliderMaxRotation, sliderMaxRotation);
                    }
                }

                if (selectedOffset != (int)BodyPart.WholeBody)
                {
                    using (GUILayout.HorizontalScope rotationScope = new GUILayout.HorizontalScope("box"))
                    {
                        using (GUILayout.VerticalScope verticalScopeX = new GUILayout.VerticalScope("box"))
                        {
                            using (GUILayout.HorizontalScope horizontalScopeX = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label("Hint X");

                                if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                    characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hintPosition.x = 0;
                            }
                            characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hintPosition.x = GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hintPosition.x, -HS2_BetterHScenes.sliderMaxHintPosition.Value, HS2_BetterHScenes.sliderMaxHintPosition.Value);
                        }

                        using (GUILayout.VerticalScope verticalScopeY = new GUILayout.VerticalScope("box"))
                        {
                            using (GUILayout.HorizontalScope horizontalScopeY = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label("Hint Y");

                                if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                    characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hintPosition.y = 0;
                            }
                            characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hintPosition.y = GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hintPosition.y, -HS2_BetterHScenes.sliderMaxHintPosition.Value, HS2_BetterHScenes.sliderMaxHintPosition.Value);
                        }

                        using (GUILayout.VerticalScope verticalScopeZ = new GUILayout.VerticalScope("box"))
                        {
                            using (GUILayout.HorizontalScope horizontalScopeZ = new GUILayout.HorizontalScope())
                            {
                                GUILayout.Label("Hint Z");

                                if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                    characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hintPosition.z = 0;
                            }
                            characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hintPosition.z = GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hintPosition.z, -HS2_BetterHScenes.sliderMaxHintPosition.Value, HS2_BetterHScenes.sliderMaxHintPosition.Value);
                        }
                    }
                }
                else
                {
                    if (lastPosition != characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position || lastRotation != characterOffsets[selectedCharacter].offsetVectors[selectedOffset].rotation)
                        ApplyPositions();
                }

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

                    if (GUILayout.Button("Save This"))
                        SavePosition(HS2_BetterHScenes.useOneOffsetForAllMotions.Value);

                    if (HS2_BetterHScenes.useOneOffsetForAllMotions.Value == false && GUILayout.Button("Save Default"))
                        SavePosition(true);
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
            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].hintPosition.x = -characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hintPosition.x;
            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].hintPosition.y = characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hintPosition.y;
            characterOffsets[selectedCharacter].offsetVectors[mirroredOffset].hintPosition.z = characterOffsets[selectedCharacter].offsetVectors[selectedOffset].hintPosition.z;
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
    }
}