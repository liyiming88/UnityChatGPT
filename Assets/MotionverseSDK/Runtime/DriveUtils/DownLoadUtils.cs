using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace OpenAI
{
    public class DownLoadUtils
    {
        private static Dictionary<string, DownCache> m_cacheDownload = new();//���ػ���
        private static Dictionary<string, TaskInfo> m_taskCallBack = new();//���ػص�����

        private static List<string> m_waitDownloadTask = new();//�ȴ����ص��б�
        private static List<string> m_curDownloadTask = new();//��ǰ�������ص��б�

        private static int m_maxDownloadNum = 20;//����ͬʱ��������
        private static int m_DownloadTimeOut = 20;//���س�ʱ

        /// <summary>
        /// һ��url��Ӧһ��TaskInfo�����汣���˸�url����������DownloadHandler�����м�����url���صĻص�
        /// </summary>
        private class TaskInfo
        {
            private List<Action<DownCache>> m_callBacks = new();
            public string Url;
            public DownloadHandler Handle;

            public TaskInfo(string url, DownloadHandler handle)
            {
                Url = url;
                Handle = handle;
            }

            public void AddCallBack(Action<DownCache> callBack)
            {
                if (!m_callBacks.Contains(callBack))
                {
                    m_callBacks.Add(callBack);
                }
            }

            public void RemoveCallBack(Action<DownCache> callBack)
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

            public void DownloadEnd(DownCache cache)
            {
                for (int i = 0; i < m_callBacks.Count; i++)
                {
                    m_callBacks[i]?.Invoke(cache);
                }

                ClearCallBack();
            }
        }

        public class DownCache
        {
            public byte[] data;
            public string text;
            public AudioClip clip;
            public string url;
        }

        //����
        public static void Download(string url, Action<DownCache> callBack, DownloadHandler handle = null)
        {
            if (callBack == null) return;

            if (m_cacheDownload.TryGetValue(url, out DownCache cache))
            {
                callBack(cache);
                return;
            }

            if (!m_taskCallBack.TryGetValue(url, out TaskInfo taskInfo))
            {
                taskInfo = new TaskInfo(url, handle);
                m_taskCallBack.Add(url, taskInfo);
            }

            taskInfo.AddCallBack(callBack);

            //���ڵ�ǰ�����ء��ȴ��б�����ִ�ж���
            if (!m_waitDownloadTask.Contains(url) && !m_curDownloadTask.Contains(url))
            {
                CastTask(url);
            }

        }

        private static void CastTask(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                if (m_waitDownloadTask.Count == 0)
                {
                    return;//û�еȴ����ص�����
                }

                url = m_waitDownloadTask[0];
                m_waitDownloadTask.RemoveAt(0);
            }

            //��ǰ��������������3������
            if (m_curDownloadTask.Count > m_maxDownloadNum)
            {
                m_waitDownloadTask.Add(url);
            }
            else
            {
                m_curDownloadTask.Add(url);

                if (url.Contains(".json") || url.Contains(".bin") || url.Contains("StreamingAssets"))
                {
                    TaskManager.Instance.Create(RealDownload(url));
                }
                else
                {
                    TaskManager.Instance.Create(RealDownloadAudioClip(url));
                }
            }
        }

        private static IEnumerator RealDownloadAudioClip(string url)
        {
            AudioType audioType;
            if (url.Contains(".wav"))
            {
                audioType = AudioType.WAV;
            }
            else
            {
                audioType = AudioType.MPEG;
            }
            using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(url, audioType))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    m_waitDownloadTask.Add(url);
                    DownloadEnd(url);
                    yield break;
                }

                HandleDownload(url, webRequest.downloadHandler);
                DownloadEnd(url);
            }
            yield return null;
        }

        private static IEnumerator RealDownload(string url)
        {


            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.timeout = m_DownloadTimeOut;

                yield return webRequest.SendWebRequest();


                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    m_waitDownloadTask.Add(url);
                    DownloadEnd(url);
                    yield break;
                }

                HandleDownload(url, webRequest.downloadHandler);

                DownloadEnd(url);
            }

            yield return null;
        }

        //���ش������ؽ�����������url����
        private static void DownloadEnd(string url)
        {
            m_taskCallBack.Remove(url);
            m_curDownloadTask.Remove(url);
            CastTask(null);
        }

        private static void HandleDownload(string url, DownloadHandler handle = null)
        {

            AudioClip clip = null;
            DownCache cacheHandle = new();//���棬req.Dispose������handle��������ߵ�������
            if (handle is DownloadHandlerAudioClip clipHandle)
            {
                clip = clipHandle.audioClip;
                if (clip)
                {
                    clip.name = url;
                }
            }
            else
            {
                cacheHandle.data = handle.data;
                cacheHandle.text = handle.text;
            }
            cacheHandle.url = url;
            cacheHandle.clip = clip;


            if (!m_cacheDownload.ContainsKey(url))
                m_cacheDownload.Add(url, cacheHandle);

            if (m_taskCallBack.TryGetValue(url, out TaskInfo taskInfo))
            {
                taskInfo.DownloadEnd(cacheHandle);
                m_taskCallBack.Remove(url);
            }
        }

        //�Ƴ�ĳ����������
        public static void RemoveHandle(string url)
        {
            m_taskCallBack.Remove(url);
            if (m_waitDownloadTask.Contains(url))
                m_waitDownloadTask.Remove(url);
        }

        //�Ƴ�������������
        public static void RemoveHandle(string url, Action<DownCache> callBack)
        {
            if (m_taskCallBack.TryGetValue(url, out TaskInfo taskInfo))
            {
                taskInfo.RemoveCallBack(callBack);

                if (taskInfo.Count() == 0)
                {
                    m_taskCallBack.Remove(url);
                }
            }
        }
    }
}
