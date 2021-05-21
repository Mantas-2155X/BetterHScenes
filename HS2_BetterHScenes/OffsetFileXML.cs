using System.Xml.Serialization;
using System.Collections.Generic;

namespace HS2_BetterHScenes
{
    [XmlRoot("BetterHScenesOffsetsXML")]
    [XmlInclude(typeof(CharacterGroupXML))]
    public class BetterHScenesOffsetsXML
    {
        [XmlArray("CharacterGroups")]
        public List<CharacterGroupXML> CharacterGroupList = new List<CharacterGroupXML>();

        public void AddCharacterGroup(CharacterGroupXML characterGroup)
        {
            var existingCharacterGroup = CharacterGroupList.Find(x => x.CharacterGroupName == characterGroup.CharacterGroupName);
            if (existingCharacterGroup != null)
            {
                foreach (var character in characterGroup.CharacterList)
                    existingCharacterGroup.AddCharacter(character);
            }
            else
            {
                CharacterGroupList.Add(characterGroup);
            }
        }
    }

    [XmlType("CharacterGroupXML")] // define Type
    [XmlInclude(typeof(CharacterXML))]
    public class CharacterGroupXML
    {
        [XmlAttribute("CharacterGroupName", DataType = "string")]
        public string CharacterGroupName { get; set; }

        [XmlArray("Characters")]
        public List<CharacterXML> CharacterList = new List<CharacterXML>();

        public CharacterGroupXML() { }

        public CharacterGroupXML(string characterGroupName)
        {
            CharacterGroupName = characterGroupName;
        }

        public void AddCharacter(CharacterXML character)
        {
            var existingCharacter = CharacterList.Find(x => x.CharacterName == character.CharacterName);
            if (existingCharacter != null)
            {
                existingCharacter.ShoeOffset = character.ShoeOffset;

                foreach (var animation in character.AnimationList)
                    existingCharacter.AddAnimation(animation);
            }
            else
            {
                CharacterList.Add(character);
            }
        }
    }

    [XmlType("CharacterXML")]
    [XmlInclude(typeof(AnimationXML))]
    public class CharacterXML
    {
        [XmlAttribute("CharacterName", DataType = "string")]
        public string CharacterName { get; set; }

        [XmlElement("ShoeOffset")]
        public float ShoeOffset { get; set; }

        [XmlArray("Animations")]
        public List<AnimationXML> AnimationList = new List<AnimationXML>();

        public bool ShouldSerializeShoeOffset() => ShoeOffset != 0.0;

        public CharacterXML() { }

        public CharacterXML(string name, float shoeOffset = 0)
        {
            CharacterName = name;
            ShoeOffset = shoeOffset;
        }

        public void AddAnimation(AnimationXML animation)
        {
            var existingAnimation = AnimationList.Find(x => x.AnimationName == animation.AnimationName);
            if (existingAnimation != null)
            {
                foreach (var motionOffset in animation.MotionOffsetList)
                    existingAnimation.AddMotionOffsets(motionOffset);
            }
            else
            {
                AnimationList.Add(animation);
            }
        }
    }

    [XmlType("AnimationXML")] // define Type
    [XmlInclude(typeof(MotionOffsetsXML))]
    public class AnimationXML
    {
        [XmlAttribute("AnimationName", DataType = "string")]

        public string AnimationName { get; set; }

        [XmlArray("Motions")]
        public List<MotionOffsetsXML> MotionOffsetList = new List<MotionOffsetsXML>();

        public AnimationXML() { }

        public AnimationXML(string animationName)
        {
            AnimationName = animationName;
        }

        public void AddMotionOffsets(MotionOffsetsXML motionOffsets)
        {
            var existingMotionOffsets = MotionOffsetList.Find(x => x.MotionName == motionOffsets.MotionName);
            if (existingMotionOffsets != null)
                MotionOffsetList.Remove(existingMotionOffsets);

            MotionOffsetList.Add(motionOffsets);
        }
    }

    [XmlType("MotionOffsetsXML")] // define Type
    public class MotionOffsetsXML
    {
        [XmlAttribute("MotionName", DataType = "string")]
        public string MotionName { get; set; }

        [XmlElement("PositionOffsetX")]
        public float PositionOffsetX { get; set; }

        [XmlElement("PositionOffsetY")]
        public float PositionOffsetY { get; set; }

        [XmlElement("PositionOffsetZ")]
        public float PositionOffsetZ { get; set; }

        [XmlElement("RotationOffsetP")]
        public float RotationOffsetP { get; set; }

        [XmlElement("RotationOffsetY")]
        public float RotationOffsetY { get; set; }

        [XmlElement("RotationOffsetR")]
        public float RotationOffsetR { get; set; }

