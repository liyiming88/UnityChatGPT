using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenAI
{
    public class SpeechToText : MonoBehaviour
    {
        private static SpeechToText speechy;

        public static SpeechToText Speechy
        {
            get { return speechy; }
        }
        public TMP_Text outputText;
        // PULLED OUT OF BUTTON CLICK
        SpeechRecognizer recognizer;
        SpeechConfig config;
        SpeechSynthesizer synthesizer;
        // if the whole message has been transcribed over
        bool isMessageOver = false;

        private object threadLocker = new object();
        private bool speechStarted = false; //checking to see if you've started listening for speech
        private string message;

        private bool once = true;
        private JArray bsArray = new JArray();

        // 用来识别整句输出，并且输出完整文本
        private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
        {
            lock (threadLocker)
            {
                message = e.Result.Text;
                isMessageOver = true;
            }
        }

        
        private void StartViseme(object sender, SpeechSynthesisVisemeEventArgs e)
        {
            /*            Debug.Log($"Viseme event received. Audio offset: " +
                                $"{e.AudioOffset / 10000}ms, viseme id: {e.VisemeId}.");*/
            // `Animation` is an xml string for SVG or a json string for blend shapes
            if (e.Animation != "")
            {
                var animation = e.Animation;
                JObject jsonObject = (JObject)JsonConvert.DeserializeObject(animation);
                string bs_str = JsonConvert.SerializeObject(jsonObject["BlendShapes"]);
                JArray br_arr = JArray.Parse(bs_str);
                foreach (var item in br_arr)
                {
                    bsArray.Add(item);
                }
            }
        }

        // 开始录音
        public async void Recording()
        {
            
            using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
            lock (threadLocker)
            {
                speechStarted = true;
            }
        }

        public async void KillRecord()
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            
        }

        // 文字转语音
        public async void SynthesizeAudioAsync(string text)
        {
            //todo replace "a" to text
            var content = text;
            var ssml = @$"<speak version='1.0' xml:lang='en-US' xmlns='http://www.w3.org/2001/10/synthesis' xmlns:mstts='http://www.w3.org/2001/mstts'>
            <voice name='en-US-JennyNeural'>
                <mstts:viseme type='FacialExpression'/>
                {content}
            </voice>
        </speak>";
            await synthesizer.SpeakSsmlAsync(ssml);
            AvatarManager.Instance.startPlayViseme = true;
            AvatarManager.Instance.SetBlendShape(bsArray);
        }

       /* public delegate void CallbackDelegat(string message);
        public void DoSomething(CallbackDelegat callback)
        {
            // Do something
            string result = "Done!"; 
            callback(result);
        }
        public void Main()
        {
            CallbackDelegat callback = new CallbackDelegat(MyCallback);
            DoSomething(callback);
        }
        public void MyCallback(string message) { Debug.Log(message); }*/


        private async void StopRecord(object sender, SpeechSynthesisEventArgs e)
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
            lock (threadLocker)
            {
                speechStarted = true;
            }
        }

        private async void RestartRecord(object sender, SpeechSynthesisEventArgs e)
        {
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
            lock (threadLocker)
            {
                speechStarted = true;
            }
            

        }


        void Start()
        {
            config = SpeechConfig.FromSubscription("e512bf00442d427e9158b0e381563240", "eastus");
            // 这两项是配置文字转语音的语言和人物
            config.SpeechSynthesisLanguage = "en-US";
            config.SpeechSynthesisVoiceName = "en-US-JennyNeural";
            // 新建合成语音转换器
            synthesizer = new SpeechSynthesizer(config);
            synthesizer.SynthesisStarted += StopRecord;
            synthesizer.SynthesisCompleted += RestartRecord; 
             // 新建语音识别器
             recognizer = new SpeechRecognizer(config);
            // 订阅事件：当用户语音完整输出后，调用Handler
            recognizer.Recognized += RecognizedHandler;
            // // 当文字转语音开始调用时，也开始调用该方法，输出口型数据
            synthesizer.VisemeReceived += StartViseme;
            string[] aaa = Microphone.devices;
        }

        void Update()
        {

            lock (threadLocker)
            {
                if (outputText != null)
                {
                    outputText.text = message;
                }
            }

            if (isMessageOver)
            {
                isMessageOver = false;
                ChatGPTSTT.Chatty.SendReply(message);
            }
        }

        void Awake()
        {
            if (speechy != null && speechy != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                speechy = this;
                DontDestroyOnLoad(gameObject);
            }
        }


        void OnDestroy()
        {
            Debug.Log("");
        }
    }
}
