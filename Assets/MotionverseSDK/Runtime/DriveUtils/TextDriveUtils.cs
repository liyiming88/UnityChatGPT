using MotionverseSDK.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace OpenAI
{
    public class TextDriveUtils
    {
        public static Action<Drive> CallBack;

        private static Dictionary<string, Drive> m_cacheDrive = new();//缓存
        private static Dictionary<string, TaskInfo> m_taskCallBack = new();

        private static List<string> m_waitTask = new();//等待列表
        private static List<string> m_curTask = new();//当前正在加载列表
        private static int m_maxdNum = 3;//最大数量

        /// <summary>
        /// 一个text对应一个TaskInfo
        /// </summary>
        private class TaskInfo
        {
            private List<Action<Drive>> m_callBacks = new();

            public string Text;

            public TaskInfo(string text)
            {
                Text = text;
            }

            public void AddCallBack(Action<Drive> callBack)
            {
                if (!m_callBacks.Contains(callBack))
                {
                    m_callBacks.Add(callBack);
                }
            }

            public void RemoveCallBack(Action<Drive> callBack)
            {
                if (m_callBacks.Contains(callBack))
                {
                    m_callBacks.Remove(callBack);
                }
            }

            public void ClearCallBack()
            {
                m_callBacks.Clear();
            }

            public int Count()
            {
                return m_callBacks.Count;
            }

            public void End(Drive cache)
            {
                for (int i = 0; i < m_callBacks.Count; i++)
                {
                    m_callBacks[i]?.Invoke(cache);
                }

                ClearCallBack();
            }
        }

        public static void GetMotion(string text)
        {
            if (CallBack == null) return;
            m_cacheDrive.Clear();
            if (!m_taskCallBack.TryGetValue(Utils.EncryptWithMD5(text), out TaskInfo taskInfo))
            {
                taskInfo = new TaskInfo(Utils.EncryptWithMD5(text));
                m_taskCallBack.Add(Utils.EncryptWithMD5(text), taskInfo);

            }

            taskInfo.AddCallBack(CallBack);

            //不在当前的下载、等待列表，加入执行队列
            if (!m_waitTask.Contains(text) && !m_curTask.Contains(text))
            {
                CastTask(text);
            }

        }
        public static IEnumerator RealTextMotion(string text)
        {
            using (UnityWebRequest webRequest = new(Config.TextMotionUrl, "POST"))
            {
                TextMotionParams postData = new();
                postData.draft_content = text;
                postData.tts_config.voice_name = AvatarManager.Instance.voiceName;
                postData.body_config.body_motion = AvatarManager.Instance.bodyMotion;
                postData.body_config.style_tag = AvatarManager.Instance.styleTag;

                byte[] jsonToSend = new UTF8Encoding().GetBytes(JsonUtility.ToJson(postData));
                //Debug.Log(JsonUtility.ToJson(postData));
                webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                if (TokenManager.Instance.accessToken == null)
                {
                    yield return new WaitForSeconds(1.0f);
                    m_waitTask.Add(text);
                    TextMotionEnd(text);

                    yield break;
                }
                webRequest.SetRequestHeader("access_token", TokenManager.Instance.accessToken);
                //Debug.Log(TokenManager.Instance.accessToken);
                yield return webRequest.SendWebRequest();


                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    yield return new WaitForSeconds(1.0f);
                    m_waitTask.Add(text);
                    TextMotionEnd(text);
                    yield break;
                }

                HandleTextMotion(text, webRequest.downloadHandler.text);
                TextMotionEnd(text);
            }
        }
        //错误、结束都清掉这个text任务
        private static void TextMotionEnd(string url)
        {
            m_taskCallBack.Remove(url);
            m_curTask.Remove(url);
            CastTask(null);
        }


        private static void CastTask(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                if (m_waitTask.Count == 0)
                {
                    return;//没有等待下载的任务
                }

                text = m_waitTask[0];
                m_waitTask.RemoveAt(0);
            }

            //当前并发下载数大于1，缓存
            if (m_curTask.Count > m_maxdNum)
            {
                m_waitTask.Add(text);
            }
            else
            {
                m_curTask.Add(text);
                TaskManager.Instance.Create(RealTextMotion(text));
            }
        }

        private static void HandleTextMotion(string text, string handle)
        {
            string msg = Utils.GetJsonValue(handle, "msg");


            if (msg.Equals("ok") || msg.Equals("succese"))
            {

                string audioUrl = Utils.GetJsonValue(handle, "audio_url");
                List<string> ossUrls = Utils.GetJsonValues(handle, "oss_url");

                m_cacheDrive.Add(Utils.EncryptWithMD5(text), new Drive());

                DownLoadUtils.Download(audioUrl, (downCache) =>
                {
                    m_cacheDrive[Utils.EncryptWithMD5(text)].step = m_cacheDrive[Utils.EncryptWithMD5(text)].step + 1;
                    m_cacheDrive[Utils.EncryptWithMD5(text)].clip = downCache.clip;
                    if (m_cacheDrive[Utils.EncryptWithMD5(text)].step == 3)
                    {
                        HandleEnd(text, m_cacheDrive[Utils.EncryptWithMD5(text)]);
                    }
                });
                DownLoadUtils.Download(ossUrls[0], (downCache) =>
                {
                    m_cacheDrive[Utils.EncryptWithMD5(text)].step = m_cacheDrive[Utils.EncryptWithMD5(text)].step + 1;
                    m_cacheDrive[Utils.EncryptWithMD5(text)].bsData = downCache.text;
                    if (m_cacheDrive[Utils.EncryptWithMD5(text)].step == 3)
                    {
                        HandleEnd(text, m_cacheDrive[Utils.EncryptWithMD5(text)]);
                    }
                });
                DownLoadUtils.Download(ossUrls[1], (downCache) =>
                {
                    m_cacheDrive[Utils.EncryptWithMD5(text)].step = m_cacheDrive[Utils.EncryptWithMD5(text)].step + 1;
                    m_cacheDrive[Utils.EncryptWithMD5(text)].motionData = downCache.data;
                    if (m_cacheDrive[Utils.EncryptWithMD5(text)].step == 3)
                    {
                        HandleEnd(text, m_cacheDrive[Utils.EncryptWithMD5(text)]);
                    }
                });

            }

        }
        private static void HandleEnd(string text, Drive cacheHandle)
        {
            if (m_taskCallBack.TryGetValue(Utils.EncryptWithMD5(text), out TaskInfo taskInfo))
            {
                taskInfo.End(cacheHandle);
                m_taskCallBack.Remove(Utils.EncryptWithMD5(text));
            }
        }

    }
}