        [XmlElement("LeftHandPositionOffsetX")]
        public float LeftHandPositionOffsetX { get; set; }

        [XmlElement("LeftHandPositionOffsetY")]
        public float LeftHandPositionOffsetY { get; set; }

        [XmlElement("LeftHandPositionOffsetZ")]
        public float LeftHandPositionOffsetZ { get; set; }

        [XmlElement("LeftHandRotationOffsetP")]
        public float LeftHandRotationOffsetP { get; set; }

        [XmlElement("LeftHandRotationOffsetY")]
        public float LeftHandRotationOffsetY { get; set; }

        [XmlElement("LeftHandRotationOffsetR")]
        public float LeftHandRotationOffsetR { get; set; }

        [XmlElement("LeftHandHintPositionOffsetX")]
        public float LeftHandHintPositionOffsetX { get; set; }

        [XmlElement("LeftHandHintPositionOffsetY")]
        public float LeftHandHintPositionOffsetY { get; set; }

        [XmlElement("LeftHandHintPositionOffsetZ")]
        public float LeftHandHintPositionOffsetZ { get; set; }

        [XmlElement("RighttHandPositionOffsetX")]
        public float RightHandPositionOffsetX { get; set; }

        [XmlElement("RightHandPositionOffsetY")]
        public float RightHandPositionOffsetY { get; set; }

        [XmlElement("RightHandPositionOffsetZ")]
        public float RightHandPositionOffsetZ { get; set; }

        [XmlElement("RightHandRotationOffsetP")]
        public float RightHandRotationOffsetP { get; set; }

        [XmlElement("RightHandRotationOffsetY")]
        public float RightHandRotationOffsetY { get; set; }

        [XmlElement("RightHandRotationOffsetR")]
        public float RightHandRotationOffsetR { get; set; }

        [XmlElement("RightHandHintPositionOffsetX")]
        public float RightHandHintPositionOffsetX { get; set; }

        [XmlElement("RightHandHintPositionOffsetY")]
        public float RightHandHintPositionOffsetY { get; set; }

        [XmlElement("RightHandHintPositionOffsetZ")]
        public float RightHandHintPositionOffsetZ { get; set; }

        [XmlElement("LeftFootPositionOffsetX")]
        public float LeftFootPositionOffsetX { get; set; }

        [XmlElement("LeftFootPositionOffsetY")]
        public float LeftFootPositionOffsetY { get; set; }

        [XmlElement("LeftFootPositionOffsetZ")]
        public float LeftFootPositionOffsetZ { get; set; }

        [XmlElement("LeftFootRotationOffsetP")]
        public float LeftFootRotationOffsetP { get; set; }

        [XmlElement("LeftFootRotationOffsetY")]
        public float LeftFootRotationOffsetY { get; set; }

        [XmlElement("LeftFootRotationOffsetR")]
        public float LeftFootRotationOffsetR { get; set; }

        [XmlElement("LeftFootHintPositionOffsetX")]
        public float LeftFootHintPositionOffsetX { get; set; }

        [XmlElement("LeftFootHintPositionOffsetY")]
        public float LeftFootHintPositionOffsetY { get; set; }

        [XmlElement("LeftFootHintPositionOffsetZ")]
        public float LeftFootHintPositionOffsetZ { get; set; }

        [XmlElement("RightFootPositionOffsetX")]
        public float RightFootPositionOffsetX { get; set; }

        [XmlElement("RightFootPositionOffsetY")]
        public float RightFootPositionOffsetY { get; set; }

        [XmlElement("RightFootPositionOffsetZ")]
        public float RightFootPositionOffsetZ { get; set; }

        [XmlElement("RightFootRotationOffsetP")]
        public float RightFootRotationOffsetP { get; set; }

        [XmlElement("RightFootRotationOffsetY")]
        public float RightFootRotationOffsetY { get; set; }

        [XmlElement("RightFootRotationOffsetR")]
        public float RightFootRotationOffsetR { get; set; }

        [XmlElement("RightFootHintPositionOffsetX")]
        public float RightFootHintPositionOffsetX { get; set; }

        [XmlElement("RightFootHintPositionOffsetY")]
        public float RightFootHintPositionOffsetY { get; set; }

        [XmlElement("RightFootHintPositionOffsetZ")]
        public float RightFootHintPositionOffsetZ { get; set; }

        [XmlElement("LeftHandJointCorrection")]
        public bool LeftHandJointCorrection { get; set; }

        [XmlElement("RightHandJointCorrection")]
        public bool RightHandJointCorrection { get; set; }

        [XmlElement("LeftFootJointCorrection")]
        public bool LeftFootJointCorrection { get; set; }

        [XmlElement("RightFootJointCorrection")]
        public bool RightFootJointCorrection { get; set; }


