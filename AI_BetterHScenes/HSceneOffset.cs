using System.Linq;
using System.Xml.Serialization;
using System.IO;
using UnityEngine;

namespace AI_BetterHScenes
{
    class HSceneOffset
    {
        //-- Apply character offsets for current animation, if they can be found --//
        public static void ApplyCharacterOffsets()
        {
            string currentAnimation = AI_BetterHScenes.hFlagCtrl.nowAnimationInfo.nameAnimation;
            bool bValidOffsetsFound = false;

            if (currentAnimation == null || AI_BetterHScenes.currentMotion == null)
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

                AnimationsList animationList = AI_BetterHScenes.animationOffsets.Animations.Find(x => x.AnimationName == currentAnimation);
                if (animationList != null && characterPairName != null)
                {
                    MotionList motionList;
                    if (AI_BetterHScenes.useOneOffsetForAllMotions.Value == true)
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
                                    bValidOffsetsFound = true;
                                }
                            }
                        }
                    }
                }
            }

            // if we didn't find offsets to apply, move the characters to their 0 position, in case they were moved out of it from another offset.
            if (!bValidOffsetsFound)
            {
                foreach (var character in AI_BetterHScenes.characters.Where(character => character != null))
                {
                    character.SetPosition(new Vector3(0, 0, 0));
                    character.SetRotation(new Vector3(0, 0, 0));
                }
            }

            if (AI_BetterHScenes.useSliderUI.Value == true)
                SliderUI.UpdateUIPositions();
        }

        //-- Save the character pair of offsets to the xml file, overwriting if necessary --//
        public static void SaveCharacterPairPosition(CharacterPairList characterPair, bool isDefault = false)
        {
            string currentAnimation = AI_BetterHScenes.hFlagCtrl.nowAnimationInfo.nameAnimation;

            if (currentAnimation == null || characterPair == null || AI_BetterHScenes.currentMotion == null)
                return;

            string motion = AI_BetterHScenes.currentMotion;
            if (isDefault)
                motion = "default";

            AI_BetterHScenes.Logger.LogMessage("Saving Offsets for " + currentAnimation + " Motion " + motion + " for characters " + characterPair);

            AnimationsList animation = AI_BetterHScenes.animationOffsets.Animations.Find(x => x.AnimationName == currentAnimation);

            if (animation != null)
            {
                AI_BetterHScenes.animationOffsets.Animations.Remove(animation);

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

                AI_BetterHScenes.animationOffsets.AddCharacterAnimationsList(animation);
            }
            else
            {
                animation = new AnimationsList(currentAnimation);
                MotionList motionList = new MotionList(motion);
                motionList.CharacterPairList.Add(characterPair);
                animation.MotionList.Add(motionList);
                AI_BetterHScenes.animationOffsets.AddCharacterAnimationsList(animation);
            }

            SaveOffsetsToFile();
        }

        public static void SaveOffsetsToFile()
        {
            if (AI_BetterHScenes.animationOffsets == null)
                return;

            // Create an XML serializer so we can store the offset configuration in an XML file
            XmlSerializer serializer = new XmlSerializer(typeof(AnimationOffsets));

            // Create a new file stream in which the offset will be stored
            StreamWriter OffsetFile;
            try
            {
                // Store the setup data
                OffsetFile = new StreamWriter(AI_BetterHScenes.offsetFile.Value);
                serializer.Serialize(OffsetFile, AI_BetterHScenes.animationOffsets);
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
                OffsetFile = new FileStream(AI_BetterHScenes.offsetFile.Value, FileMode.Open);
                AI_BetterHScenes.animationOffsets = (AnimationOffsets)serializer.Deserialize(OffsetFile);
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

    }
}
