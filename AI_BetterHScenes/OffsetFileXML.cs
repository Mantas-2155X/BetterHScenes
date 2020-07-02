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

        public bool ShouldSerializePositionOffsetX() => PositionOffsetX != 0.0;
        public bool ShouldSerializePositionOffsetY() => PositionOffsetY != 0.0;
        public bool ShouldSerializePositionOffsetZ() => PositionOffsetZ != 0.0;
        public bool ShouldSerializeRotationOffsetP() => RotationOffsetP != 0.0;
        public bool ShouldSerializeRotationOffsetY() => RotationOffsetY != 0.0;
        public bool ShouldSerializeRotationOffsetR() => RotationOffsetR != 0.0;

        public CharacterOffsets() { }

        public CharacterOffsets(string _characterName, Vector3 _positionOffset, Vector3 _rotationOffset)
        {
            CharacterName = _characterName;
            PositionOffsetX = _positionOffset.x;
            PositionOffsetY = _positionOffset.y;
            PositionOffsetZ = _positionOffset.z;
            RotationOffsetP = _rotationOffset.x;
            RotationOffsetY = _rotationOffset.y;
            RotationOffsetR = _rotationOffset.z;
        }
    }
}
