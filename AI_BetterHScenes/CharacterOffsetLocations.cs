using System.Linq;
using UnityEngine;
using AIChara;
using CharaUtils;

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
        public Vector3 hint = Vector3.zero;
        public float digitRotation = 0f;

        public OffsetVectors()
        {
            position = Vector3.zero;
            rotation = Vector3.zero;
            hint = Vector3.zero;
            digitRotation = 0f;
        }

        public OffsetVectors(Vector3 initPosition, Vector3 initRotation, Vector3 initHintPosition, float initDigitAngle)
        {
            position = initPosition;
            rotation = initRotation;
            hint = initHintPosition;
            digitRotation = initDigitAngle;
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
        public const string mouthTransformName = "cf_J_MouthBase_s";
        public const string hipTransformName = "cf_J_Hips";
        public const string neckTransformName = "cf_J_Neck";
        public const string headTransformName = "cf_J_Head";
        public static readonly string[] leftToeTransformName = { "cf_J_Toes01_L" };
        public static readonly string[] rightToeTransformName = { "cf_J_Toes01_R" };
        public static readonly string[] leftFingerTransformNames = { 
            "cf_J_Hand_Index01_L",  "cf_J_Hand_Index02_L",  "cf_J_Hand_Index03_L", 
            "cf_J_Hand_Middle01_L", "cf_J_Hand_Middle02_L", "cf_J_Hand_Middle03_L", 
            "cf_J_Hand_Ring01_L",   "cf_J_Hand_Ring02_L",   "cf_J_Hand_Ring03_L",
            "cf_J_Hand_Little01_L", "cf_J_Hand_Little02_L", "cf_J_Hand_Little03_L" };
        public static readonly string[] rightFingerTransformNames = {
            "cf_J_Hand_Index01_R",  "cf_J_Hand_Index02_R",  "cf_J_Hand_Index03_R",
            "cf_J_Hand_Middle01_R", "cf_J_Hand_Middle02_R", "cf_J_Hand_Middle03_R",
            "cf_J_Hand_Ring01_R",   "cf_J_Hand_Ring02_R",   "cf_J_Hand_Ring03_R",
            "cf_J_Hand_Little01_R", "cf_J_Hand_Little02_R", "cf_J_Hand_Little03_R" };

        public static readonly string[] offsetTransformNames = { bodyTransformName, leftHandTransformName, rightHandTransformName, leftFootTransformName, rightFootTransformName };
        public static readonly string[] hintTransformNames = { null, leftElbowTransformName, rightElbowTransformName, leftKneeTransformName, rightKneeTransformName };
        public static readonly string[] headTransformNames = { neckTransformName, headTransformName };
        public static readonly string[][] digitTransformNames = { null, leftFingerTransformNames, rightFingerTransformNames, leftToeTransformName, rightToeTransformName };
        public Transform[] offsetTransforms = new Transform[offsetTransformNames.Length];
        public Transform[] hintTransforms = new Transform[hintTransformNames.Length];
        public Transform[] headTransforms = new Transform[headTransformNames.Length];
        public Transform[][] digitTransforms = new Transform[digitTransformNames.Length][];
        public Transform[] baseReplaceTransforms = new Transform[offsetTransformNames.Length];
        public Transform mouthTransform;
        public Transform hipTransform;
        public Correct.BaseData[] baseData = new Correct.BaseData[offsetTransformNames.Length];
        public OffsetVectors[] offsetVectors = new OffsetVectors[offsetTransformNames.Length];
        public Vector3[] lastBasePosition = new Vector3[offsetTransformNames.Length];
        public Vector3[] lastBaseRotation = new Vector3[offsetTransformNames.Length];
        public bool allLimbsFound;
        public bool dependentAnimation = false;
        public bool[] jointCorrection;

        public int LeftHand { get; private set; }

        public CharacterOffsetLocations()
        {
            offsetVectors = new OffsetVectors[offsetTransformNames.Length];
            offsetTransforms = new Transform[offsetTransformNames.Length];
            hintTransforms = new Transform[hintTransformNames.Length];
            headTransforms = new Transform[headTransformNames.Length];
            digitTransforms = new Transform[digitTransformNames.Length][];
            baseData = new Correct.BaseData[offsetTransformNames.Length];
            lastBasePosition = new Vector3[offsetTransformNames.Length];
            lastBaseRotation = new Vector3[offsetTransformNames.Length];
            baseReplaceTransforms = new Transform[offsetTransformNames.Length];
            jointCorrection = new bool[(int)BodyPart.BodyPartsCount];

            for (var offset = 0; offset < offsetVectors.Length; offset++)
                offsetVectors[offset] = new OffsetVectors();

            for (var joint = 0; joint < jointCorrection.Length; joint++)
                jointCorrection[joint] = AI_BetterHScenes.defaultJointCorrection.Value;

            for (var offset = (int)BodyPart.LeftHand; offset < digitTransforms.Length; offset++)
                digitTransforms[offset] = new Transform[digitTransformNames[offset].Length];

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

            for (var offset = 0; offset < headTransformNames.Length; offset++)
            {
                headTransforms[offset] = character.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(headTransformNames[offset])).FirstOrDefault();
                if (headTransforms[offset] == null)
                    return;
            }

            for (var offset = (int)BodyPart.LeftHand; offset < hintTransformNames.Length; offset++)
            {
                hintTransforms[offset] = character.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(hintTransformNames[offset])).FirstOrDefault();
                if (hintTransforms[offset] == null)
                    return;
            }

            for (var offset = (int)BodyPart.LeftHand; offset < digitTransformNames.Length; offset++)
            {
                digitTransforms[offset] = new Transform[digitTransformNames[offset].Length];
                for (var digit = 0; digit < digitTransformNames[offset].Length; digit++)
                {
                    digitTransforms[offset][digit] = character.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(digitTransformNames[offset][digit])).FirstOrDefault();
                    if (digitTransforms[offset][digit] == null)
                        return;
                }
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

            mouthTransform = character.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(mouthTransformName)).FirstOrDefault();
            hipTransform = character.GetComponentsInChildren<Transform>().Where(x => x.name.Contains(hipTransformName)).FirstOrDefault();
            allLimbsFound = true;
        }

        public void UpdateDependentStatus(ChaControl character)
        {
            dependentAnimation = false;

            if (!allLimbsFound || !AI_BetterHScenes.solveDependenciesFirst.Value || character == null)
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

        public void ApplyLimbOffsets(bool useLastSolverResult, bool useReplacementTransforms, bool bLeftFootJob, bool bRightFootJob, bool shoeOffset, float shoeOffsetAmount)
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
                        {
                            offsetTransforms[offset].position = lastBasePosition[offset];
                            offsetTransforms[offset].eulerAngles = lastBaseRotation[offset];
                        }
                        else if (useReplacementTransforms && baseReplaceTransforms[offset] != null && baseData[offset].bone.name.Contains("f_pv"))
                        {
                            offsetTransforms[offset].position = baseReplaceTransforms[offset].position;
                            offsetTransforms[offset].eulerAngles = baseReplaceTransforms[offset].eulerAngles;
                        }
                        else
                        {
                            offsetTransforms[offset].position = baseData[offset].bone.position;
                            offsetTransforms[offset].eulerAngles = baseData[offset].bone.eulerAngles;
                        }
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

                if (offsetVectors[offset].hint != Vector3.zero)
                    hintTransforms[offset].localPosition += offsetVectors[offset].hint;

                if (shoeOffset && ((offset == (int)BodyPart.LeftFoot) || (offset == (int)BodyPart.RightFoot)))
                    offsetTransforms[offset].localPosition += offsetTransforms[offset].up * shoeOffsetAmount;

                switch (offset)
                {
                    case (int)BodyPart.WholeBody:
                        break;
                    case (int)BodyPart.LeftHand:
                        for (var digit = 0; digit < digitTransforms[offset].Length; digit++)
                            digitTransforms[offset][digit].localEulerAngles -= new Vector3(0f, 0f, offsetVectors[offset].digitRotation);
                        break;
                    case (int)BodyPart.RightHand:
                        for (var digit = 0; digit < digitTransforms[offset].Length; digit++)
                            digitTransforms[offset][digit].localEulerAngles += new Vector3(0f, 0f, offsetVectors[offset].digitRotation);
                        break;
                    case (int)BodyPart.LeftFoot:
                    case (int)BodyPart.RightFoot:
                        digitTransforms[offset][0].localEulerAngles -= new Vector3(offsetVectors[offset].digitRotation, 0f, 0f);
                        break;
                }
            }

            if (offsetVectors[(int)BodyPart.WholeBody].hint != Vector3.zero)
            {
                for (int offset = 0; offset < headTransforms.Count(); offset++)
                    headTransforms[offset].localEulerAngles += offsetVectors[(int)BodyPart.WholeBody].hint;
            }
        }

        public void ApplyJointCorrections()
        {
            for (int offset = (int)BodyPart.LeftHand; offset < offsetTransforms.Length; offset++)
            {
                Expression expression = offsetTransforms[offset].GetComponentInParent<Expression>();
                if (expression == null)
                    continue;

                foreach (var info in expression.info)
                {
                    if (info.categoryNo == offset - 1)
                        info.enable = jointCorrection[offset];
                }
            }
        }

        public void ApplyKissOffset(Transform offsetTarget)
        {
            Vector3 offset = offsetTarget.position
                           + offsetTarget.right * AI_BetterHScenes.kissOffset.Value.x
                           + offsetTarget.up * AI_BetterHScenes.kissOffset.Value.y
                           + offsetTarget.forward * AI_BetterHScenes.kissOffset.Value.z
                           - mouthTransform.position;

            hipTransform.position += offset;
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