        public bool ShouldSerializePositionOffsetX() => PositionOffsetX != 0.0;
        public bool ShouldSerializePositionOffsetY() => PositionOffsetY != 0.0;
        public bool ShouldSerializePositionOffsetZ() => PositionOffsetZ != 0.0;
        public bool ShouldSerializeRotationOffsetP() => RotationOffsetP != 0.0;
        public bool ShouldSerializeRotationOffsetY() => RotationOffsetY != 0.0;
        public bool ShouldSerializeRotationOffsetR() => RotationOffsetR != 0.0;

        public bool ShouldSerializeLeftHandPositionOffsetX() => LeftHandPositionOffsetX != 0.0;
        public bool ShouldSerializeLeftHandPositionOffsetY() => LeftHandPositionOffsetY != 0.0;
        public bool ShouldSerializeLeftHandPositionOffsetZ() => LeftHandPositionOffsetZ != 0.0;
        public bool ShouldSerializeLeftHandRotationOffsetP() => LeftHandRotationOffsetP != 0.0;
        public bool ShouldSerializeLeftHandRotationOffsetY() => LeftHandRotationOffsetY != 0.0;
        public bool ShouldSerializeLeftHandRotationOffsetR() => LeftHandRotationOffsetR != 0.0;
        public bool ShouldSerializeLeftHandHintPositionOffsetX() => LeftHandHintPositionOffsetX != 0.0;
        public bool ShouldSerializeLeftHandHintPositionOffsetY() => LeftHandHintPositionOffsetY != 0.0;
        public bool ShouldSerializeLeftHandHintPositionOffsetZ() => LeftHandHintPositionOffsetZ != 0.0;
        public bool ShouldSerializeRightHandPositionOffsetX() => RightHandPositionOffsetX != 0.0;
        public bool ShouldSerializeRightHandPositionOffsetY() => RightHandPositionOffsetY != 0.0;
        public bool ShouldSerializeRightHandPositionOffsetZ() => RightHandPositionOffsetZ != 0.0;
        public bool ShouldSerializeRightHandRotationOffsetP() => RightHandRotationOffsetP != 0.0;
        public bool ShouldSerializeRightHandRotationOffsetY() => RightHandRotationOffsetY != 0.0;
        public bool ShouldSerializeRightHandRotationOffsetR() => RightHandRotationOffsetR != 0.0;
        public bool ShouldSerializeRightHandHintPositionOffsetX() => RightHandHintPositionOffsetX != 0.0;
        public bool ShouldSerializeRightHandHintPositionOffsetY() => RightHandHintPositionOffsetY != 0.0;
        public bool ShouldSerializeRightHandHintPositionOffsetZ() => RightHandHintPositionOffsetZ != 0.0;
        public bool ShouldSerializeLeftFootPositionOffsetX() => LeftFootPositionOffsetX != 0.0;
        public bool ShouldSerializeLeftFootPositionOffsetY() => LeftFootPositionOffsetY != 0.0;
        public bool ShouldSerializeLeftFootPositionOffsetZ() => LeftFootPositionOffsetZ != 0.0;
        public bool ShouldSerializeLeftFootRotationOffsetP() => LeftFootRotationOffsetP != 0.0;
        public bool ShouldSerializeLeftFootRotationOffsetY() => LeftFootRotationOffsetY != 0.0;
        public bool ShouldSerializeLeftFootRotationOffsetR() => LeftFootRotationOffsetR != 0.0;
        public bool ShouldSerializeLeftFootHintPositionOffsetX() => LeftFootHintPositionOffsetX != 0.0;
        public bool ShouldSerializeLeftFootHintPositionOffsetY() => LeftFootHintPositionOffsetY != 0.0;
        public bool ShouldSerializeLeftFootHintPositionOffsetZ() => LeftFootHintPositionOffsetZ != 0.0;
        public bool ShouldSerializeRightFootPositionOffsetX() => RightFootPositionOffsetX != 0.0;
        public bool ShouldSerializeRightFootPositionOffsetY() => RightFootPositionOffsetY != 0.0;
        public bool ShouldSerializeRightFootPositionOffsetZ() => RightFootPositionOffsetZ != 0.0;
        public bool ShouldSerializeRightFootRotationOffsetP() => RightFootRotationOffsetP != 0.0;
        public bool ShouldSerializeRightFootRotationOffsetY() => RightFootRotationOffsetY != 0.0;
        public bool ShouldSerializeRightFootRotationOffsetR() => RightFootRotationOffsetR != 0.0;
        public bool ShouldSerializeRightFootHintPositionOffsetX() => RightFootHintPositionOffsetX != 0.0;
        public bool ShouldSerializeRightFootHintPositionOffsetY() => RightFootHintPositionOffsetY != 0.0;
        public bool ShouldSerializeRightFootHintPositionOffsetZ() => RightFootHintPositionOffsetZ != 0.0;

