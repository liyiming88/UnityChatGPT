1. clone the project.
2. config your local variable:
   Create a file called auth.json in the .openai folder' under your PC user folder and paste your openai key and organizaiton.
   ```
   {
    "api_key": "sk-...W6yi",
    "organization": "org-...L7W"
    }
3. open this project by Unity, go to SpeechToText.cs file, in void Start method, replace your Azure Speech Studio service region and resouce key.
   ```
   config = SpeechConfig.FromSubscription("e512bf00xxxxxxxxxxxxxxxxx", "easxxx");
