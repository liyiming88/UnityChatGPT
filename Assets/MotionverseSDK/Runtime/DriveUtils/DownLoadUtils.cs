using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace OpenAI
{
    public class DownLoadUtils
    {
        private static Dictionary<string, DownCache> m_cacheDownload = new();//下载缓存
        private static Dictionary<string, TaskInfo> m_taskCallBack = new();//下载回调缓存

        private static List<string> m_waitDownloadTask = new();//等待下载的列表
        private static List<string> m_curDownloadTask = new();//当前正在下载的列表

        private static int m_maxDownloadNum = 20;//最大可同时下载数量
        private static int m_DownloadTimeOut = 20;//下载超时

        /// <summary>
        /// 一个url对应一个TaskInfo，里面保存了该url的下载类型DownloadHandler，所有监听该url下载的回调
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

        //下载
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

            //不在当前的下载、等待列表，加入执行队列
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
                    return;//没有等待下载的任务
                }

                url = m_waitDownloadTask[0];
                m_waitDownloadTask.RemoveAt(0);
            }

            //当前并发下载数大于3，缓存
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

        //下载错误、下载结束都清掉这个url任务
        private static void DownloadEnd(string url)
        {
            m_taskCallBack.Remove(url);
            m_curDownloadTask.Remove(url);
            CastTask(null);
        }

        private static void HandleDownload(string url, DownloadHandler handle = null)
        {

            AudioClip clip = null;
            DownCache cacheHandle = new();//缓存，req.Dispose会销毁handle，所以这边单独缓存
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

        //移除某个链接下载
        public static void RemoveHandle(string url)
        {
            m_taskCallBack.Remove(url);
            if (m_waitDownloadTask.Contains(url))
                m_waitDownloadTask.Remove(url);
        }

        //移除单个下载任务
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
