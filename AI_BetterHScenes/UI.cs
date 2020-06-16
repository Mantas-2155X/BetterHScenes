using System.Collections.Generic;
using System.Linq;
using AIChara;
using UnityEngine;

namespace AI_BetterHScenes
{
    public static class UI
    {
        private static readonly string[] increments =
        {
            "0.01",
            "0.1",
            "0.5",
            "1",
            "2",
            "-0.01",
            "-0.1",
            "-0.5",
            "-1",
            "-2",
            "5",
            "15",
            "30",
            "45",
            "90",
            "-5",
            "-15",
            "-30",
            "-45",
            "-90"
        };
        
        private static readonly string[] axis =
        {
            "X",
            "Y",
            "Z",
            "P",
            "Y",
            "R"
        };
        
        private static bool[] axisToggles;
        private static bool[] characterToggles;
        private static List<ChaControl> validCharacters;

        private const int uiWidth = 425;
        private const int uiHeight = 179;
        
        private static Rect window = new Rect(Screen.width / 2 - uiWidth / 2, 10, uiWidth, uiHeight);
        private static Vector2 charaScrollPos;
        private static int selInt = -1;
        private static int dragType;
        
        public static void InitDraggersUI()
        {
            validCharacters = new List<ChaControl>();
            
            foreach (var chara in AI_BetterHScenes.characters.Where(chara => chara != null))
                validCharacters.Add(chara);

            characterToggles = new bool[validCharacters.Count];
            axisToggles = new bool[axis.Length];

            dragType = 0;
        }
        
        public static void DrawDraggersUI()
        {
            window = GUILayout.Window(789456123, window, DrawWindow, "Character Dragger UI", GUILayout.Width(uiWidth), GUILayout.Height(uiHeight));
        }

        private static void MoveCharacter(float amount)
        {
            for(int i = 0; i < characterToggles.Length; i++)
            {
                if (!characterToggles[i])
                    continue;

                switch (dragType)
                {
                    case 0:
                    {
                        Vector3 characterPosition = validCharacters[i].GetPosition();

                        validCharacters[i].SetPosition(new Vector3(
                            axisToggles[0] ? characterPosition.x + amount : characterPosition.x,
                            axisToggles[1] ? characterPosition.y + amount : characterPosition.y,
                            axisToggles[2] ? characterPosition.z + amount : characterPosition.z
                            ));

                        characterPosition = validCharacters[i].GetPosition();
                        break;
                    }
                    case 3:
                    {
                        Vector3 characterAngle = validCharacters[i].GetRotation();

                        validCharacters[i].SetRotation(new Vector3(
                            axisToggles[0 + 3] ? characterAngle.x + amount : characterAngle.x,
                            axisToggles[1 + 3] ? characterAngle.y + amount : characterAngle.y,
                            axisToggles[2 + 3] ? characterAngle.z + amount : characterAngle.z
                            ));

                        characterAngle = validCharacters[i].GetRotation();
                        break;
                    }
                }
            }
        }

        private static void ResetCharacterPosition()
        {
            for (int i = 0; i < characterToggles.Length; i++)
            {
                if (!characterToggles[i])
                    continue;

                switch (dragType)
                {
                    case 0:
                        {
                            Vector3 characterPosition = validCharacters[i].GetPosition();

                            validCharacters[i].SetPosition(new Vector3(
                                axisToggles[0] ? 0 : characterPosition.x,
                                axisToggles[1] ? 0 : characterPosition.y,
                                axisToggles[2] ? 0 : characterPosition.z
                                ));

                            characterPosition = validCharacters[i].GetPosition();
                            break;
                        }
                    case 3:
                        {
                            Vector3 characterAngle = validCharacters[i].GetRotation();

                            validCharacters[i].SetRotation(new Vector3(
                                axisToggles[0 + 3] ? 0 : characterAngle.x,
                                axisToggles[1 + 3] ? 0 : characterAngle.y,
                                axisToggles[2 + 3] ? 0 : characterAngle.z
                                ));

                            characterAngle = validCharacters[i].GetRotation();
                            break;
                        }
                }
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

            AI_BetterHScenes.SaveCharacterPairPosition(characterPair, bAsDefault);
        }

        private static void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            
                GUILayout.BeginArea(new Rect(5, 20, uiWidth / 4f, uiHeight - 25), GUI.skin.box);
                
                    GUILayout.BeginVertical();
                        
                        charaScrollPos = GUILayout.BeginScrollView(charaScrollPos, true, true, GUILayout.Width(-5 + uiWidth / 4), GUILayout.Height(uiHeight - 60));
                        
                            for(int i = 0; i < characterToggles.Length; i++)
                                characterToggles[i] = GUILayout.Toggle(characterToggles[i], validCharacters[i].chaFile.parameter.fullname);

                        GUILayout.EndScrollView();
                        
                        GUILayout.BeginHorizontal();

                            if (GUILayout.Button("All"))
                                for(int i = 0; i < validCharacters.Count; i++)
                                    characterToggles[i] = true;

                            if (GUILayout.Button("None"))
                                for(int i = 0; i < validCharacters.Count; i++)
                                    characterToggles[i] = false;

                        GUILayout.EndHorizontal();
                        
                    GUILayout.EndVertical();

                GUILayout.EndArea();
                
                GUILayout.BeginArea(new Rect(8 + uiWidth / 4f, 20, uiWidth - 13 - uiWidth / 4f, uiHeight - 25), GUI.skin.box);
                
                    GUILayout.BeginVertical();
                    
                        GUILayout.BeginHorizontal();
                        
                            for(int i = dragType; i < 3 + dragType; i++)
                                axisToggles[i] = GUILayout.Toggle(axisToggles[i], axis[i]);

                            if (GUILayout.Button("All"))
                                for(int i = dragType; i < 3 + dragType; i++)
                                    axisToggles[i] = true;

                            if (GUILayout.Button("None"))
                                for(int i = dragType; i < 3 + dragType; i++)
                                    axisToggles[i] = false;

                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();

                            if (GUILayout.Button(dragType == 0 ? ">Position<" : " Position "))
                                dragType = 0;
            
                            if (GUILayout.Button(dragType == 3 ? ">Rotation<" : " Rotation "))
                                dragType = 3;
                            
				            if (GUILayout.Button("Reset"))
				                ResetCharacterPosition();

				            if (GUILayout.Button("Save"))
				                SavePosition(AI_BetterHScenes.UseOneOffsetForAllMotions());

                            if (AI_BetterHScenes.UseOneOffsetForAllMotions() == false)
                            {
                                if (GUILayout.Button("Default"))
                                    SavePosition(true);
                            }

						GUILayout.EndHorizontal();
                            
                        GUILayout.BeginHorizontal();

                            selInt = -1;
                            selInt = GUILayout.SelectionGrid(selInt, increments, 5);

                            if (selInt > -1)
                                MoveCharacter(float.Parse(increments[selInt]));
                            
                        GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                        
                GUILayout.EndArea();
                
            GUILayout.EndHorizontal();
            
            GUI.DragWindow();
        }
    }
}