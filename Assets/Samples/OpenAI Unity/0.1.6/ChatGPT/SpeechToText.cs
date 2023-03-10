using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;
using TMPro;

namespace OpenAI {
	public class SpeechToText : MonoBehaviour
	{
		public TMP_Text outputText;
		public Button startRecordButton;

		// PULLED OUT OF BUTTON CLICK
		SpeechRecognizer recognizer;
		SpeechConfig config;
		bool fisrtcall = true;

		bool chatPermission = false;

		private object threadLocker = new object();
		private bool speechStarted = false; //checking to see if you've started listening for speech
		private string message;

		private bool micPermissionGranted = false;

		private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
		{
			lock (threadLocker)
			{
				message = e.Result.Text;
			}
		}
		public async void ButtonClick()
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
		}

		void Start()
		{
			startRecordButton.onClick.AddListener(ButtonClick);
			config = SpeechConfig.FromSubscription("e512bf00442d427e9158b0e381563240", "eastus");
			recognizer = new SpeechRecognizer(config);
			recognizer.Recognizing += RecognizingHandler;
			
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

			if (!speechStarted && !fisrtcall && chatPermission) {
				chatPermission = false;
				ChatGPTSTT.Chatty.SendReply(message);
			}
		}
	}
}
