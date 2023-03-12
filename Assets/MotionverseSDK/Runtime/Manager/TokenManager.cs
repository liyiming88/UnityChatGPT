using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using MotionverseSDK.Core;
namespace OpenAI
{
    public class TokenManager : Singleton<TokenManager>
    {
        [HideInInspector]
        public string accessToken = null;

        void Start()
        {
            StartCoroutine(GetToken());
        }

        IEnumerator GetToken()
        {
            Dictionary<string, string> getParameDic = new();
            long timestamp = Utils.GetTimeStampSecond();
            getParameDic.Add("appid", AvatarManager.Instance.AppID);
            getParameDic.Add("secret", AvatarManager.Instance.SecretKey);
            getParameDic.Add("timestamp", timestamp.ToString());
            getParameDic.Add("sign", Utils.SHA1(AvatarManager.Instance.AppID + timestamp + AvatarManager.Instance.SecretKey));

            StringBuilder stringBuilder = new();
            bool isFirst = true;
            foreach (var item in getParameDic)
            {
                if (isFirst)
                {
                    isFirst = false;
                    stringBuilder.Append('?');
                }
                else
                    stringBuilder.Append('&');

                stringBuilder.Append(item.Key);
                stringBuilder.Append('=');
                stringBuilder.Append(item.Value);
            }

            using (UnityWebRequest webRequest = UnityWebRequest.Get(Config.GetTokenUrl + stringBuilder))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    //Debug.Log(webRequest.downloadHandler.text);
                    accessToken = Utils.GetJsonValue(webRequest.downloadHandler.text, "access_token");
                    //Debug.Log("获取token成功");
                }
            }

            if (accessToken == string.Empty)
            {
                yield return new WaitForSeconds(1.0f);
                StartCoroutine(GetToken());
            }


        }
    }
}