        public bool ShouldSerializeLeftHandJointCorrection() => LeftHandJointCorrection != HS2_BetterHScenes.defaultJointCorrection.Value;
        public bool ShouldSerializeRightHandJointCorrection() => RightHandJointCorrection != HS2_BetterHScenes.defaultJointCorrection.Value;
        public bool ShouldSerializeLeftFootJointCorrection() => LeftFootJointCorrection != HS2_BetterHScenes.defaultJointCorrection.Value;
        public bool ShouldSerializeRightFootJointCorrection() => RightFootJointCorrection != HS2_BetterHScenes.defaultJointCorrection.Value;

        public MotionOffsetsXML() { }

        public MotionOffsetsXML(string motionName, OffsetVectors[] offsetVectors, bool[] jointCorrections)
        {
            MotionName = motionName;

            if (offsetVectors.IsNullOrEmpty())
                return;

            PositionOffsetX = offsetVectors[(int)BodyPart.WholeBody].position.x;
            PositionOffsetY = offsetVectors[(int)BodyPart.WholeBody].position.y;
            PositionOffsetZ = offsetVectors[(int)BodyPart.WholeBody].position.z;
            RotationOffsetP = offsetVectors[(int)BodyPart.WholeBody].rotation.x;
            RotationOffsetY = offsetVectors[(int)BodyPart.WholeBody].rotation.y;
            RotationOffsetR = offsetVectors[(int)BodyPart.WholeBody].rotation.z;

            if (offsetVectors.Length < (int)BodyPart.BodyPartsCount)
                return;

            LeftHandPositionOffsetX = offsetVectors[(int)BodyPart.LeftHand].position.x;
            LeftHandPositionOffsetY = offsetVectors[(int)BodyPart.LeftHand].position.y;
            LeftHandPositionOffsetZ = offsetVectors[(int)BodyPart.LeftHand].position.z;
            LeftHandRotationOffsetP = offsetVectors[(int)BodyPart.LeftHand].rotation.x;
            LeftHandRotationOffsetY = offsetVectors[(int)BodyPart.LeftHand].rotation.y;
            LeftHandRotationOffsetR = offsetVectors[(int)BodyPart.LeftHand].rotation.z;
            LeftHandHintPositionOffsetX = offsetVectors[(int)BodyPart.LeftHand].hintPosition.x;
            LeftHandHintPositionOffsetY = offsetVectors[(int)BodyPart.LeftHand].hintPosition.y;
            LeftHandHintPositionOffsetZ = offsetVectors[(int)BodyPart.LeftHand].hintPosition.z;
            RightHandPositionOffsetX = offsetVectors[(int)BodyPart.RightHand].position.x;
            RightHandPositionOffsetY = offsetVectors[(int)BodyPart.RightHand].position.y;
            RightHandPositionOffsetZ = offsetVectors[(int)BodyPart.RightHand].position.z;
            RightHandRotationOffsetP = offsetVectors[(int)BodyPart.RightHand].rotation.x;
            RightHandRotationOffsetY = offsetVectors[(int)BodyPart.RightHand].rotation.y;
            RightHandRotationOffsetR = offsetVectors[(int)BodyPart.RightHand].rotation.z;
            RightHandHintPositionOffsetX = offsetVectors[(int)BodyPart.RightHand].hintPosition.x;
            RightHandHintPositionOffsetY = offsetVectors[(int)BodyPart.RightHand].hintPosition.y;
            RightHandHintPositionOffsetZ = offsetVectors[(int)BodyPart.RightHand].hintPosition.z;
            LeftFootPositionOffsetX = offsetVectors[(int)BodyPart.LeftFoot].position.x;
            LeftFootPositionOffsetY = offsetVectors[(int)BodyPart.LeftFoot].position.y;
            LeftFootPositionOffsetZ = offsetVectors[(int)BodyPart.LeftFoot].position.z;
            LeftFootRotationOffsetP = offsetVectors[(int)BodyPart.LeftFoot].rotation.x;
            LeftFootRotationOffsetY = offsetVectors[(int)BodyPart.LeftFoot].rotation.y;
            LeftFootRotationOffsetR = offsetVectors[(int)BodyPart.LeftFoot].rotation.z;
            LeftFootHintPositionOffsetX = offsetVectors[(int)BodyPart.LeftFoot].hintPosition.x;
            LeftFootHintPositionOffsetY = offsetVectors[(int)BodyPart.LeftFoot].hintPosition.y;
            LeftFootHintPositionOffsetZ = offsetVectors[(int)BodyPart.LeftFoot].hintPosition.z;
            RightFootPositionOffsetX = offsetVectors[(int)BodyPart.RightFoot].position.x;
            RightFootPositionOffsetY = offsetVectors[(int)BodyPart.RightFoot].position.y;
            RightFootPositionOffsetZ = offsetVectors[(int)BodyPart.RightFoot].position.z;
            RightFootRotationOffsetP = offsetVectors[(int)BodyPart.RightFoot].rotation.x;
            RightFootRotationOffsetY = offsetVectors[(int)BodyPart.RightFoot].rotation.y;
            RightFootRotationOffsetR = offsetVectors[(int)BodyPart.RightFoot].rotation.z;
            RightFootHintPositionOffsetX = offsetVectors[(int)BodyPart.RightFoot].hintPosition.x;
            RightFootHintPositionOffsetY = offsetVectors[(int)BodyPart.RightFoot].hintPosition.y;
            RightFootHintPositionOffsetZ = offsetVectors[(int)BodyPart.RightFoot].hintPosition.z;

            LeftHandJointCorrection = jointCorrections[(int)BodyPart.LeftHand];
            RightHandJointCorrection = jointCorrections[(int)BodyPart.RightHand];
            LeftFootJointCorrection = jointCorrections[(int)BodyPart.LeftFoot];
            RightFootJointCorrection = jointCorrections[(int)BodyPart.RightFoot];
        }
    }


