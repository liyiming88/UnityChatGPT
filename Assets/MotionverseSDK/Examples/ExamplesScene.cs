using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionverseSDK;
public class ExamplesScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void TextDrive(string text)
    {
        TextDriveUtils.GetMotion(text);
    }

    public void AnswerDrive(string text)
    {
        AnswerDriveUtils.GetMotion(text);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
