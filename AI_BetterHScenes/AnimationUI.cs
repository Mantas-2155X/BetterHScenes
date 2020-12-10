using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


namespace AI_BetterHScenes
{
    public static class AnimationUI
    {
        private const int uiWidth = 256;
        private const int uiHeight = 100;
        private static Rect window = new Rect(0, 400, uiWidth, uiHeight);

        public static int selectedMotion = 0;
        public static int currentMotion = 0;

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

            using (GUILayout.VerticalScope guiVerticalScope = new GUILayout.VerticalScope("box"))
            {
                if (AI_BetterHScenes.maleMotionList != null && AI_BetterHScenes.maleMotionList.Count > 0)
                {
                    List<string> motionNames = new List<string>();
                    for (int motionIndex = 0; motionIndex < AI_BetterHScenes.maleMotionList.Count; motionIndex++)
                        motionNames.Add(AI_BetterHScenes.maleMotionList[motionIndex].anim);

                    if (AI_BetterHScenes.currentMotion != null && !motionNames.IsNullOrEmpty())
                    {
                        currentMotion = motionNames.IndexOf(AI_BetterHScenes.currentMotion);

                        if (currentMotion >= 0)
                        {
                            selectedMotion = GUILayout.SelectionGrid(currentMotion, motionNames.ToArray(), 2, gridStyle);

                            if (selectedMotion != currentMotion)
                            {
                                Console.WriteLine("Apply Motion " + selectedMotion + ": " + AI_BetterHScenes.maleMotionList[selectedMotion].anim);
                                AI_BetterHScenes.SwitchAnimations(AI_BetterHScenes.maleMotionList[selectedMotion].anim);
                                currentMotion = selectedMotion;
                            }
                        }
                    }
                }
            }

            GUI.DragWindow();
        }

        public static void DrawAnimationUI() => window = GUILayout.Window(789456124, window, DrawWindow, "Animation Selection UI", GUILayout.Width(uiWidth), GUILayout.Height(uiHeight));
    }
}
