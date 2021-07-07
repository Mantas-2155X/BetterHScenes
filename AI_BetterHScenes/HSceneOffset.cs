using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using UnityEngine;

namespace AI_BetterHScenes
{
    public static class HSceneOffset
    {
        private static BetterHScenesOffsetsXML hSceneOffsets;

        //-- Apply character offsets for current animation, if they can be found --//
        public static void ApplyCharacterOffsets()
        {
            var currentAnimation = AI_BetterHScenes.hFlagCtrl.nowAnimationInfo.nameAnimation;
            var bValidOffsetsFound = false;

            if (currentAnimation == null)
                return;

            if (AI_BetterHScenes.currentMotion == null)
                AI_BetterHScenes.currentMotion = "default";

            string characterGroupName = null;
            foreach (var character in AI_BetterHScenes.characters.Where(character => character != null && character.visibleAll))
            {
                if (characterGroupName == null)
                    characterGroupName = character.fileParam.fullname;
                else
                    characterGroupName += "_" + character.fileParam.fullname;
            }

            if (characterGroupName != null)
            {
                var characterGroup = hSceneOffsets.CharacterGroupList.Find(x => x.CharacterGroupName == characterGroupName);
                if (characterGroup != null)
                {
                    for (var charIndex = 0; charIndex < AI_BetterHScenes.characters.Count; charIndex++)
                    {
                        var character = characterGroup.CharacterList.Find(x => x.CharacterName == AI_BetterHScenes.characters[charIndex].fileParam.fullname);
                        if (character == null)
                            continue;

                        var animation = character.AnimationList.Find(x => x.AnimationName == currentAnimation);
                        if (animation == null)
                            continue;

                        var motion = animation.MotionOffsetList.Find(x => x.MotionName == AI_BetterHScenes.currentMotion);
                        if (AI_BetterHScenes.useOneOffsetForAllMotions.Value || motion == null || motion.MotionName != AI_BetterHScenes.currentMotion)
                        {
                            if (AI_BetterHScenes.useUniqueOffsetForWeak.Value && AI_BetterHScenes.currentMotion.StartsWith("D_"))
                                motion = animation.MotionOffsetList.Find(x => x.MotionName == "defaultWeak");
                            else
                                motion = animation.MotionOffsetList.Find(x => x.MotionName == "default");
                        }

                        if (motion == null)
                            continue;

                        OffsetVectors[] loadOffsets = new OffsetVectors[(int)BodyPart.BodyPartsCount];
                        loadOffsets[(int)BodyPart.WholeBody] = new OffsetVectors(new Vector3(motion.PositionOffsetX, motion.PositionOffsetY, motion.PositionOffsetZ),
                                                                                 new Vector3(motion.RotationOffsetP, motion.RotationOffsetY, motion.RotationOffsetR),
                                                                                 Vector3.zero);
                        loadOffsets[(int)BodyPart.LeftHand] = new OffsetVectors(new Vector3(motion.LeftHandPositionOffsetX, motion.LeftHandPositionOffsetY, motion.LeftHandPositionOffsetZ),
                                                                                new Vector3(motion.LeftHandRotationOffsetP, motion.LeftHandRotationOffsetY, motion.LeftHandRotationOffsetR),
                                                                                new Vector3(motion.LeftHandHintPositionOffsetX, motion.LeftHandHintPositionOffsetY, motion.LeftHandHintPositionOffsetZ));
                        loadOffsets[(int)BodyPart.RightHand] = new OffsetVectors(new Vector3(motion.RightHandPositionOffsetX, motion.RightHandPositionOffsetY, motion.RightHandPositionOffsetZ),
                                                                                 new Vector3(motion.RightHandRotationOffsetP, motion.RightHandRotationOffsetY, motion.RightHandRotationOffsetR),
                                                                                 new Vector3(motion.RightHandHintPositionOffsetX, motion.RightHandHintPositionOffsetY, motion.RightHandHintPositionOffsetZ));
                        loadOffsets[(int)BodyPart.LeftFoot] = new OffsetVectors(new Vector3(motion.LeftFootPositionOffsetX, motion.LeftFootPositionOffsetY, motion.LeftFootPositionOffsetZ),
                                                                                new Vector3(motion.LeftFootRotationOffsetP, motion.LeftFootRotationOffsetY, motion.LeftFootRotationOffsetR),
                                                                                new Vector3(motion.LeftFootHintPositionOffsetX, motion.LeftFootHintPositionOffsetY, motion.LeftFootHintPositionOffsetZ));
                        loadOffsets[(int)BodyPart.RightFoot] = new OffsetVectors(new Vector3(motion.RightFootPositionOffsetX, motion.RightFootPositionOffsetY, motion.RightFootPositionOffsetZ),
                                                                                 new Vector3(motion.RightFootRotationOffsetP, motion.RightFootRotationOffsetY, motion.RightFootRotationOffsetR),
                                                                                 new Vector3(motion.RightFootHintPositionOffsetX, motion.RightFootHintPositionOffsetY, motion.RightFootHintPositionOffsetZ));

                        bool[] jointCorrections = new bool[(int)BodyPart.BodyPartsCount];
                        jointCorrections[(int)BodyPart.LeftHand] = motion.LeftHandJointCorrection;
                        jointCorrections[(int)BodyPart.RightHand] = motion.RightHandJointCorrection;
                        jointCorrections[(int)BodyPart.LeftFoot] = motion.LeftFootJointCorrection;
                        jointCorrections[(int)BodyPart.RightFoot] = motion.RightFootJointCorrection;

                        SliderUI.LoadOffsets(charIndex, loadOffsets, jointCorrections, character.ShoeOffset);

                        bValidOffsetsFound = true;
                    }

                    SliderUI.ApplyPositionsAndCorrections();
                }
            }

            // if we didn't find offsets to apply, move the characters to their 0 position, in case they were moved out of it from another offset.
            if (!bValidOffsetsFound)
                SliderUI.ResetPositions();

            SliderUI.UpdateUIPositions();
        }

