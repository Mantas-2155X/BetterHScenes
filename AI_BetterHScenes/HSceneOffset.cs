using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using UnityEngine;

namespace AI_BetterHScenes
{
    public static class HSceneOffset
    {
        public static AnimationOffsets animationOffsets;

        //-- Apply character offsets for current animation, if they can be found --//
        public static void ApplyCharacterOffsets()
        {
            var currentAnimation = AI_BetterHScenes.hFlagCtrl.nowAnimationInfo.nameAnimation;
            var bValidOffsetsFound = false;

            if (currentAnimation == null)
                return;

            if (AI_BetterHScenes.currentMotion == null)
                AI_BetterHScenes.currentMotion = "default";

            string characterPairName = null;
            foreach (var character in AI_BetterHScenes.characters.Where(character => character != null && character.visibleAll))
            {
                if (characterPairName == null)
                    characterPairName = character.fileParam.fullname;
                else
                    characterPairName += "_" + character.fileParam.fullname;
            }

            var animationList = animationOffsets.Animations.Find(x => x.AnimationName == currentAnimation);
            if (animationList != null && characterPairName != null)
            {
                MotionList motionList;
                if (AI_BetterHScenes.useOneOffsetForAllMotions.Value)
                {
                    motionList = animationList.MotionList.Find(x => x.MotionName == "default");
                }
                else
                {
                    motionList = animationList.MotionList.Find(x => x.MotionName == AI_BetterHScenes.currentMotion);
                    if (motionList == null)
                        motionList = animationList.MotionList.Find(x => x.MotionName == "default");
                    else if (motionList.MotionName != AI_BetterHScenes.currentMotion)
                        motionList = animationList.MotionList.Find(x => x.MotionName == "default");
                }

                var characterPair = motionList?.CharacterPairList.Find(x => x.CharacterPairName == characterPairName);
                if (characterPair != null)
                {
                    for (var charIndex = 0; charIndex < AI_BetterHScenes.characters.Count; charIndex++)
                    {
                        var characterOffsetParameters = characterPair.CharacterOffsets.Find(x => x.CharacterName == AI_BetterHScenes.characters[charIndex].fileParam.fullname);
                        if (characterOffsetParameters == null)
                            continue;

                        OffsetVectors[] loadOffsets = new OffsetVectors[(int)BodyPart.BodyPartsCount];
                        loadOffsets[(int)BodyPart.WholeBody] = new OffsetVectors(new Vector3(characterOffsetParameters.PositionOffsetX, characterOffsetParameters.PositionOffsetY, characterOffsetParameters.PositionOffsetZ),
                                                                                 new Vector3(characterOffsetParameters.RotationOffsetP, characterOffsetParameters.RotationOffsetY, characterOffsetParameters.RotationOffsetR),
                                                                                 Vector3.zero);
                        loadOffsets[(int)BodyPart.LeftHand] = new OffsetVectors(new Vector3(characterOffsetParameters.LeftHandPositionOffsetX, characterOffsetParameters.LeftHandPositionOffsetY, characterOffsetParameters.LeftHandPositionOffsetZ),
                                                                                new Vector3(characterOffsetParameters.LeftHandRotationOffsetP, characterOffsetParameters.LeftHandRotationOffsetY, characterOffsetParameters.LeftHandRotationOffsetR), 
                                                                                new Vector3(characterOffsetParameters.LeftHandHintPositionOffsetX, characterOffsetParameters.LeftHandHintPositionOffsetY, characterOffsetParameters.LeftHandHintPositionOffsetZ));
                        loadOffsets[(int)BodyPart.RightHand] = new OffsetVectors(new Vector3(characterOffsetParameters.RightHandPositionOffsetX, characterOffsetParameters.RightHandPositionOffsetY, characterOffsetParameters.RightHandPositionOffsetZ),
                                                                                 new Vector3(characterOffsetParameters.RightHandRotationOffsetP, characterOffsetParameters.RightHandRotationOffsetY, characterOffsetParameters.RightHandRotationOffsetR),
                                                                                 new Vector3(characterOffsetParameters.RightHandHintPositionOffsetX, characterOffsetParameters.RightHandHintPositionOffsetY, characterOffsetParameters.RightHandHintPositionOffsetZ));
                        loadOffsets[(int)BodyPart.LeftFoot] = new OffsetVectors(new Vector3(characterOffsetParameters.LeftFootPositionOffsetX, characterOffsetParameters.LeftFootPositionOffsetY, characterOffsetParameters.LeftFootPositionOffsetZ),
                                                                                new Vector3(characterOffsetParameters.LeftFootRotationOffsetP, characterOffsetParameters.LeftFootRotationOffsetY, characterOffsetParameters.LeftFootRotationOffsetR),
                                                                                new Vector3(characterOffsetParameters.LeftFootHintPositionOffsetX, characterOffsetParameters.LeftFootHintPositionOffsetY, characterOffsetParameters.LeftFootHintPositionOffsetZ));
                        loadOffsets[(int)BodyPart.RightFoot] = new OffsetVectors(new Vector3(characterOffsetParameters.RightFootPositionOffsetX, characterOffsetParameters.RightFootPositionOffsetY, characterOffsetParameters.RightFootPositionOffsetZ),
                                                                                 new Vector3(characterOffsetParameters.RightFootRotationOffsetP, characterOffsetParameters.RightFootRotationOffsetY, characterOffsetParameters.RightFootRotationOffsetR),
                                                                                 new Vector3(characterOffsetParameters.RightFootHintPositionOffsetX, characterOffsetParameters.RightFootHintPositionOffsetY, characterOffsetParameters.RightFootHintPositionOffsetZ));

                        SliderUI.LoadOffsets(charIndex, loadOffsets);

                        bValidOffsetsFound = true;
                    }

                    SliderUI.ApplyPositions();
                }
            }

            // if we didn't find offsets to apply, move the characters to their 0 position, in case they were moved out of it from another offset.
            if (!bValidOffsetsFound)
                SliderUI.ResetPositions();

            SliderUI.UpdateUIPositions();
        }