    //-- Legacy Version --//
    [XmlRoot("AnimationOffsets")]
    [XmlInclude(typeof(AnimationsList))]
    public class AnimationOffsets
    {
        [XmlArray("Animations")]
        [XmlArrayItem("Animation")]
        public List<AnimationsList> Animations = new List<AnimationsList>();

        public void AddCharacterAnimationsList(AnimationsList characterAnimationList)
        {
            Animations.Add(characterAnimationList);
        }
    }

    [XmlType("AnimationsList")]
    [XmlInclude(typeof(MotionList))]
    public class AnimationsList
    {
        [XmlAttribute("AnimationName", DataType = "string")]
        public string AnimationName { get; set; }

        [XmlArray("Motions")]
        [XmlArrayItem("Motion")]
        public List<MotionList> MotionList = new List<MotionList>();

        public AnimationsList() { }

        public AnimationsList(string name)
        {
            this.AnimationName = name;
        }

        public void AddCharacterPair(MotionList motionList)
        {
            MotionList.Add(motionList);
        }
    }

    [XmlType("MotionList")]
    [XmlInclude(typeof(CharacterPairList))]
    public class MotionList
    {
        [XmlAttribute("MotionName", DataType = "string")]

        public string MotionName { get; set; }

        [XmlArray("CharacterPairs")]
        [XmlArrayItem("CharacterPair")]
        public List<CharacterPairList> CharacterPairList = new List<CharacterPairList>();

        public MotionList() { }

        public MotionList(string name)
        {
            this.MotionName = name;
        }

        public void AddCharacterPair(CharacterPairList charaterPairList)
        {
            CharacterPairList.Add(charaterPairList);
        }
    }


    [XmlType("CharacterPairList")]
    [XmlInclude(typeof(CharacterOffsets))]
    public class CharacterPairList
    {
        [XmlAttribute("CharacterPairName", DataType = "string")]

        public string CharacterPairName { get; set; }

        [XmlArray("Characters")]
        [XmlArrayItem("Character")]
        public List<CharacterOffsets> CharacterOffsets = new List<CharacterOffsets>();

        public CharacterPairList() { }

        public CharacterPairList(string name)
        {
            CharacterPairName = name;
        }

        public void AddCharacterOffset(CharacterOffsets characterOffsets)
        {
            CharacterOffsets.Add(characterOffsets);
        }
    }

    [XmlType("CharacterOffsets")]
    public class CharacterOffsets
    {
        [XmlAttribute("CharacterName", DataType = "string")]
        public string CharacterName { get; set; }

        [XmlElement("PositionOffsetX")]
        public float PositionOffsetX { get; set; }

        [XmlElement("PositionOffsetY")]
        public float PositionOffsetY { get; set; }

        [XmlElement("PositionOffsetZ")]
        public float PositionOffsetZ { get; set; }

        [XmlElement("RotationOffsetP")]
        public float RotationOffsetP { get; set; }

        [XmlElement("RotationOffsetY")]
        public float RotationOffsetY { get; set; }

        [XmlElement("RotationOffsetR")]
        public float RotationOffsetR { get; set; }

        [XmlElement("LeftHandPositionOffsetX")]
        public float LeftHandPositionOffsetX { get; set; }

        [XmlElement("LeftHandPositionOffsetY")]
        public float LeftHandPositionOffsetY { get; set; }

