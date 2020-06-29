using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using AIChara;
using UnityEngine;

namespace AI_BetterHScenes
{
    public static class SliderUI
    {
        private static List<ChaControl> validCharacters;
        private static int uiWidth = 600;
        private static int uiHeight = 256;
        private static Rect window = new Rect(0, 0, uiWidth, uiHeight);

        private static Vector3[] charPosition;
        private static Vector3[] charRotation;
        private static Vector3[] charCopyPosition;
        private static Vector3[] charCopyRotation;
        private static Vector3[] charLastPosition;
        private static Vector3[] charLastRotation;

        public static void InitDraggersUI()
        {
            validCharacters = new List<ChaControl>();

            foreach (var chara in AI_BetterHScenes.characters.Where(chara => chara != null))
                validCharacters.Add(chara);

            charPosition = new Vector3[validCharacters.Count];
            charRotation = new Vector3[validCharacters.Count];
            charCopyPosition = new Vector3[validCharacters.Count];
            charCopyRotation = new Vector3[validCharacters.Count];
            charLastPosition = new Vector3[validCharacters.Count];
            charLastRotation = new Vector3[validCharacters.Count];

            for (int charIndex = 0; charIndex < validCharacters.Count; charIndex++)
            {
                charCopyPosition[charIndex] = new Vector3(0, 0, 0);
                charCopyRotation[charIndex] = new Vector3(0, 0, 0);
            }

            UpdateUIPositions();
        }

        public static void UpdateUIPositions()
        {
            for(int charIndex = 0; charIndex < validCharacters.Count; charIndex++)
            {
                charLastPosition[charIndex] = charPosition[charIndex] = validCharacters[charIndex].GetPosition();
                charLastRotation[charIndex] = charRotation[charIndex] = validCharacters[charIndex].GetRotation();

                if (charRotation[charIndex].x > 180)
                    charLastRotation[charIndex].x = charRotation[charIndex].x = charRotation[charIndex].x - 360;

                if (charRotation[charIndex].y > 180)
                    charLastRotation[charIndex].y = charRotation[charIndex].y = charRotation[charIndex].y - 360;

                if (charRotation[charIndex].z > 180)
                    charLastRotation[charIndex].z = charRotation[charIndex].z = charRotation[charIndex].z - 360;
            }
        }

        public static void DrawDraggersUI()
        {
            window = GUILayout.Window(789456123, window, DrawWindow, "Character Dragger UI", GUILayout.Width(uiWidth), GUILayout.Height(uiHeight));
        }

        private static void MoveCharacter(int charIndex, Vector3 position, Vector3 rotation)
        {
            if (charIndex < validCharacters.Count && position != null && rotation != null)
            {
                validCharacters[charIndex].SetPosition(position);
                validCharacters[charIndex].SetRotation(rotation);
            }
        }

        public static void SavePosition(bool bAsDefault = false)
        {
            string characterPairName = null;
            foreach (var character in validCharacters.Where(character => character != null))
            {
                if (characterPairName == null)
                    characterPairName = character.fileParam.fullname;
                else
                    characterPairName += "_" + character.fileParam.fullname;
            }

            if (characterPairName == null)
                return;

            CharacterPairList characterPair = new CharacterPairList(characterPairName);

            foreach (var character in validCharacters.Where(character => character != null))
            {
                string characterName = character.fileParam.fullname;
                Vector3 characterPosition = character.GetPosition();
                Vector3 characterAngle = character.GetRotation();

                CharacterOffsets characterOffsets = new CharacterOffsets(characterName, characterPosition, characterAngle);

                characterPair.AddCharacterOffset(characterOffsets);
            }

            HSceneOffset.SaveCharacterPairPosition(characterPair, bAsDefault);
        }

        public static void CopyPositions()
        {
            for (int charIndex = 0; charIndex < validCharacters.Count; charIndex++)
            {
                charCopyPosition[charIndex] = charPosition[charIndex];
                charCopyRotation[charIndex] = charRotation[charIndex];
            }
            AI_BetterHScenes.Logger.LogMessage("Offsets Copied");
        }

        public static void PastePositions()
        {
            for (int charIndex = 0; charIndex < validCharacters.Count; charIndex++)
            {
                charPosition[charIndex] = charCopyPosition[charIndex];
                charRotation[charIndex] = charCopyRotation[charIndex];
            }
            ApplyPositions();
        }

