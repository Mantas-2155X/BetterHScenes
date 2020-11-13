using System;
using System.Linq;

using UnityEngine;

namespace AI_BetterHScenes
{
    public static class SliderUI
    {
        private const int uiWidth = 600;
        private const int uiHeight = 256;
        private static Rect window = new Rect(0, 0, uiWidth, uiHeight);

        public static int selectedCharacter = 0;
        public static int selectedOffset = 0;
        public static bool[] linkHands;
        public static bool[] linkFeet;

        private static CharacterOffsetLocations[] characterOffsets;
        private static OffsetVectors[][] copyOffsetVectors;

        public static void InitDraggersUI()
        {
            characterOffsets = new CharacterOffsetLocations[AI_BetterHScenes.characters.Count];
            copyOffsetVectors = new OffsetVectors[AI_BetterHScenes.characters.Count][];
            linkHands = new bool[AI_BetterHScenes.characters.Count];
            linkFeet = new bool[AI_BetterHScenes.characters.Count];

            for (var charIndex = 0; charIndex < AI_BetterHScenes.characters.Count; charIndex++)
            {
                characterOffsets[charIndex] = new CharacterOffsetLocations();
                copyOffsetVectors[charIndex] = new OffsetVectors[(int)BodyPart.BodyPartsCount];
                linkHands[charIndex] = false;
                linkFeet[charIndex] = false;

                characterOffsets[charIndex].LoadCharacterTransforms(AI_BetterHScenes.characters[charIndex]);
            }

            UpdateUIPositions();
        }

        public static void UpdateUIPositions()
        {
            if (AI_BetterHScenes.characters.Count <= 0)
                return;

            for(var charIndex = 0; charIndex < characterOffsets.Length; charIndex++)
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

        public static void LoadOffsets(int charIndex, OffsetVectors[] offsetValues)
        {
            if (charIndex < characterOffsets.Length)
                characterOffsets[charIndex].offsetVectors = offsetValues;
        }

        private static void MoveCharacter(int charIndex, Vector3 position, Vector3 rotation)
        {
            if (charIndex >= AI_BetterHScenes.characters.Count) 
                return;

            characterOffsets[charIndex].offsetTransforms[(int)BodyPart.WholeBody].localPosition = position;
            characterOffsets[charIndex].offsetTransforms[(int)BodyPart.WholeBody].localEulerAngles = rotation;
        }

        private static void SavePosition(bool bAsDefault = false)
        {
            string characterPairName = null;
            foreach (var character in AI_BetterHScenes.characters.Where(character => character != null && character.visibleAll))
            {
                if (characterPairName == null)
                    characterPairName = character.fileParam.fullname;
                else
                    characterPairName += "_" + character.fileParam.fullname;
            }

            if (characterPairName == null)
                return;

            var characterPair = new CharacterPairList(characterPairName);

            for (var charIndex = 0; charIndex < AI_BetterHScenes.characters.Count; charIndex++)
            {
                if (!AI_BetterHScenes.characters[charIndex].visibleAll)
                    continue;

                var characterName = AI_BetterHScenes.characters[charIndex].fileParam.fullname;
                var characterOffsetParams = new CharacterOffsets(characterName, characterOffsets[charIndex].offsetVectors);

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
                    copyOffsetVectors[charIndex][bodyPart] = new OffsetVectors(characterOffsets[charIndex].offsetVectors[bodyPart].position, characterOffsets[charIndex].offsetVectors[bodyPart].rotation);
                }
            }
        }

        private static void PastePositions()
        {
            for (var charIndex = 0; charIndex < copyOffsetVectors.Length; charIndex++)
            {
                for (var bodyPart = 0; bodyPart < copyOffsetVectors[charIndex].Length; bodyPart++)
                {
                    characterOffsets[charIndex].offsetVectors[bodyPart] = new OffsetVectors(copyOffsetVectors[charIndex][bodyPart].position, copyOffsetVectors[charIndex][bodyPart].rotation);
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
                    characterOffsets[charIndex].offsetVectors[offset] = new OffsetVectors(new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                }
            }

            ApplyPositions();
        }

        public static void ApplyPositions()
        {
            for (var charIndex = 0; charIndex < characterOffsets.Length; charIndex++)
            {  
                MoveCharacter(charIndex, characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].position, characterOffsets[charIndex].offsetVectors[(int)BodyPart.WholeBody].rotation);
            }
        }

