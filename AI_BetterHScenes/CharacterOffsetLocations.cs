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
        public Vector3 position = Vector3.zero;
        public Vector3 rotation = Vector3.zero;
        public Vector3 hintPosition = Vector3.zero;

        public OffsetVectors()
        {
            position = Vector3.zero;
            rotation = Vector3.zero;
            hintPosition = Vector3.zero;
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
        public const string rightFootTransformName = "f_t_leg_R";
        public const string leftElbowTransformName = "f_t_elbo_L";
        public const string rightElbowTransformName = "f_t_elbo_R";
        public const string leftKneeTransformName = "f_t_knee_L";
        public const string rightKneeTransformName = "f_t_knee_R";

        public static readonly string[] offsetTransformNames = { bodyTransformName, leftHandTransformName, rightHandTransformName, leftFootTransformName, rightFootTransformName };
        public static readonly string[] hintTransformNames = { bodyTransformName, leftElbowTransformName, rightElbowTransformName, leftKneeTransformName, rightKneeTransformName };
        public Transform[] offsetTransforms = new Transform[offsetTransformNames.Length];
        public Transform[] hintTransforms = new Transform[hintTransformNames.Length];
        public Transform[] baseReplaceTransforms = new Transform[offsetTransformNames.Length];
        public Correct.BaseData[] baseData = new Correct.BaseData[offsetTransformNames.Length];
        public OffsetVectors[] offsetVectors = new OffsetVectors[offsetTransformNames.Length];
        public Vector3[] lastBasePosition = new Vector3[offsetTransformNames.Length];
        public Vector3[] lastBaseRotation = new Vector3[offsetTransformNames.Length];
        public bool allLimbsFound;
        public bool dependentAnimation = false;

        public int LeftHand { get; private set; }

        public CharacterOffsetLocations()
        {
            offsetVectors = new OffsetVectors[offsetTransformNames.Length];
            offsetTransforms = new Transform[offsetTransformNames.Length];
            hintTransforms = new Transform[hintTransformNames.Length];
            baseData = new Correct.BaseData[offsetTransformNames.Length];
            lastBasePosition = new Vector3[offsetTransformNames.Length];
            lastBaseRotation = new Vector3[offsetTransformNames.Length];
            baseReplaceTransforms = new Transform[offsetTransformNames.Length];

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

            for (var offset = (int)BodyPart.LeftHand; offset < hintTransformNames.Length; offset++)
            {
                hintTransforms[offset] = character.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(hintTransformNames[offset])).FirstOrDefault();
                if (hintTransforms[offset] == null)
                    return;
            }

            for (var offset = (int)BodyPart.LeftHand; offset < offsetTransforms.Length; offset++)
            {
                baseData[offset] = character.GetComponentsInChildren<Correct.BaseData>().Where(x => x.name.Contains(offsetTransformNames[offset])).FirstOrDefault();
                if (baseData[offset] == null)
                    return;

                if (baseData[offset].bone != null)
                {
                    lastBasePosition[offset] = baseData[offset].bone.position;
                    lastBaseRotation[offset] = baseData[offset].bone.eulerAngles;
                }
                else
                {
                    lastBasePosition[offset] = Vector3.zero;
                    lastBaseRotation[offset] = Vector3.zero;
                }
            }

            allLimbsFound = true;
        }

        public void UpdateDependentStatus(ChaControl character)
        {
            dependentAnimation = false;

            if (!allLimbsFound || !AI_BetterHScenes.solveDependenciesFirst.Value)
                return;

            for (int offset = (int)BodyPart.LeftHand; offset < offsetTransforms.Length; offset++)
            {
                if (baseData[offset].bone == null || baseData[offset].bone.name.Contains("f_pv"))
                    continue;

                ChaControl targetCharacter = baseData[offset].bone.GetComponentInParent<ChaControl>();

                if (targetCharacter != null && character.chaID != targetCharacter.chaID)
                {
                    dependentAnimation = true;
                    return;
                }
            }
        }

        public void ApplyLimbOffsets(bool useLastSolverResult, bool useReplacementTransforms, bool bLeftFootJob, bool bRightFootJob)
        {
            if (!allLimbsFound)
                return;

            for (int offset = (int)BodyPart.LeftHand; offset < offsetTransforms.Length; offset++)
            {
                if (AI_BetterHScenes.enableAnimationFixer.Value)
                {
                    if (offset == (int)BodyPart.WholeBody || offsetTransforms[offset] == null || baseData[offset] == null)
                        continue;

                    if (baseData[offset].bone != null)
                    {
                        if (useLastSolverResult)
                            offsetTransforms[offset].position = lastBasePosition[offset];
                        else if (useReplacementTransforms && baseReplaceTransforms[offset] != null && baseData[offset].bone.name.Contains("f_pv"))
                            offsetTransforms[offset].position = baseReplaceTransforms[offset].position;
                        else
                            offsetTransforms[offset].position = baseData[offset].bone.position;
                    }

                    if (baseData[offset].bone != null)
                    {
                        if (useLastSolverResult)
                            offsetTransforms[offset].eulerAngles = lastBaseRotation[offset];
                        else if (useReplacementTransforms && baseReplaceTransforms[offset] != null && baseData[offset].bone.name.Contains("f_pv"))
                            offsetTransforms[offset].eulerAngles = baseReplaceTransforms[offset].eulerAngles;
                        else
                            offsetTransforms[offset].eulerAngles = baseData[offset].bone.eulerAngles;
                    }

                    if (bLeftFootJob && offset == (int)BodyPart.LeftFoot && baseReplaceTransforms[(int)BodyPart.LeftHand] != null && baseReplaceTransforms[offset] != null)
                        offsetTransforms[offset].position += baseReplaceTransforms[(int)BodyPart.LeftHand].position - baseReplaceTransforms[offset].position;

                    if (bRightFootJob && offset == (int)BodyPart.RightFoot && baseReplaceTransforms[(int)BodyPart.RightHand] != null && baseReplaceTransforms[offset] != null)
                        offsetTransforms[offset].position += baseReplaceTransforms[(int)BodyPart.RightHand].position - baseReplaceTransforms[offset].position;
                }

                if (offsetVectors[offset].position != Vector3.zero)
                   offsetTransforms[offset].localPosition += offsetVectors[offset].position;

                if (offsetVectors[offset].rotation != Vector3.zero)
                    offsetTransforms[offset].localEulerAngles += offsetVectors[offset].rotation;

                if (offsetVectors[offset].hintPosition != Vector3.zero)
                    hintTransforms[offset].localPosition += offsetVectors[offset].hintPosition;
            }
        }

        public void SaveBasePoints(bool useReplacementTransforms)
        {
            for (var offset = (int)BodyPart.LeftHand; offset < offsetTransforms.Length; offset++)
            {
                if (baseData[offset] != null && baseData[offset].bone != null)
                {
                    if (useReplacementTransforms && baseReplaceTransforms[offset] != null && baseData[offset].bone.name.Contains("f_pv"))
                    {
                        lastBasePosition[offset] = baseReplaceTransforms[offset].position;
                        lastBaseRotation[offset] = baseReplaceTransforms[offset].eulerAngles;
                    }
                    else
                    {
                        lastBasePosition[offset] = baseData[offset].bone.position;
                        lastBaseRotation[offset] = baseData[offset].bone.eulerAngles;
                    }
                }
                else
                {
                    lastBasePosition[offset] = Vector3.zero;
                    lastBaseRotation[offset] = Vector3.zero;
                }
            }
        }

        public void SetBaseReplacement(int offset, Transform basePoint)
        {
            baseReplaceTransforms[offset] = basePoint;
        }

        public void ClearBaseReplacements()
        {
            baseReplaceTransforms = new Transform[offsetTransformNames.Length];
        }
    }
}
