using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using TMPro;

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
        
        // 用来识别整句输出，并且输出完整文本
        private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
        {
            lock (threadLocker)
            {
                message = e.Result.Text;
                isMessageOver = true;
            }
        }

        // 开始录音
        public async void Recording()
        {
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
            lock (threadLocker)
            {
                speechStarted = true;
            }
        }

        // 文字转语音
        public async void SynthesizeAudioAsync(string text)
        {
            await synthesizer.SpeakTextAsync(text);
        }


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
            // 新建语音转换器
            synthesizer = new SpeechSynthesizer(config);
            synthesizer.SynthesisStarted += StopRecord;
            synthesizer.SynthesisCompleted += RestartRecord; 
             // 新建语音识别器
             recognizer = new SpeechRecognizer(config);
            // 订阅事件：当用户语音完整输出后，调用Handler
            recognizer.Recognized += RecognizedHandler;
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
    }
}
