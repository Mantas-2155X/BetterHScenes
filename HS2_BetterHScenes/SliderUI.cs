using System.Linq;
using System.Collections.Generic;

using AIChara;

using UnityEngine;

namespace HS2_BetterHScenes
{
    public static class SliderUI
    {
        private const int uiWidth = 600;
        private const int uiHeight = 256;
        private static Rect window = new Rect(0, 0, uiWidth, uiHeight);

        private static List<ChaControl> validCharacters;
        
        private static Vector3[] charPosition;
        private static Vector3[] charRotation;
        private static Vector3[] charCopyPosition;
        private static Vector3[] charCopyRotation;
        private static Vector3[] charLastPosition;
        private static Vector3[] charLastRotation;

        public static void InitDraggersUI()
        {
            validCharacters = new List<ChaControl>();

            foreach (var chara in HS2_BetterHScenes.characters.Where(chara => chara != null))
                validCharacters.Add(chara);

            charPosition = new Vector3[validCharacters.Count];
            charRotation = new Vector3[validCharacters.Count];
            charCopyPosition = new Vector3[validCharacters.Count];
            charCopyRotation = new Vector3[validCharacters.Count];
            charLastPosition = new Vector3[validCharacters.Count];
            charLastRotation = new Vector3[validCharacters.Count];

            for (var charIndex = 0; charIndex < validCharacters.Count; charIndex++)
            {
                charCopyPosition[charIndex] = new Vector3(0, 0, 0);
                charCopyRotation[charIndex] = new Vector3(0, 0, 0);
            }

            UpdateUIPositions();
        }

        public static void UpdateUIPositions()
        {
            for(var charIndex = 0; charIndex < validCharacters.Count; charIndex++)
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
        private static void MoveCharacter(int charIndex, Vector3 position, Vector3 rotation)
        {
            if (charIndex >= validCharacters.Count) 
                return;
            
            validCharacters[charIndex].SetPosition(position);
            validCharacters[charIndex].SetRotation(rotation);
        }

        private static void SavePosition(bool bAsDefault = false)
        {
            string characterPairName = null;
            foreach (var character in validCharacters.Where(character => character != null && character.visibleAll))
            {
                if (characterPairName == null)
                    characterPairName = character.fileParam.fullname;
                else
                    characterPairName += "_" + character.fileParam.fullname;
            }

            if (characterPairName == null)
                return;

            var characterPair = new CharacterPairList(characterPairName);

            foreach (var character in validCharacters.Where(character => character != null && character.visibleAll))
            {
                var characterName = character.fileParam.fullname;
                var characterPosition = character.GetPosition();
                var characterAngle = character.GetRotation();

                var characterOffsets = new CharacterOffsets(characterName, characterPosition, characterAngle);

                characterPair.AddCharacterOffset(characterOffsets);
            }

            HSceneOffset.SaveCharacterPairPosition(characterPair, bAsDefault);
        }

        private static void CopyPositions()
        {
            for (var charIndex = 0; charIndex < validCharacters.Count; charIndex++)
            {
                charCopyPosition[charIndex] = charPosition[charIndex];
                charCopyRotation[charIndex] = charRotation[charIndex];
            }
        }

        private static void PastePositions()
        {
            for (var charIndex = 0; charIndex < validCharacters.Count; charIndex++)
            {
                charPosition[charIndex] = charCopyPosition[charIndex];
                charRotation[charIndex] = charCopyRotation[charIndex];
            }
            
            ApplyPositions();
        }

        private static void ResetPositions()
        {
            for (var charIndex = 0; charIndex < validCharacters.Count; charIndex++)
            {
                charPosition[charIndex] = new Vector3(0, 0, 0);
                charRotation[charIndex] = new Vector3(0, 0, 0);
            }
            
            ApplyPositions();
        }

        private static void ApplyPositions()
        {
            for (var charIndex = 0; charIndex < validCharacters.Count; charIndex++)
            {
                if (charPosition[charIndex] == charLastPosition[charIndex] && charRotation[charIndex] == charLastRotation[charIndex]) 
                    continue;
                
                MoveCharacter(charIndex, charPosition[charIndex], charRotation[charIndex]);
                charLastPosition[charIndex] = charPosition[charIndex];
                charLastRotation[charIndex] = charRotation[charIndex];
            }
        }

        private static void DrawWindow(int id)
        {
            var centeredStyle = new GUIStyle(GUI.skin.GetStyle("Label")) {alignment = TextAnchor.UpperCenter};
            
            var lineStyle = new GUIStyle("box");
            lineStyle.border.top = lineStyle.border.bottom = 1;
            lineStyle.margin.top = lineStyle.margin.bottom = 1;
            lineStyle.padding.top = lineStyle.padding.bottom = 1;

            for (var iCharacterIndex = 0; iCharacterIndex < validCharacters.Count; iCharacterIndex++)
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

                            charPosition[iCharacterIndex].x = GUILayout.HorizontalSlider(charPosition[iCharacterIndex].x, -HS2_BetterHScenes.sliderMaxPosition.Value, HS2_BetterHScenes.sliderMaxPosition.Value);
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

                            charPosition[iCharacterIndex].y = GUILayout.HorizontalSlider(charPosition[iCharacterIndex].y, -HS2_BetterHScenes.sliderMaxPosition.Value, HS2_BetterHScenes.sliderMaxPosition.Value);
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

                            charPosition[iCharacterIndex].z = GUILayout.HorizontalSlider(charPosition[iCharacterIndex].z, -HS2_BetterHScenes.sliderMaxPosition.Value, HS2_BetterHScenes.sliderMaxPosition.Value);
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

                            charRotation[iCharacterIndex].x = GUILayout.HorizontalSlider(charRotation[iCharacterIndex].x, -HS2_BetterHScenes.sliderMaxRotation.Value, HS2_BetterHScenes.sliderMaxRotation.Value);
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

                            charRotation[iCharacterIndex].y = GUILayout.HorizontalSlider(charRotation[iCharacterIndex].y, -HS2_BetterHScenes.sliderMaxRotation.Value, HS2_BetterHScenes.sliderMaxRotation.Value);
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

                            charRotation[iCharacterIndex].z = GUILayout.HorizontalSlider(charRotation[iCharacterIndex].z, -HS2_BetterHScenes.sliderMaxRotation.Value, HS2_BetterHScenes.sliderMaxRotation.Value);
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
                    SavePosition(HS2_BetterHScenes.useOneOffsetForAllMotions.Value);

                if (HS2_BetterHScenes.useOneOffsetForAllMotions.Value == false && GUILayout.Button("Save Default"))
                    SavePosition(true);
            }
            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }
        
        public static void DrawDraggersUI() => window = GUILayout.Window(789456123, window, DrawWindow, "Character Dragger UI", GUILayout.Width(uiWidth), GUILayout.Height(uiHeight));
    }
}