using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace OpenAI
{
    public class ChatGPTSTT : MonoBehaviour
    {
        private static ChatGPTSTT chatty;

        public static ChatGPTSTT Chatty
        {
            get { return chatty; }
        }

        [SerializeField] private TMP_Text textArea;

        private OpenAIApi openai = new OpenAIApi();

        private string userInput;
        private string Instruction = "You are a Unity programmer.\nQ: ";


        public async void SendReply(string userInput)
        {
            try
            {
                Instruction += $"{userInput}\nA: ";

                textArea.text = "...";

                // Complete the instruction
                var completionResponse = await openai.CreateCompletion(new CreateCompletionRequest()
                {
                    Prompt = Instruction,
                    Model = "text-davinci-003",
                    MaxTokens = 128,
                    N = 1,
                    Temperature = 0.2f,
                });

                if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
                {
                    textArea.text = completionResponse.Choices[0].Text;
                    Instruction += $"{completionResponse.Choices[0].Text}\nQ: ";
                    SpeechToText.Speechy.SynthesizeAudioAsync(completionResponse.Choices[0].Text);
                }
                else
                {
                    Debug.LogWarning("No text was generated from this prompt.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

        }

        private void Start()
        {
            // 程序开始时录音
            SpeechToText.Speechy.Recording();
        }

        void Awake()
        {
            if (chatty != null && chatty != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                chatty = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        void OnDestroy()
        {
            SpeechToText.Speechy.KillRecord();
        }
    }
}
