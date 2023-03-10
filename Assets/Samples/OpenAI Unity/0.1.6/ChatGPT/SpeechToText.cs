using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using TMPro;

namespace OpenAI
{
    public class SpeechToText : MonoBehaviour
    {
        public TMP_Text outputText;

        // PULLED OUT OF BUTTON CLICK
        SpeechRecognizer recognizer;
        SpeechConfig config;
        // if the whole message has been transcribed over
        bool isMessageOver = false;

        private object threadLocker = new object();
        private bool speechStarted = false; //checking to see if you've started listening for speech
        private string message;
        
        // It will give you a whole speech.
        private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
        {
            lock (threadLocker)
            {
                message = e.Result.Text;
                isMessageOver = true;
            }

        }

        public async void Recording()
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
            recognizer = new SpeechRecognizer(config);
            recognizer.Recognized += RecognizedHandler;
            Recording();
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

        /*        public async void ButtonClick()
        {
            if (speechStarted)
            {
                await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false); // this stops the listening when you click the button, if it's already on
                lock (threadLocker)
                {
                    speechStarted = false;
                    fisrtcall = false;
                    chatPermission = true;
                }
            }
            else
            {
                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false); // this will start the listening when you click the button, if it's already off
                lock (threadLocker)
                {
                    speechStarted = true;
                }
            }
        }*/
    }
}
