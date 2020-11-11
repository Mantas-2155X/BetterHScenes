using System;
using System.Xml.Serialization;
using System.Collections.Generic;

using UnityEngine;

namespace AI_BetterHScenes
{
    [XmlRoot("AnimationOffsets")]
    [XmlInclude(typeof(AnimationsList))] // include type class CharacterAnimationOffsets
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

    [XmlType("AnimationsList")] // define Type
    [XmlInclude(typeof(MotionList))]
    public class AnimationsList
    {
        [XmlAttribute("AnimationName", DataType = "string")]

        //[XmlElement("CharacterName")]
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

    [XmlType("MotionList")] // define Type
    [XmlInclude(typeof(CharacterPairList))]
    public class MotionList
    {
        [XmlAttribute("MotionName", DataType = "string")]

        //[XmlElement("CharacterName")]
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


    [XmlType("CharacterPairList")] // define Type
    [XmlInclude(typeof(CharacterOffsets))]
    public class CharacterPairList
    {
        [XmlAttribute("CharacterPairName", DataType = "string")]

        //[XmlElement("CharacterName")]
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
        public bool ShouldSerializeRightHandPositionOffsetX() => RightHandPositionOffsetX != 0.0;
        public bool ShouldSerializeRightHandPositionOffsetY() => RightHandPositionOffsetY != 0.0;
        public bool ShouldSerializeRightHandPositionOffsetZ() => RightHandPositionOffsetZ != 0.0;
        public bool ShouldSerializeRightHandRotationOffsetP() => RightHandRotationOffsetP != 0.0;
        public bool ShouldSerializeRightHandRotationOffsetY() => RightHandRotationOffsetY != 0.0;
        public bool ShouldSerializeRightHandRotationOffsetR() => RightHandRotationOffsetR != 0.0;
        public bool ShouldSerializeLeftFootPositionOffsetX() => LeftFootPositionOffsetX != 0.0;
        public bool ShouldSerializeLeftFootPositionOffsetY() => LeftFootPositionOffsetY != 0.0;
        public bool ShouldSerializeLeftFootPositionOffsetZ() => LeftFootPositionOffsetZ != 0.0;
        public bool ShouldSerializeLeftFootRotationOffsetP() => LeftFootRotationOffsetP != 0.0;
        public bool ShouldSerializeLeftFootRotationOffsetY() => LeftFootRotationOffsetY != 0.0;
        public bool ShouldSerializeLeftFootRotationOffsetR() => LeftFootRotationOffsetR != 0.0;
        public bool ShouldSerializeRightFootPositionOffsetX() => RightFootPositionOffsetX != 0.0;
        public bool ShouldSerializeRightFootPositionOffsetY() => RightFootPositionOffsetY != 0.0;
        public bool ShouldSerializeRightFootPositionOffsetZ() => RightFootPositionOffsetZ != 0.0;
        public bool ShouldSerializeRightFootRotationOffsetP() => RightFootRotationOffsetP != 0.0;
        public bool ShouldSerializeRightFootRotationOffsetY() => RightFootRotationOffsetY != 0.0;
        public bool ShouldSerializeRightFootRotationOffsetR() => RightFootRotationOffsetR != 0.0;

        public CharacterOffsets() { }

        public CharacterOffsets(string _characterName, OffsetVectors[] _offsetVectors)
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
            RightHandPositionOffsetX = _offsetVectors[(int)BodyPart.RightHand].position.x;
            RightHandPositionOffsetY = _offsetVectors[(int)BodyPart.RightHand].position.y;
            RightHandPositionOffsetZ = _offsetVectors[(int)BodyPart.RightHand].position.z;
            RightHandRotationOffsetP = _offsetVectors[(int)BodyPart.RightHand].rotation.x;
            RightHandRotationOffsetY = _offsetVectors[(int)BodyPart.RightHand].rotation.y;
            RightHandRotationOffsetR = _offsetVectors[(int)BodyPart.RightHand].rotation.z;
            LeftFootPositionOffsetX = _offsetVectors[(int)BodyPart.LeftFoot].position.x;
            LeftFootPositionOffsetY = _offsetVectors[(int)BodyPart.LeftFoot].position.y;
            LeftFootPositionOffsetZ = _offsetVectors[(int)BodyPart.LeftFoot].position.z;
            LeftFootRotationOffsetP = _offsetVectors[(int)BodyPart.LeftFoot].rotation.x;
            LeftFootRotationOffsetY = _offsetVectors[(int)BodyPart.LeftFoot].rotation.y;
            LeftFootRotationOffsetR = _offsetVectors[(int)BodyPart.LeftFoot].rotation.z;
            RightFootPositionOffsetX = _offsetVectors[(int)BodyPart.RightFoot].position.x;
            RightFootPositionOffsetY = _offsetVectors[(int)BodyPart.RightFoot].position.y;
            RightFootPositionOffsetZ = _offsetVectors[(int)BodyPart.RightFoot].position.z;
            RightFootRotationOffsetP = _offsetVectors[(int)BodyPart.RightFoot].rotation.x;
            RightFootRotationOffsetY = _offsetVectors[(int)BodyPart.RightFoot].rotation.y;
            RightFootRotationOffsetR = _offsetVectors[(int)BodyPart.RightFoot].rotation.z;
        }
    }
}
