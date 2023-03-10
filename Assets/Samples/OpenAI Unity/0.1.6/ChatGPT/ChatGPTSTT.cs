using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        private string Instruction = "Act as a random stranger in a chat room and reply to the questions.\nQ: ";


        public async void SendReply(string userInput)
        {
            Instruction += $"{userInput}\nA: ";

            textArea.text = "...";

            // Complete the instruction
            var completionResponse = await openai.CreateCompletion(new CreateCompletionRequest()
            {
                Prompt = Instruction,
                Model = "text-davinci-003",
                MaxTokens = 128
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                textArea.text = completionResponse.Choices[0].Text;
                Instruction += $"{completionResponse.Choices[0].Text}\nQ: ";
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }

        private void Start()
        {

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
    }
}
