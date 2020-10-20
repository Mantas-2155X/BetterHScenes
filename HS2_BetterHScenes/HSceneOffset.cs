using System.IO;
using System.Linq;
using System.Xml.Serialization;

using UnityEngine;

namespace HS2_BetterHScenes
{
    public static class HSceneOffset
    {
        //-- Apply character offsets for current animation, if they can be found --//
        public static void ApplyCharacterOffsets()
        {
            var currentAnimation = HS2_BetterHScenes.hFlagCtrl.nowAnimationInfo.nameAnimation;
            var bValidOffsetsFound = false;

            if (currentAnimation == null)
            {
                HS2_BetterHScenes.Logger.LogMessage("null Animation");
            }
            else
            {
                if (HS2_BetterHScenes.currentMotion == null)
                    HS2_BetterHScenes.currentMotion = "default";

                string characterPairName = null;
                foreach (var character in HS2_BetterHScenes.characters.Where(character => character != null && character.visibleAll))
                {
                    if (characterPairName == null)
                        characterPairName = character.fileParam.fullname;
                    else
                        characterPairName += "_" + character.fileParam.fullname;
                }

                var animationList = HS2_BetterHScenes.animationOffsets.Animations.Find(x => x.AnimationName == currentAnimation);
                if (animationList != null && characterPairName != null)
                {
                    MotionList motionList;
                    if (HS2_BetterHScenes.useOneOffsetForAllMotions.Value)
                    {
                        motionList = animationList.MotionList.Find(x => x.MotionName == "default");
                    }
                    else
                    {
                        motionList = animationList.MotionList.Find(x => x.MotionName == HS2_BetterHScenes.currentMotion);
                        if (motionList == null)
                            motionList = animationList.MotionList.Find(x => x.MotionName == "default");
                        else if (motionList.MotionName != HS2_BetterHScenes.currentMotion)
                            motionList = animationList.MotionList.Find(x => x.MotionName == "default");
                    }

                    var characterPair = motionList?.CharacterPairList.Find(x => x.CharacterPairName == characterPairName);

                    if (characterPair != null)
                    {
                        foreach (var character in HS2_BetterHScenes.characters.Where(character => character != null))
                        {
                            var characterOffsets = characterPair.CharacterOffsets.Find(x => x.CharacterName == character.fileParam.fullname);

                            if (characterOffsets == null) 
                                continue;
                            
                            var positionOffset = new Vector3(characterOffsets.PositionOffsetX, characterOffsets.PositionOffsetY, characterOffsets.PositionOffsetZ);
                            var rotationOffset = new Vector3(characterOffsets.RotationOffsetP, characterOffsets.RotationOffsetY, characterOffsets.RotationOffsetR);

                            var characterBody = character.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(HS2_BetterHScenes.bodyTransform)).FirstOrDefault();
                            characterBody.localPosition = positionOffset;
                            characterBody.localEulerAngles= rotationOffset;

                            bValidOffsetsFound = true;
                        }
                        
                        //HS2_BetterHScenes.Logger.LogMessage("Offsets Applied");
                    }
                }
            }

            // if we didn't find offsets to apply, move the characters to their 0 position, in case they were moved out of it from another offset.
            if (!bValidOffsetsFound)
            {
                foreach (var character in HS2_BetterHScenes.characters.Where(character => character != null))
                {
                    var characterBody = character.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(HS2_BetterHScenes.bodyTransform)).FirstOrDefault();
                    characterBody.localPosition = new Vector3(0, 0, 0);
                    characterBody.localEulerAngles = new Vector3(0, 0, 0);
                }
            }
            
            SliderUI.UpdateUIPositions();
        }

        //-- Save the character pair of offsets to the xml file, overwriting if necessary --//
        public static void SaveCharacterPairPosition(CharacterPairList characterPair, bool isDefault = false)
        {
            var currentAnimation = HS2_BetterHScenes.hFlagCtrl.nowAnimationInfo.nameAnimation;
            if (currentAnimation == null || characterPair == null || HS2_BetterHScenes.currentMotion == null)
                return;

            var motion = HS2_BetterHScenes.currentMotion;
            if (isDefault)
                motion = "default";

            HS2_BetterHScenes.Logger.LogMessage("Saving Offsets for " + currentAnimation + " Motion " + motion + " for characters " + characterPair);

            var animation = HS2_BetterHScenes.animationOffsets.Animations.Find(x => x.AnimationName == currentAnimation);

            if (animation != null)
            {
                HS2_BetterHScenes.animationOffsets.Animations.Remove(animation);

                var motionList = animation.MotionList.Find(x => x.MotionName == motion);
                if (motionList != null)
                {
                    animation.MotionList.Remove(motionList);
                    
                    var existingCharacterPair = motionList.CharacterPairList.Find(x => x.CharacterPairName == characterPair.CharacterPairName);
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

                HS2_BetterHScenes.animationOffsets.AddCharacterAnimationsList(animation);
            }
            else
            {
                animation = new AnimationsList(currentAnimation);
                var motionList = new MotionList(motion);
                motionList.CharacterPairList.Add(characterPair);
                animation.MotionList.Add(motionList);
                HS2_BetterHScenes.animationOffsets.AddCharacterAnimationsList(animation);
            }

            SaveOffsetsToFile();
        }

        private static void SaveOffsetsToFile()
        {
            if (HS2_BetterHScenes.animationOffsets == null)
                return;

            // Create an XML serializer so we can store the offset configuration in an XML file
            var serializer = new XmlSerializer(typeof(AnimationOffsets));

            // Create a new file stream in which the offset will be stored
            StreamWriter OffsetFile;
            try
            {
                // Store the setup data
                OffsetFile = new StreamWriter(HS2_BetterHScenes.offsetFile.Value);
                serializer.Serialize(OffsetFile, HS2_BetterHScenes.animationOffsets);
                // serializer.Serialize(fileStream, offsets);
            }
            catch
            {
                HS2_BetterHScenes.Logger.LogMessage("save exception!");
                return;
            }

            // Close the file
            OffsetFile.Flush();
            OffsetFile.Close();

            HS2_BetterHScenes.Logger.LogMessage("Offsets Saved");
        }

        public static void LoadOffsetsFromFile()
        {
            // Create an XML serializer so we can read the offset configuration in an XML file
            var serializer = new XmlSerializer(typeof(AnimationOffsets));

            Stream OffsetFile;
            try
            {
                // Read in the data
                OffsetFile = new FileStream(HS2_BetterHScenes.offsetFile.Value, FileMode.Open);
                HS2_BetterHScenes.animationOffsets = (AnimationOffsets)serializer.Deserialize(OffsetFile);
            }
            catch
            {
                //HS2_BetterHScenes.Logger.LogMessage("read error!");
                return;
            }

            // Close the file
            OffsetFile.Close();
        }
    }
}