        public static void ResetPositions()
        {
            for (int charIndex = 0; charIndex < validCharacters.Count; charIndex++)
            {
                charPosition[charIndex] = new Vector3(0, 0, 0);
                charRotation[charIndex] = new Vector3(0, 0, 0);
            }
            ApplyPositions();
        }

        public static void ApplyPositions()
        {
            for (int charIndex = 0; charIndex < validCharacters.Count; charIndex++)
            {
                if (charPosition[charIndex] != charLastPosition[charIndex] || charRotation[charIndex] != charLastRotation[charIndex])
                {
                    MoveCharacter(charIndex, charPosition[charIndex], charRotation[charIndex]);
                    charLastPosition[charIndex] = charPosition[charIndex];
                    charLastRotation[charIndex] = charRotation[charIndex];
                }
            }
        }

        private static void DrawWindow(int id)
        {
            var centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperCenter;

            var lineStyle = new GUIStyle("box");
            lineStyle.border.top = lineStyle.border.bottom = 1;
            lineStyle.margin.top = lineStyle.margin.bottom = 1;
            lineStyle.padding.top = lineStyle.padding.bottom = 1;

            for (int iCharacterIndex = 0; iCharacterIndex < validCharacters.Count; iCharacterIndex++)
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Label(validCharacters[iCharacterIndex].fileParam.fullname, centeredStyle);

                    GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1f));

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical();
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Position X");

                                if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                    charPosition[iCharacterIndex].x = 0;
                            }
                            GUILayout.EndHorizontal();

                            charPosition[iCharacterIndex].x = GUILayout.HorizontalSlider(charPosition[iCharacterIndex].x, -AI_BetterHScenes.sliderMaxPosition.Value, AI_BetterHScenes.sliderMaxPosition.Value);
                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical();
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Position Y");

                                if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                    charPosition[iCharacterIndex].y = 0;
                            }
                            GUILayout.EndHorizontal();

                            charPosition[iCharacterIndex].y = GUILayout.HorizontalSlider(charPosition[iCharacterIndex].y, -AI_BetterHScenes.sliderMaxPosition.Value, AI_BetterHScenes.sliderMaxPosition.Value);
                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical();
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Position Z");

                                if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                    charPosition[iCharacterIndex].z = 0;
                            }
                            GUILayout.EndHorizontal();

                            charPosition[iCharacterIndex].z = GUILayout.HorizontalSlider(charPosition[iCharacterIndex].z, -AI_BetterHScenes.sliderMaxPosition.Value, AI_BetterHScenes.sliderMaxPosition.Value);
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.BeginVertical();
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Rotation P");

                                if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                    charRotation[iCharacterIndex].x = 0;
                            }
                            GUILayout.EndHorizontal();

                            charRotation[iCharacterIndex].x = GUILayout.HorizontalSlider(charRotation[iCharacterIndex].x, -AI_BetterHScenes.sliderMaxRotation.Value, AI_BetterHScenes.sliderMaxRotation.Value);
                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical();
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Rotation Y");

                                if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                    charRotation[iCharacterIndex].y = 0;
                            }
                            GUILayout.EndHorizontal();

                            charRotation[iCharacterIndex].y = GUILayout.HorizontalSlider(charRotation[iCharacterIndex].y, -AI_BetterHScenes.sliderMaxRotation.Value, AI_BetterHScenes.sliderMaxRotation.Value);
                        }
                        GUILayout.EndVertical();

                        GUILayout.BeginVertical();
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Rotation Z");

                                if (GUILayout.Button("Reset", GUILayout.MaxWidth(uiWidth / 12)))
                                    charRotation[iCharacterIndex].z = 0;
                            }
                            GUILayout.EndHorizontal();

                            charRotation[iCharacterIndex].z = GUILayout.HorizontalSlider(charRotation[iCharacterIndex].z, -AI_BetterHScenes.sliderMaxRotation.Value, AI_BetterHScenes.sliderMaxRotation.Value);
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();

                    ApplyPositions();

                    GUILayout.Box(GUIContent.none, lineStyle, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
                }
                GUILayout.EndVertical();
            }

            GUILayout.BeginHorizontal();
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

                if (AI_BetterHScenes.useOneOffsetForAllMotions.Value == false)
                {
                    if (GUILayout.Button("Save Default"))
                        SavePosition(true);
                }
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow();

        }
    }
}