        [XmlElement("LeftHandPositionOffsetZ")]
        public float LeftHandPositionOffsetZ { get; set; }

        [XmlElement("LeftHandRotationOffsetP")]
        public float LeftHandRotationOffsetP { get; set; }

        [XmlElement("LeftHandRotationOffsetY")]
        public float LeftHandRotationOffsetY { get; set; }

        [XmlElement("LeftHandRotationOffsetR")]
        public float LeftHandRotationOffsetR { get; set; }

        [XmlElement("LeftHandHintPositionOffsetX")]
        public float LeftHandHintPositionOffsetX { get; set; }

        [XmlElement("LeftHandHintPositionOffsetY")]
        public float LeftHandHintPositionOffsetY { get; set; }

        [XmlElement("LeftHandHintPositionOffsetZ")]
        public float LeftHandHintPositionOffsetZ { get; set; }

        [XmlElement("RighttHandPositionOffsetX")]
        public float RightHandPositionOffsetX { get; set; }

        [XmlElement("RightHandPositionOffsetY")]
        public float RightHandPositionOffsetY { get; set; }

        [XmlElement("RightHandPositionOffsetZ")]
        public float RightHandPositionOffsetZ { get; set; }

        [XmlElement("RightHandRotationOffsetP")]
        public float RightHandRotationOffsetP { get; set; }

        [XmlElement("RightHandRotationOffsetY")]
        public float RightHandRotationOffsetY { get; set; }

        [XmlElement("RightHandRotationOffsetR")]
        public float RightHandRotationOffsetR { get; set; }

        [XmlElement("RightHandHintPositionOffsetX")]
        public float RightHandHintPositionOffsetX { get; set; }

        [XmlElement("RightHandHintPositionOffsetY")]
        public float RightHandHintPositionOffsetY { get; set; }

        [XmlElement("RightHandHintPositionOffsetZ")]
        public float RightHandHintPositionOffsetZ { get; set; }

        [XmlElement("LeftFootPositionOffsetX")]
        public float LeftFootPositionOffsetX { get; set; }

        [XmlElement("LeftFootPositionOffsetY")]
        public float LeftFootPositionOffsetY { get; set; }

        [XmlElement("LeftFootPositionOffsetZ")]
        public float LeftFootPositionOffsetZ { get; set; }

        [XmlElement("LeftFootRotationOffsetP")]
        public float LeftFootRotationOffsetP { get; set; }

        [XmlElement("LeftFootRotationOffsetY")]
        public float LeftFootRotationOffsetY { get; set; }

        [XmlElement("LeftFootRotationOffsetR")]
        public float LeftFootRotationOffsetR { get; set; }

        [XmlElement("LeftFootHintPositionOffsetX")]
        public float LeftFootHintPositionOffsetX { get; set; }

        [XmlElement("LeftFootHintPositionOffsetY")]
        public float LeftFootHintPositionOffsetY { get; set; }

        [XmlElement("LeftFootHintPositionOffsetZ")]
        public float LeftFootHintPositionOffsetZ { get; set; }

        [XmlElement("RightFootPositionOffsetX")]
        public float RightFootPositionOffsetX { get; set; }

        [XmlElement("RightFootPositionOffsetY")]
        public float RightFootPositionOffsetY { get; set; }

        [XmlElement("RightFootPositionOffsetZ")]
        public float RightFootPositionOffsetZ { get; set; }

        [XmlElement("RightFootRotationOffsetP")]
        public float RightFootRotationOffsetP { get; set; }

        [XmlElement("RightFootRotationOffsetY")]
        public float RightFootRotationOffsetY { get; set; }

        [XmlElement("RightFootRotationOffsetR")]
        public float RightFootRotationOffsetR { get; set; }

        [XmlElement("RightFootHintPositionOffsetX")]
        public float RightFootHintPositionOffsetX { get; set; }

        [XmlElement("RightFootHintPositionOffsetY")]
        public float RightFootHintPositionOffsetY { get; set; }

        [XmlElement("RightFootHintPositionOffsetZ")]
        public float RightFootHintPositionOffsetZ { get; set; }

        [XmlElement("ShoeOffset")]
        public float ShoeOffset { get; set; }

        public bool ShouldSerializePositionOffsetX() => PositionOffsetX != 0.0;
        public bool ShouldSerializePositionOffsetY() => PositionOffsetY != 0.0;
        public bool ShouldSerializePositionOffsetZ() => PositionOffsetZ != 0.0;
        public bool ShouldSerializeRotationOffsetP() => RotationOffsetP != 0.0;
        public bool ShouldSerializeRotationOffsetY() => RotationOffsetY != 0.0;
        public bool ShouldSerializeRotationOffsetR() => RotationOffsetR != 0.0;

