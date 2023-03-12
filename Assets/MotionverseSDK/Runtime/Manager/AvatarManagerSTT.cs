using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using UnityEngine;
using MotionverseSDK.Core;
using Unity.VisualScripting;

namespace OpenAI
{
    [RequireComponent(typeof(AudioSource))]
    public class AvatarManagerSTT : MonoBehaviour
    {

        private static AvatarManagerSTT manager;

        public static AvatarManagerSTT Manager
        {
            get { return manager; }
        }

        private SkinnedMeshRenderer skinnedMeshRenderer;
        private Mesh skinnedMesh;
        private Animator animator;
        private AudioSource audioSource;

        Dictionary<int, AnimationCurve> bsCurves = new();
        Dictionary<string, string> bsDictionary = new();
        private Dictionary<string, Dictionary<string, AnimationCurve>> AnimCurves = new();
        private Dictionary<string, Transform> Skeletons = new();
        private List<float> boneValueOriList = new(); // 骨骼初始值
        private Dictionary<string, Quaternion> SkeletopnsLastRot = new();

        private Transform characterTransform;
        private bool isPlaying = false;
        static readonly int faceLength = 51;

        public string AppID;
        public string SecretKey;


        public string voiceName = "aiqi";
        public int bodyMotion = 1;
        public string styleTag = "Kefu";

        void Awake()
        {
            Application.targetFrameRate = 30;//锁定最大帧率为30帧
            UnityEngine.Analytics.Analytics.enabled = false;
            UnityEngine.Analytics.Analytics.deviceStatsEnabled = false;
            UnityEngine.Analytics.Analytics.initializeOnStartup = false;
            UnityEngine.Analytics.Analytics.limitUserTracking = false;
            UnityEngine.Analytics.PerformanceReporting.enabled = false;

            if (manager != null && manager != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                manager = this;
                DontDestroyOnLoad(gameObject);
            }

#if !UNITY_EDITOR
        //Debug.unityLogger.logEnabled = false;
#endif

        }

        void Start()
        {
            /*audioSource = GetComponent<AudioSource>();
            TextDriveUtils.CallBack += StartDrive;
            AudioDriveUtils.CallBack += StartDrive;
            AnswerDriveUtils.CallBack += StartDrive;*/
            Invoke("InitCharacter", 0.5f);
        }


        public void StartDrive(Drive drive)
        {
            audioSource.clip = drive.clip;
            SetBlendShape(drive.bsData);
            SetBodyData(drive.motionData);
            DataReady();
        }
        public void AnswerMotion(string text, int state = 0)
        {
            if (audioSource.time > 0)
                return;
            AnswerDriveUtils.GetMotion(text);
        }
        private void InitCharacter()
        {
            var accessToken = TokenManager.Instance.accessToken;
            Transform[] transforms = transform.GetComponentsInChildren<Transform>();
            bsDictionary.Clear();
            foreach (Transform item in transforms)
            {
                if (item.name.Contains("Face_"))
                {
                    skinnedMeshRenderer = item.GetComponent<SkinnedMeshRenderer>();
                    skinnedMesh = skinnedMeshRenderer.sharedMesh;
                    item.AddComponent<EyeBlink>();
                    for (int i = 0; i < skinnedMesh.blendShapeCount; i++)
                    {
                        foreach (var bs in Config.BSNamesGeneric)
                        {
                            if (skinnedMesh.GetBlendShapeName(i).Contains(bs))
                            {
                                bsDictionary.Add(bs, skinnedMesh.GetBlendShapeName(i));
                            }
                        }
                    }
                    break;
                }
            }

            foreach (var item in Config.Skeleton_joints)
            {
                Transform go = Utils.FindChildRecursively(transform, item);

                Skeletons.Add(item, go);
                SkeletopnsLastRot.Add(item, go.localRotation);
                boneValueOriList.Add(go.transform.localRotation.x);
                boneValueOriList.Add(go.transform.localRotation.y);
                boneValueOriList.Add(go.transform.localRotation.z);
                boneValueOriList.Add(go.transform.localRotation.w);
            }
            animator = gameObject.GetComponentInChildren<Animator>();

            Debug.Log("初始化完成");
        }


