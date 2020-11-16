using System;
using System.Linq;
using UnityEngine;
using AIChara;

namespace AI_BetterHScenes
{
    internal enum BodyPart
    {
        WholeBody,
        LeftHand,
        RightHand,
        LeftFoot,
        RightFoot,
        BodyPartsCount
    }

    public class OffsetVectors
    {
        public Vector3 position = new Vector3(0, 0, 0);
        public Vector3 rotation = new Vector3(0, 0, 0);
        public Vector3 hintPosition = new Vector3(0, 0, 0);

        public OffsetVectors()
        {
            position = new Vector3(0, 0, 0);
            rotation = new Vector3(0, 0, 0);
            hintPosition = new Vector3(0, 0, 0);
        }

        public OffsetVectors(Vector3 initPosition, Vector3 initRotation, Vector3 initHintPosition)
        {
            position = initPosition;
            rotation = initRotation;
            hintPosition = initHintPosition;
        }
    }

    public class CharacterOffsetLocations
    {
        public const string bodyTransformName = "cf_N_height";
        public const string leftHandTransformName = "f_t_arm_L";
        public const string rightHandTransformName = "f_t_arm_R";
        public const string leftFootTransformName = "f_t_leg_L";
        public const string rightFootTransform = "f_t_leg_R";
        public const string leftElbowTransformName = "f_t_elbo_L";
        public const string rightElbowTransformName = "f_t_elbo_R";
        public const string leftKneeTransformName = "f_t_knee_L";
        public const string rightKneeTransform = "f_t_knee_R";

        public static readonly string[] offsetTransformNames = { bodyTransformName, leftHandTransformName, rightHandTransformName, leftFootTransformName, rightFootTransform };
        public static readonly string[] hintTransformNames = { bodyTransformName, leftElbowTransformName, rightElbowTransformName, leftKneeTransformName, rightKneeTransform };
        public Transform[] offsetTransforms = new Transform[offsetTransformNames.Length];
        public Transform[] hintTransforms = new Transform[hintTransformNames.Length];
        public OffsetVectors[] offsetVectors = new OffsetVectors[offsetTransformNames.Length];
        public bool allLimbsFound;

        public int LeftHand { get; private set; }

        public CharacterOffsetLocations()
        {
            offsetVectors = new OffsetVectors[offsetTransformNames.Length];
            offsetTransforms = new Transform[offsetTransformNames.Length];
            hintTransforms = new Transform[hintTransformNames.Length];

            for (var offset = 0; offset < offsetVectors.Length; offset++)
                offsetVectors[offset] = new OffsetVectors();

            allLimbsFound = false;
        }

        public void LoadCharacterTransforms(ChaControl character)
        {
            allLimbsFound = false;

            for (var offset = 0; offset < offsetTransforms.Length; offset++)
            {
                offsetTransforms[offset] = character.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(offsetTransformNames[offset])).FirstOrDefault();
                if (offsetTransforms[offset] == null)
                    return;
            }

            for (var offset = 0; offset < hintTransformNames.Length; offset++)
            {
                hintTransforms[offset] = character.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(hintTransformNames[offset])).FirstOrDefault();
                if (hintTransforms[offset] == null)
                    return;
            }
			
            allLimbsFound = true;
        }

        public void ApplyLimbOffsets()
        {
            if (!allLimbsFound)
                return;

            for (int offset = (int)BodyPart.LeftHand; offset < offsetTransforms.Length; offset++)
            {           
                if (offset == (int)BodyPart.WholeBody || offsetTransforms[offset] == null)
                    continue;

                if (offsetVectors[offset].position != new Vector3(0, 0, 0))
                    offsetTransforms[offset].localPosition += offsetVectors[offset].position;

                if (offsetVectors[offset].rotation != new Vector3(0, 0, 0))
                    offsetTransforms[offset].eulerAngles += offsetVectors[offset].rotation;

                if (offsetVectors[offset].hintPosition != new Vector3(0, 0, 0))
                    hintTransforms[offset].localPosition += offsetVectors[offset].hintPosition;
            }
        }
    }
}