        //-- Save the character pair of offsets to the xml file, overwriting if necessary --//
        public static void SaveCharacterPairPosition(CharacterPairList characterPair, bool isDefault = false)
        {
            var currentAnimation = AI_BetterHScenes.hFlagCtrl.nowAnimationInfo.nameAnimation;
            if (currentAnimation == null || characterPair == null || AI_BetterHScenes.currentMotion == null)
                return;

            var motion = AI_BetterHScenes.currentMotion;
            if (isDefault)
                motion = "default";

            AI_BetterHScenes.Logger.LogMessage("Saving Offsets for " + currentAnimation + " Motion " + motion + " for characters " + characterPair);

            var animation = animationOffsets.Animations.Find(x => x.AnimationName == currentAnimation);

            if (animation != null)
            {
                animationOffsets.Animations.Remove(animation);

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

                animationOffsets.AddCharacterAnimationsList(animation);
            }
            else
            {
                animation = new AnimationsList(currentAnimation);
                var motionList = new MotionList(motion);
                motionList.CharacterPairList.Add(characterPair);
                animation.MotionList.Add(motionList);
                animationOffsets.AddCharacterAnimationsList(animation);
            }

            SaveOffsetsToFile();
        }

        private static void SaveOffsetsToFile()
        {
            if (animationOffsets == null)
                return;

            // Create an XML serializer so we can store the offset configuration in an XML file
            var serializer = new XmlSerializer(typeof(AnimationOffsets));

            // Create a new file stream in which the offset will be stored
            StreamWriter OffsetFile;
            try
            {
                // Store the setup data
                OffsetFile = new StreamWriter(AI_BetterHScenes.offsetFile.Value);
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
        }

        public static void LoadOffsetsFromFile()
        {
            // Create an XML serializer so we can read the offset configuration in an XML file
            var serializer = new XmlSerializer(typeof(AnimationOffsets));
            animationOffsets = new AnimationOffsets();

            Stream OffsetFile;
            try
            {
                // Read in the data
                OffsetFile = new FileStream(AI_BetterHScenes.offsetFile.Value, FileMode.Open);
                animationOffsets = (AnimationOffsets)serializer.Deserialize(OffsetFile);
            }
            catch
            {
                //AI_BetterHScenes.Logger.LogMessage("read error!");
                return;
            }

            // Close the file
            OffsetFile.Close();
        }
    }
}
