using System;
using System.Collections.Generic;
using UnityEngine;

namespace MotionverseSDK.Core
{
    public static class Config
    {

        public static string[] BSNamesGeneric = {
        "eyeBlinkRight","eyeLookDownRight","eyeLookInRight","eyeLookOutRight","eyeLookUpRight",
        "eyeSquintRight","eyeWideRight","eyeBlinkLeft","eyeLookDownLeft","eyeLookInLeft","eyeLookOutLeft","eyeLookUpLeft",
        "eyeSquintLeft","eyeWideLeft","jawForward","jawRight","jawLeft","jawOpen","mouthClose","mouthFunnel",
        "mouthPucker","mouthRight","mouthLeft","mouthSmileLeft","mouthSmileRight","mouthFrownRight","mouthFrownLeft",
        "mouthDimpleRight","mouthDimpleLeft","mouthStretchRight","mouthStretchLeft","mouthRollLower","mouthRollUpper",
        "mouthShrugLower","mouthShrugUpper","mouthPressRight","mouthPressLeft","mouthLowerDownRight","mouthLowerDownLeft",
        "mouthUpperUpRight","mouthUpperUpLeft","browDownRight","browDownLeft","browInnerUp","browOuterUpRight",
        "browOuterUpLeft","cheekPuff","cheekSquintRight","cheekSquintLeft","noseSneerRight","noseSneerLeft"
        };
        public static List<string> Skeleton_joints = new()
        {
            "Hips",
            "LeftUpLeg",
            "LeftLeg",
            "LeftFoot",
            "LeftToeBase",
            "RightUpLeg",
            "RightLeg",
            "RightFoot",
            "RightToeBase",
            "Spine",
            "Spine1",
            "Spine2",
            "Neck",
            "Head",
            "LeftShoulder",
            "LeftArm",
            "LeftForeArm",
            "LeftHand",
            "RightShoulder",
            "RightArm",
            "RightForeArm",
            "RightHand",
            "LeftHandThumb1",
            "LeftHandThumb2",
            "LeftHandThumb3",
            "LeftHandThumb4",
            "LeftHandIndex1",
            "LeftHandIndex2",
            "LeftHandIndex3",
            "LeftHandIndex4",
            "LeftHandMiddle1",
            "LeftHandMiddle2",
            "LeftHandMiddle3",
            "LeftHandMiddle4",
            "LeftHandRing1",
            "LeftHandRing2",
            "LeftHandRing3",
            "LeftHandRing4",
            "LeftHandPinky1",
            "LeftHandPinky2",
            "LeftHandPinky3",
            "LeftHandPinky4",
            "RightHandThumb1",
            "RightHandThumb2",
            "RightHandThumb3",
            "RightHandThumb4",
            "RightHandIndex1",
            "RightHandIndex2",
            "RightHandIndex3",
            "RightHandIndex4",
            "RightHandMiddle1",
            "RightHandMiddle2",
            "RightHandMiddle3",
            "RightHandMiddle4",
            "RightHandRing1",
            "RightHandRing2",
            "RightHandRing3",
            "RightHandRing4",
            "RightHandPinky1",
            "RightHandPinky2",
            "RightHandPinky3",
            "RightHandPinky4",
        };
        public static string Host = "https://motionverseapi.deepscience.cn";
        public static string TextMotionUrl = Host + "/v2.2/api/textBroadcastMotion";
        public static string AudioMotionUrl = Host + "/v2.2/api/voiceBroadcastMotion";
        public static string AnswerPrivateUrl = Host + "/v2.2/api/AnswerCollectMotion";
        public static string GetTokenUrl = Host + "/users/getAppToken";

    }

    [Serializable]
    public class BodyData
    {
        public float totalTime;
        public BoneData[] curves;
    }
    public class DriveData
    {
        public string audioUrl = null;
        public string bsUrl = null;
        public string motionUrl = null;
    }

    public class Drive
    {
        public int step = 0;
        public string text = null;
        public AudioClip clip;
        public string bsData;
        public Byte[] motionData;
    }

    public class BoneData
    {
        public string name;
        public string path;
        public Quaternion[] rotas;
    }
    [Serializable]
    public class AudioMotionParams
    {
        public string audio_url = null;            //请求文本(必填)
        public bool compress = true;
        public MotionParams body_config = new();
        public FaceParams face_config = new();
    }

    [Serializable]
    public class TextMotionParams
    {
        public string draft_content = null;            //请求文本(必填)
        public bool compress = true;
        public MotionParams body_config = new();
        public FaceParams face_config = new();
        public TTSParams tts_config = new();

    }

    [Serializable]
    public class FaceParams
    {
        public string gender = "female";
    }
    public class Attribute
    {
        public string type = null;
        public int probability = 1;
    }

    [Serializable]
    public class MotionParams
    {
        public int body_motion = 4;
        public string base_name = "v005";
        public bool fixed_hips = true;
        public bool limit_spine = true;
        public int limit_degree = 1;
        public string style_tag = "Kefu";
    }
    [Serializable]
    public class AnswerMotionParams
    {
        public string text = "";            //请求文本(必填)
        public bool compress = true;
        public NLPParams nlp_config = new();
        public MotionParams body_config = new();
        public FaceParams face_config = new();
        public TTSParams tts_config = new();
    }
    [Serializable]
    public class NLPParams
    {
        public int qa_type = 2;
        public bool go_chatBot = true;
        public bool is_random = true;
    }
    [Serializable]
    public class TTSParams
    {
        public string voice_name = "aixia";
        public float speed = 50f;
    }
    [Serializable]
    public class AvatarConf
    {
        public string abName = null;
        public string img = null;
        public string name = null;
        public string voiceName = "aixia";
        public int bodyMotion = 1;
        public string styleTag = "Kefu";
        public float bsRatio = 1.5f;

    }
}