        public void LateUpdate()
        {
            if (audioSource.clip != null && audioSource.time > 0)
            {
                isPlaying = true;
                PlayMotion(audioSource.time);
                PlayBS(4);
            }
            else if (audioSource.clip != null && isPlaying)
            {
                audioSource.clip = null;
                isPlaying = false;
                StartCoroutine(BodyAnimEnd());
            }


            if (Skeletons.Count > 0)
            {
                foreach (var item in Skeletons)
                {
                    var static_rot = SkeletopnsLastRot[item.Key];
                    Quaternion motion_rot = Skeletons[item.Key].localRotation;
                    Quaternion localRot = Quaternion.Lerp(static_rot, motion_rot, 0.2f);
                    Skeletons[item.Key].localRotation = localRot;
                    SkeletopnsLastRot[item.Key] = Skeletons[item.Key].localRotation;
                }
            }
        }

        public void PlayMotion(float evalTime)
        {
            if (AnimCurves.Count == 0)
                return;

            foreach (var boneName in AnimCurves.Keys)
            {
                if (!Config.Skeleton_joints.Contains(boneName))
                    continue;

                Skeletons[boneName].localRotation = new Quaternion(
                    AnimCurves[boneName]["m_LocalRotation.x"].Evaluate(evalTime),
                    AnimCurves[boneName]["m_LocalRotation.y"].Evaluate(evalTime),
                    AnimCurves[boneName]["m_LocalRotation.z"].Evaluate(evalTime),
                    AnimCurves[boneName]["m_LocalRotation.w"].Evaluate(evalTime));
            }
        }
        public void PlayBS(float evalTime)
        {
            if (skinnedMeshRenderer == null || bsCurves.Count == 0)
                return;

            for (int i = 0; i < faceLength; ++i)
            {
                var name = Config.BSNamesGeneric[i];
                if (bsDictionary.ContainsKey(name) == true)
                {
                    var bs = bsCurves[i].Evaluate(evalTime);
                    skinnedMeshRenderer.SetBlendShapeWeight(skinnedMesh.GetBlendShapeIndex(bsDictionary[name]), bs);
                }

            }
        }

        IEnumerator BodyAnimEnd()
        {
            int idx = 0;
            foreach (var item in Config.Skeleton_joints)
            {
                Transform go = GameObject.Find(item).transform;
                Quaternion rot = new(boneValueOriList[idx * 4 + 0],
                                                boneValueOriList[idx * 4 + 1],
                                                boneValueOriList[idx * 4 + 2],
                                                boneValueOriList[idx * 4 + 3]);
                Skeletons[item].DOLocalRotateQuaternion(rot, 0.6f);
                idx++;
            }
            yield return new WaitForSeconds(0.6f);
            animator.enabled = true;
        }
        /// <summary>
        /// 准备播放
        /// </summary>
        void DataReady()
        {
            audioSource.Play();
        }

        /// <summary>
        /// 设置面部数据
        /// </summary>
        /// <summary>
        /// 设置面部数据
        /// </summary>
        public void SetBlendShape(string data, string wrapmode = null)
        {
            bsCurves.Clear();
            var blendshapeArr = JArray.Parse(data);

            Dictionary<int, List<Keyframe>> KeyframeData = new();
            for (int i = 0; i < faceLength; i++)
                KeyframeData.Add(i, new List<Keyframe>());

            float timeNow = 0f;
            foreach (JArray item in blendshapeArr.Cast<JArray>())
            {
                for (int i = 0; i < faceLength; i++)
                {
                    KeyframeData[i].Add(new Keyframe(timeNow, (float)item[i]));
                }
                timeNow += 1f / 30f;
            }

            WrapMode wm = WrapMode.Default;
            if (wrapmode != null)
            {
                wm = wrapmode switch
                {
                    "PingPong" => WrapMode.PingPong,
                    "Loop" => WrapMode.Loop,
                    "Once" => WrapMode.Once,
                    _ => WrapMode.Default,
                };
            }


            for (int i = 0; i < faceLength; i++)
            {
                bsCurves.Add(i, new AnimationCurve(KeyframeData[i].ToArray()));
                bsCurves[i].preWrapMode = wm;
                bsCurves[i].postWrapMode = wm;
            }
        }