        //-- Save the character pair of offsets to the xml file, overwriting if necessary --//
        public static void SaveCharacterGroupOffsets(List<string> characterNames, List<OffsetVectors[]> offsetVectorList, List<bool[]> jointCorrectionList, List<float> shoeOffsets, bool isDefault = false)
        {
            if (characterNames.IsNullOrEmpty() || offsetVectorList.IsNullOrEmpty() || shoeOffsets.IsNullOrEmpty())
                return;

            var currentAnimation = AI_BetterHScenes.hFlagCtrl.nowAnimationInfo.nameAnimation;
            if (currentAnimation == null || AI_BetterHScenes.currentMotion == null)
                return;

            var currentMotion = AI_BetterHScenes.currentMotion;
            if (isDefault)
            {
                if (AI_BetterHScenes.useUniqueOffsetForWeak.Value && AI_BetterHScenes.currentMotion.StartsWith("D_"))
                    currentMotion = "defaultWeak";
                else
                    currentMotion = "default";
            }

            string characterGroupName = null;
            foreach (var name in characterNames)
            {
                if (characterGroupName == null)
                    characterGroupName = name;
                else
                    characterGroupName += "_" + name;
            }

            AI_BetterHScenes.Logger.LogMessage("Saving Offsets for " + currentAnimation + " Motion " + currentMotion + " for characters " + characterGroupName);

            var characterGroup = new CharacterGroupXML(characterGroupName);

            for (var charIndex = 0; charIndex < characterNames.Count; charIndex++)
            {
                var motionOffsets = new MotionOffsetsXML(currentMotion, offsetVectorList[charIndex], jointCorrectionList[charIndex]);
                var animation = new AnimationXML(currentAnimation);
                animation.AddMotionOffsets(motionOffsets);

                var character = new CharacterXML(characterNames[charIndex], shoeOffsets[charIndex]);
                character.AddAnimation(animation);

                characterGroup.AddCharacter(character);
            }

            hSceneOffsets.AddCharacterGroup(characterGroup);
            SaveOffsetsToFile();
        }

        //-- Delete the character pair of offsets in the xml file --//
        public static void DeleteCharacterGroupOffsets(List<string> characterNames)
        {
            if (characterNames.IsNullOrEmpty())
                return;

            var currentAnimation = AI_BetterHScenes.hFlagCtrl.nowAnimationInfo.nameAnimation;
            if (currentAnimation == null)
                return;

            string characterGroupName = null;
            foreach (var name in characterNames)
            {
                if (characterGroupName == null)
                    characterGroupName = name;
                else
                    characterGroupName += "_" + name;
            }

            AI_BetterHScenes.Logger.LogMessage("Deleting Offsets for " + currentAnimation + " for characters " + characterGroupName);

            var characterGroup = new CharacterGroupXML(characterGroupName);

            foreach (var name in characterNames)
                characterGroup.AddCharacter(new CharacterXML(name));

            var animation = new AnimationXML(currentAnimation);
            hSceneOffsets.DeleteCharacterGroupAnimation(characterGroup, animation);
            SaveOffsetsToFile();
        }

