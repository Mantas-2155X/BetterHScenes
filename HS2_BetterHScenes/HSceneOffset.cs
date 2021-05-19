using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using UnityEngine;

namespace HS2_BetterHScenes
{
    public static class HSceneOffset
    {
        public const string GlobalGroupName = "__GLOBAL_GROUP__";
        public const string PCNameFormat = "__PC{0}__";
        public const string NPCNameFormat = "__NPC{0}__";

        private static BetterHScenesOffsetsXML hSceneOffsets;

        //-- Apply character offsets for current animation, if they can be found --//
        public static void ApplyCharacterOffsets(bool isGlobalGroup)
        {
            var currentAnimation = HS2_BetterHScenes.hFlagCtrl.nowAnimationInfo.nameAnimation;
            var bValidOffsetsFound = false;

            if (currentAnimation == null)
                return;

            if (HS2_BetterHScenes.currentMotion == null)
                HS2_BetterHScenes.currentMotion = "default";

            string characterGroupName = null;
            if (isGlobalGroup)
            {
                characterGroupName = GlobalGroupName;
            }
            else
            {
                foreach (var character in HS2_BetterHScenes.characters.Where(character => character != null && character.visibleAll))
                {
                    if (characterGroupName == null)
                        characterGroupName = character.fileParam.fullname;
                    else
                        characterGroupName += "_" + character.fileParam.fullname;
                }
            }

            if (characterGroupName != null)
            {
                var characterGroup = hSceneOffsets.CharacterGroupList.Find(x => x.CharacterGroupName == characterGroupName);
                if (characterGroup != null)
                {
                    // Indexing the character whether PC or Non-PC for global group.
                    // NOTE: Maybe this will not work properly if there are multiple NPCs.
                    int pcIndex = 0, npcIndex = 0;
                    for (var charIndex = 0; charIndex < HS2_BetterHScenes.characters.Count; charIndex++)
                    {
                        AIChara.ChaControl destChar = HS2_BetterHScenes.characters[charIndex];
                        string searchCharName;
                        if (isGlobalGroup)
                        {
                            if (destChar.isPlayer)
                            {
                                searchCharName = string.Format(PCNameFormat, pcIndex++);
                            }
                            else
                            {
                                searchCharName = string.Format(NPCNameFormat, npcIndex++);
                            }
                        }
                        else
                        {
                            searchCharName = destChar.fileParam.fullname;
                        }
                        var character = characterGroup.CharacterList.Find(x => x.CharacterName == searchCharName);
                        if (character == null)
                            continue;

                        var animation = character.AnimationList.Find(x => x.AnimationName == currentAnimation);
                        if (animation == null)
                            continue;

                        var motion = animation.MotionOffsetList.Find(x => x.MotionName == HS2_BetterHScenes.currentMotion);
                        if (HS2_BetterHScenes.useOneOffsetForAllMotions.Value || motion == null || motion.MotionName != HS2_BetterHScenes.currentMotion)
                            motion = animation.MotionOffsetList.Find(x => x.MotionName == "default");

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
        public static void SaveCharacterGroupOffsets(List<AIChara.ChaControl> destChars, List<OffsetVectors[]> offsetVectorList, List<bool[]> jointCorrectionList, List<float> shoeOffsets, bool isGlobalGroup, bool isDefaultMotion)
        {
            if (destChars.IsNullOrEmpty() || offsetVectorList.IsNullOrEmpty() || shoeOffsets.IsNullOrEmpty())
                return;

            var currentAnimation = HS2_BetterHScenes.hFlagCtrl.nowAnimationInfo.nameAnimation;
            if (currentAnimation == null || HS2_BetterHScenes.currentMotion == null)
                return;

            var currentMotion = HS2_BetterHScenes.currentMotion;
            if (isDefaultMotion)
                currentMotion = "default";

            string characterGroupName = null;
            if (isGlobalGroup)
            {
                characterGroupName = GlobalGroupName;
            }
            else
            {
                foreach (var name in destChars.Select(c => c.fileParam.fullname))
                {
                    if (characterGroupName == null)
                        characterGroupName = name;
                    else
                        characterGroupName += "_" + name;
                }
            }

            HS2_BetterHScenes.Logger.LogMessage("Saving Offsets for " + currentAnimation + " Motion " + currentMotion + " for characters " + characterGroupName);

            var characterGroup = new CharacterGroupXML(characterGroupName);

            // See 'ApplyCharacterOffsets()'
            int pcIndex = 0, npcIndex = 0;
            for (var charIndex = 0; charIndex < destChars.Count; charIndex++)
            {
                var motionOffsets = new MotionOffsetsXML(currentMotion, offsetVectorList[charIndex], jointCorrectionList[charIndex]);
                var animation = new AnimationXML(currentAnimation);
                animation.AddMotionOffsets(motionOffsets);

                AIChara.ChaControl destChar = destChars[charIndex];
                string charName;
                if (isGlobalGroup)
                {
                    if (destChar.isPlayer)
                    {
                        charName = string.Format(PCNameFormat, pcIndex++);
                    }
                    else
                    {
                        charName = string.Format(NPCNameFormat, npcIndex++);
                    }
                }
                else
                {
                    charName = destChar.fileParam.fullname;
                }
                var character = new CharacterXML(charName, shoeOffsets[charIndex]);
                character.AddAnimation(animation);

                characterGroup.AddCharacter(character);
            }

            hSceneOffsets.AddCharacterGroup(characterGroup);
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
                string fileName = HS2_BetterHScenes.offsetFileV2.Value;
                OffsetFile = new StreamWriter(fileName);
                serializer.Serialize(OffsetFile, hSceneOffsets);
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
            ConvertLegacyFile();

            // Create an XML serializer so we can read the offset configuration in an XML file
            var serializer = new XmlSerializer(typeof(BetterHScenesOffsetsXML));
            hSceneOffsets = new BetterHScenesOffsetsXML();

            Stream OffsetFile;
            try
            {
                // Read in the data
                string fileName = HS2_BetterHScenes.offsetFileV2.Value;

                OffsetFile = new FileStream(fileName, FileMode.Open);
                hSceneOffsets = (BetterHScenesOffsetsXML)serializer.Deserialize(OffsetFile);
            }
            catch
            {
                //HS2_BetterHScenes.Logger.LogMessage("read error!");
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
                string fileName = HS2_BetterHScenes.offsetFile.Value;

                OffsetFile = new FileStream(fileName, FileMode.Open);
                var serializer = new XmlSerializer(typeof(AnimationOffsets));
                var animationOffsets = (AnimationOffsets)serializer.Deserialize(OffsetFile);

                foreach (var animation in animationOffsets.Animations)
                {
                    foreach (var motion in animation.MotionList)
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