        /// <summary>
        /// 设置身体数据
        /// </summary>
        private void SetBodyData(byte[] data, string wrapmode = null)
        {
            AnimCurves.Clear();

            int rotasLength = (int)BitConverter.ToSingle(data, 0);
            int curvesLength = (int)BitConverter.ToSingle(data, 4);

            for (int i = 0; i < curvesLength; i++)
            {

                List<Keyframe> rx = new();
                List<Keyframe> ry = new();
                List<Keyframe> rz = new();
                List<Keyframe> rw = new();

                float timeNow = 0f;
                for (int j = 0; j < rotasLength; j++)
                {

                    rx.Add(new Keyframe(timeNow, BitConverter.ToSingle(data, 16 * rotasLength * i + j * 16 + 8)));
                    ry.Add(new Keyframe(timeNow, BitConverter.ToSingle(data, 16 * rotasLength * i + j * 16 + 12)));
                    rz.Add(new Keyframe(timeNow, BitConverter.ToSingle(data, 16 * rotasLength * i + j * 16 + 16)));
                    rw.Add(new Keyframe(timeNow, BitConverter.ToSingle(data, 16 * rotasLength * i + j * 16 + 20)));

                    timeNow += 1.0f / 30.0f;
                }

                Dictionary<string, AnimationCurve> boneCurves = new()
                {
                    { "m_LocalRotation.x", new AnimationCurve(rx.ToArray()) },
                    { "m_LocalRotation.y", new AnimationCurve(ry.ToArray()) },
                    { "m_LocalRotation.z", new AnimationCurve(rz.ToArray()) },
                    { "m_LocalRotation.w", new AnimationCurve(rw.ToArray()) }
                };

                WrapMode wm = WrapMode.Default;
                if (wrapmode != null)
                {
                    wm = wrapmode switch
                    {
                        "PingPong" => WrapMode.PingPong,
                        "Loop" => WrapMode.Loop,
                        "Once" => WrapMode.Once,
                        _ => WrapMode.Default,
                    };
                }

                boneCurves["m_LocalRotation.x"].preWrapMode = wm;
                boneCurves["m_LocalRotation.x"].postWrapMode = wm;
                boneCurves["m_LocalRotation.y"].preWrapMode = wm;
                boneCurves["m_LocalRotation.y"].postWrapMode = wm;
                boneCurves["m_LocalRotation.z"].preWrapMode = wm;
                boneCurves["m_LocalRotation.z"].postWrapMode = wm;
                boneCurves["m_LocalRotation.w"].preWrapMode = wm;
                boneCurves["m_LocalRotation.w"].postWrapMode = wm;
                AnimCurves.Add(Config.Skeleton_joints[i], boneCurves);

                animator.enabled = false;

            }

        }

        private void OnDestroy()
        {
            TextDriveUtils.CallBack -= StartDrive;
            AudioDriveUtils.CallBack -= StartDrive;
            AnswerDriveUtils.CallBack -= StartDrive;
        }

        /// <summary>
        /// 清除父物体下面的所有子物体
        /// </summary>
        /// <param name="parent"></param>
        private void ClearChilds(Transform parent)
        {
            if (parent.childCount > 0)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Destroy(parent.GetChild(i).gameObject);
                }
            }
        }
    }
}