        public bool ShouldSerializeLeftHandPositionOffsetX() => LeftHandPositionOffsetX != 0.0;
        public bool ShouldSerializeLeftHandPositionOffsetY() => LeftHandPositionOffsetY != 0.0;
        public bool ShouldSerializeLeftHandPositionOffsetZ() => LeftHandPositionOffsetZ != 0.0;
        public bool ShouldSerializeLeftHandRotationOffsetP() => LeftHandRotationOffsetP != 0.0;
        public bool ShouldSerializeLeftHandRotationOffsetY() => LeftHandRotationOffsetY != 0.0;
        public bool ShouldSerializeLeftHandRotationOffsetR() => LeftHandRotationOffsetR != 0.0;
        public bool ShouldSerializeLeftHandHintPositionOffsetX() => LeftHandHintPositionOffsetX != 0.0;
        public bool ShouldSerializeLeftHandHintPositionOffsetY() => LeftHandHintPositionOffsetY != 0.0;
        public bool ShouldSerializeLeftHandHintPositionOffsetZ() => LeftHandHintPositionOffsetZ != 0.0;
        public bool ShouldSerializeRightHandPositionOffsetX() => RightHandPositionOffsetX != 0.0;
        public bool ShouldSerializeRightHandPositionOffsetY() => RightHandPositionOffsetY != 0.0;
        public bool ShouldSerializeRightHandPositionOffsetZ() => RightHandPositionOffsetZ != 0.0;
        public bool ShouldSerializeRightHandRotationOffsetP() => RightHandRotationOffsetP != 0.0;
        public bool ShouldSerializeRightHandRotationOffsetY() => RightHandRotationOffsetY != 0.0;
        public bool ShouldSerializeRightHandRotationOffsetR() => RightHandRotationOffsetR != 0.0;
        public bool ShouldSerializeRightHandHintPositionOffsetX() => RightHandHintPositionOffsetX != 0.0;
        public bool ShouldSerializeRightHandHintPositionOffsetY() => RightHandHintPositionOffsetY != 0.0;
        public bool ShouldSerializeRightHandHintPositionOffsetZ() => RightHandHintPositionOffsetZ != 0.0;
        public bool ShouldSerializeLeftFootPositionOffsetX() => LeftFootPositionOffsetX != 0.0;
        public bool ShouldSerializeLeftFootPositionOffsetY() => LeftFootPositionOffsetY != 0.0;
        public bool ShouldSerializeLeftFootPositionOffsetZ() => LeftFootPositionOffsetZ != 0.0;
        public bool ShouldSerializeLeftFootRotationOffsetP() => LeftFootRotationOffsetP != 0.0;
        public bool ShouldSerializeLeftFootRotationOffsetY() => LeftFootRotationOffsetY != 0.0;
        public bool ShouldSerializeLeftFootRotationOffsetR() => LeftFootRotationOffsetR != 0.0;
        public bool ShouldSerializeLeftFootHintPositionOffsetX() => LeftFootHintPositionOffsetX != 0.0;
        public bool ShouldSerializeLeftFootHintPositionOffsetY() => LeftFootHintPositionOffsetY != 0.0;
        public bool ShouldSerializeLeftFootHintPositionOffsetZ() => LeftFootHintPositionOffsetZ != 0.0;
        public bool ShouldSerializeRightFootPositionOffsetX() => RightFootPositionOffsetX != 0.0;
        public bool ShouldSerializeRightFootPositionOffsetY() => RightFootPositionOffsetY != 0.0;
        public bool ShouldSerializeRightFootPositionOffsetZ() => RightFootPositionOffsetZ != 0.0;
        public bool ShouldSerializeRightFootRotationOffsetP() => RightFootRotationOffsetP != 0.0;
        public bool ShouldSerializeRightFootRotationOffsetY() => RightFootRotationOffsetY != 0.0;
        public bool ShouldSerializeRightFootRotationOffsetR() => RightFootRotationOffsetR != 0.0;
        public bool ShouldSerializeRightFootHintPositionOffsetX() => RightFootHintPositionOffsetX != 0.0;
        public bool ShouldSerializeRightFootHintPositionOffsetY() => RightFootHintPositionOffsetY != 0.0;
        public bool ShouldSerializeRightFootHintPositionOffsetZ() => RightFootHintPositionOffsetZ != 0.0;

        public bool ShouldSerializeShoeOffset() => ShoeOffset != 0.0;
        public CharacterOffsets() { }