        public static void ApplyLimbOffsets(int charIndex)
        {
            if (charIndex < characterOffsets.Length)
            {
                characterOffsets[charIndex].ApplyLimbOffsets();
            }
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

            string[] characterNames = new string[AI_BetterHScenes.characters.Count];
            string[] offsetNames = new string[] { "Whole Body", "Left Hand", "Right Hand", "Left Foot", "Right Foot" };
            for (var charIndex = 0; charIndex < AI_BetterHScenes.characters.Count; charIndex++)
            {
                characterNames[charIndex] = AI_BetterHScenes.characters[charIndex].fileParam.fullname;
            }

            using (GUILayout.VerticalScope guiVerticalScope = new GUILayout.VerticalScope("box"))
            {
                selectedCharacter = GUILayout.SelectionGrid(selectedCharacter, characterNames, AI_BetterHScenes.characters.Count, gridStyle, GUILayout.Height(30));
                GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
                selectedOffset = GUILayout.SelectionGrid(selectedOffset, offsetNames, offsetNames.Length, gridStyle, GUILayout.Height(30));
                using (GUILayout.HorizontalScope linkScope = new GUILayout.HorizontalScope("box"))
                {
                    GUILayout.Space((uiWidth / 5) - 10);
                    linkHands[selectedCharacter] = GUILayout.Toggle(linkHands[selectedCharacter], "Link Hands", gridStyle);
                    linkFeet[selectedCharacter] = GUILayout.Toggle(linkFeet[selectedCharacter], "Link Feet", gridStyle);
                }
                GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
                GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1f));

                float sliderMaxRotation = AI_BetterHScenes.sliderMaxRotation.Value;
                if (selectedOffset != 0)
                    sliderMaxRotation *= 2;

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
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.x = GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.x, -AI_BetterHScenes.sliderMaxPosition.Value, AI_BetterHScenes.sliderMaxPosition.Value);
                    }

                    using (GUILayout.VerticalScope verticalScopeY = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeY = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Position Y");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.y = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.y = GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.y, -AI_BetterHScenes.sliderMaxPosition.Value, AI_BetterHScenes.sliderMaxPosition.Value);
                    }

                    using (GUILayout.VerticalScope verticalScopeZ = new GUILayout.VerticalScope("box"))
                    {
                        using (GUILayout.HorizontalScope horizontalScopeZ = new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Position Z");

                            if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.z = 0;
                        }
                        characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.z = GUILayout.HorizontalSlider(characterOffsets[selectedCharacter].offsetVectors[selectedOffset].position.z, -AI_BetterHScenes.sliderMaxPosition.Value, AI_BetterHScenes.sliderMaxPosition.Value);
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

                if (linkHands[selectedCharacter])
                {
                    if (selectedOffset == (int)BodyPart.LeftHand)
                    {
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightHand].position.x = -characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftHand].position.x;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightHand].position.y = characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftHand].position.y;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightHand].position.z = characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftHand].position.z;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightHand].rotation.x = characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftHand].rotation.x;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightHand].rotation.y = -characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftHand].rotation.y;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightHand].rotation.z = -characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftHand].rotation.z;
                    }
                    else if (selectedOffset == (int)BodyPart.RightHand)
                    {
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftHand].position.x = -characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightHand].position.x;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftHand].position.y = characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightHand].position.y;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftHand].position.z = characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightHand].position.z;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftHand].rotation.x = characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightHand].rotation.x;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftHand].rotation.y = -characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightHand].rotation.y;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftHand].rotation.z = -characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightHand].rotation.z;
                    }
                }

                if (linkFeet[selectedCharacter])
                {
                    if (selectedOffset == (int)BodyPart.LeftFoot)
                    {
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightFoot].position.x = -characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftFoot].position.x;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightFoot].position.y = characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftFoot].position.y;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightFoot].position.z = characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftFoot].position.z;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightFoot].rotation.x = characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftFoot].rotation.x;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightFoot].rotation.y = -characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftFoot].rotation.y;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightFoot].rotation.z = -characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftFoot].rotation.z;
                    }
                    else if (selectedOffset == (int)BodyPart.RightFoot)
                    {
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftFoot].position.x = -characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightFoot].position.x;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftFoot].position.y = characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightFoot].position.y;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftFoot].position.z = characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightFoot].position.z;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftFoot].rotation.x = characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightFoot].rotation.x;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftFoot].rotation.y = -characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightFoot].rotation.y;
                        characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.LeftFoot].rotation.z = -characterOffsets[selectedCharacter].offsetVectors[(int)BodyPart.RightFoot].rotation.z;
                    }
                }

                ApplyPositions();

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
                        SavePosition(AI_BetterHScenes.useOneOffsetForAllMotions.Value);

                    if (AI_BetterHScenes.useOneOffsetForAllMotions.Value == false && GUILayout.Button("Save Default"))
                        SavePosition(true);
                }
            }

            GUI.DragWindow();
        }

        public static void DrawDraggersUI() => window = GUILayout.Window(789456123, window, DrawWindow, "Character Dragger UI", GUILayout.Width(uiWidth), GUILayout.Height(uiHeight));
    }
}