        private static void SaveOffsetsToFile()
        {
            if (hSceneOffsets == null)
                return;

            // Create an XML serializer so we can store the offset configuration in an XML file
            var serializer = new XmlSerializer(typeof(BetterHScenesOffsetsXML));

            // Create a new file stream in which the offset will be stored
            StreamWriter OffsetFile;
            try
            {
                // Store the setup data
                string fileName = AI_BetterHScenes.offsetFileV2.Value;
                OffsetFile = new StreamWriter(fileName);
                serializer.Serialize(OffsetFile, hSceneOffsets);
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
            ConvertLegacyFile();

            // Create an XML serializer so we can read the offset configuration in an XML file
            var serializer = new XmlSerializer(typeof(BetterHScenesOffsetsXML));
            hSceneOffsets = new BetterHScenesOffsetsXML();

            Stream OffsetFile;
            try
            {
                // Read in the data
                string fileName = AI_BetterHScenes.offsetFileV2.Value;

                OffsetFile = new FileStream(fileName, FileMode.Open);
                hSceneOffsets = (BetterHScenesOffsetsXML)serializer.Deserialize(OffsetFile);
            }
            catch
            {
                //AI_BetterHScenes.Logger.LogMessage("read error!");
                return;
            }

            // Close the file
            OffsetFile.Close();
        }

        private static void ConvertLegacyFile()
        {          
            hSceneOffsets = new BetterHScenesOffsetsXML();

            Stream OffsetFile;
            try
            {
                // Read in the data
                string fileName = AI_BetterHScenes.offsetFile.Value;

                OffsetFile = new FileStream(fileName, FileMode.Open);
                var serializer = new XmlSerializer(typeof(AnimationOffsets));
                var animationOffsets = (AnimationOffsets)serializer.Deserialize(OffsetFile);

                foreach(var animation in animationOffsets.Animations)
                {
                    foreach(var motion in animation.MotionList)
                    {
                        foreach (var characterPair in motion.CharacterPairList)
                        {
                            CharacterGroupXML characterGroupXML = new CharacterGroupXML(characterPair.CharacterPairName);
                            foreach (var character in characterPair.CharacterOffsets)
                            {

                                OffsetVectors[] offsetVectors = new OffsetVectors[(int)BodyPart.BodyPartsCount];
                                offsetVectors[(int)BodyPart.WholeBody] = new OffsetVectors(new Vector3(character.PositionOffsetX, character.PositionOffsetY, character.PositionOffsetZ),
                                                                                         new Vector3(character.RotationOffsetP, character.RotationOffsetY, character.RotationOffsetR),
                                                                                         Vector3.zero);
                                offsetVectors[(int)BodyPart.LeftHand] = new OffsetVectors(new Vector3(character.LeftHandPositionOffsetX, character.LeftHandPositionOffsetY, character.LeftHandPositionOffsetZ),
                                                                                        new Vector3(character.LeftHandRotationOffsetP, character.LeftHandRotationOffsetY, character.LeftHandRotationOffsetR),
                                                                                        new Vector3(character.LeftHandHintPositionOffsetX, character.LeftHandHintPositionOffsetY, character.LeftHandHintPositionOffsetZ));
                                offsetVectors[(int)BodyPart.RightHand] = new OffsetVectors(new Vector3(character.RightHandPositionOffsetX, character.RightHandPositionOffsetY, character.RightHandPositionOffsetZ),
                                                                                         new Vector3(character.RightHandRotationOffsetP, character.RightHandRotationOffsetY, character.RightHandRotationOffsetR),
                                                                                         new Vector3(character.RightHandHintPositionOffsetX, character.RightHandHintPositionOffsetY, character.RightHandHintPositionOffsetZ));
                                offsetVectors[(int)BodyPart.LeftFoot] = new OffsetVectors(new Vector3(character.LeftFootPositionOffsetX, character.LeftFootPositionOffsetY, character.LeftFootPositionOffsetZ),
                                                                                        new Vector3(character.LeftFootRotationOffsetP, character.LeftFootRotationOffsetY, character.LeftFootRotationOffsetR),
                                                                                        new Vector3(character.LeftFootHintPositionOffsetX, character.LeftFootHintPositionOffsetY, character.LeftFootHintPositionOffsetZ));
                                offsetVectors[(int)BodyPart.RightFoot] = new OffsetVectors(new Vector3(character.RightFootPositionOffsetX, character.RightFootPositionOffsetY, character.RightFootPositionOffsetZ),
                                                                                         new Vector3(character.RightFootRotationOffsetP, character.RightFootRotationOffsetY, character.RightFootRotationOffsetR),
                                                                                         new Vector3(character.RightFootHintPositionOffsetX, character.RightFootHintPositionOffsetY, character.RightFootHintPositionOffsetZ));

                                bool[] jointCorrections = new bool[(int)BodyPart.BodyPartsCount];

                                MotionOffsetsXML motionOffsetsXML = new MotionOffsetsXML(motion.MotionName, offsetVectors, jointCorrections);
                                AnimationXML animationXML = new AnimationXML(animation.AnimationName);
                                CharacterXML characterXML = new CharacterXML(character.CharacterName, 0);

                                animationXML.AddMotionOffsets(motionOffsetsXML);
                                characterXML.AddAnimation(animationXML);
                                characterGroupXML.AddCharacter(characterXML);
                            }

                            hSceneOffsets.AddCharacterGroup(characterGroupXML);
                        }
                    }
                }

                SaveOffsetsToFile();

                // Close the file
                OffsetFile.Close();
                File.Delete(fileName);
            }
            catch
            {
                //HS2_BetterHScenes.Logger.LogMessage("read error!");
                return;
            }
        }
    }
}