        public CharacterOffsets(string _characterName, OffsetVectors[] _offsetVectors, float _shoeOffset)
        {
            CharacterName = _characterName;

            if (_offsetVectors.IsNullOrEmpty())
                return;

            PositionOffsetX = _offsetVectors[(int)BodyPart.WholeBody].position.x;
            PositionOffsetY = _offsetVectors[(int)BodyPart.WholeBody].position.y;
            PositionOffsetZ = _offsetVectors[(int)BodyPart.WholeBody].position.z;
            RotationOffsetP = _offsetVectors[(int)BodyPart.WholeBody].rotation.x;
            RotationOffsetY = _offsetVectors[(int)BodyPart.WholeBody].rotation.y;
            RotationOffsetR = _offsetVectors[(int)BodyPart.WholeBody].rotation.z;

            if (_offsetVectors.Length < (int)BodyPart.BodyPartsCount)
                return;

            LeftHandPositionOffsetX = _offsetVectors[(int)BodyPart.LeftHand].position.x;
            LeftHandPositionOffsetY = _offsetVectors[(int)BodyPart.LeftHand].position.y;
            LeftHandPositionOffsetZ = _offsetVectors[(int)BodyPart.LeftHand].position.z;
            LeftHandRotationOffsetP = _offsetVectors[(int)BodyPart.LeftHand].rotation.x;
            LeftHandRotationOffsetY = _offsetVectors[(int)BodyPart.LeftHand].rotation.y;
            LeftHandRotationOffsetR = _offsetVectors[(int)BodyPart.LeftHand].rotation.z;
            LeftHandHintPositionOffsetX = _offsetVectors[(int)BodyPart.LeftHand].hintPosition.x;
            LeftHandHintPositionOffsetY = _offsetVectors[(int)BodyPart.LeftHand].hintPosition.y;
            LeftHandHintPositionOffsetZ = _offsetVectors[(int)BodyPart.LeftHand].hintPosition.z;
            RightHandPositionOffsetX = _offsetVectors[(int)BodyPart.RightHand].position.x;
            RightHandPositionOffsetY = _offsetVectors[(int)BodyPart.RightHand].position.y;
            RightHandPositionOffsetZ = _offsetVectors[(int)BodyPart.RightHand].position.z;
            RightHandRotationOffsetP = _offsetVectors[(int)BodyPart.RightHand].rotation.x;
            RightHandRotationOffsetY = _offsetVectors[(int)BodyPart.RightHand].rotation.y;
            RightHandRotationOffsetR = _offsetVectors[(int)BodyPart.RightHand].rotation.z;
            RightHandHintPositionOffsetX = _offsetVectors[(int)BodyPart.RightHand].hintPosition.x;
            RightHandHintPositionOffsetY = _offsetVectors[(int)BodyPart.RightHand].hintPosition.y;
            RightHandHintPositionOffsetZ = _offsetVectors[(int)BodyPart.RightHand].hintPosition.z;
            LeftFootPositionOffsetX = _offsetVectors[(int)BodyPart.LeftFoot].position.x;
            LeftFootPositionOffsetY = _offsetVectors[(int)BodyPart.LeftFoot].position.y;
            LeftFootPositionOffsetZ = _offsetVectors[(int)BodyPart.LeftFoot].position.z;
            LeftFootRotationOffsetP = _offsetVectors[(int)BodyPart.LeftFoot].rotation.x;
            LeftFootRotationOffsetY = _offsetVectors[(int)BodyPart.LeftFoot].rotation.y;
            LeftFootRotationOffsetR = _offsetVectors[(int)BodyPart.LeftFoot].rotation.z;
            LeftFootHintPositionOffsetX = _offsetVectors[(int)BodyPart.LeftFoot].hintPosition.x;
            LeftFootHintPositionOffsetY = _offsetVectors[(int)BodyPart.LeftFoot].hintPosition.y;
            LeftFootHintPositionOffsetZ = _offsetVectors[(int)BodyPart.LeftFoot].hintPosition.z;
            RightFootPositionOffsetX = _offsetVectors[(int)BodyPart.RightFoot].position.x;
            RightFootPositionOffsetY = _offsetVectors[(int)BodyPart.RightFoot].position.y;
            RightFootPositionOffsetZ = _offsetVectors[(int)BodyPart.RightFoot].position.z;
            RightFootRotationOffsetP = _offsetVectors[(int)BodyPart.RightFoot].rotation.x;
            RightFootRotationOffsetY = _offsetVectors[(int)BodyPart.RightFoot].rotation.y;
            RightFootRotationOffsetR = _offsetVectors[(int)BodyPart.RightFoot].rotation.z;
            RightFootHintPositionOffsetX = _offsetVectors[(int)BodyPart.RightFoot].hintPosition.x;
            RightFootHintPositionOffsetY = _offsetVectors[(int)BodyPart.RightFoot].hintPosition.y;
            RightFootHintPositionOffsetZ = _offsetVectors[(int)BodyPart.RightFoot].hintPosition.z;

            ShoeOffset = _shoeOffset;
        }
    }
}