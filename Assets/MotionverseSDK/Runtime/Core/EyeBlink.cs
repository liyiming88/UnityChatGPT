using System.Collections;
using UnityEngine;
namespace MotionverseSDK.Core
{
    public class EyeBlink : MonoBehaviour
    {
        private int curAddValue = 20;
        private int curEyeValue = 0;
        private bool isBlink = false;

        private SkinnedMeshRenderer skinnedMeshRenderer;
        private Mesh skinnedMesh;

        private void Start()
        {
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            skinnedMesh = skinnedMeshRenderer.sharedMesh;
            StartCoroutine(Blink());
        }
        private IEnumerator Blink()
        {
            float eyeBlinkTime = Random.Range(1, 3);
            yield return new WaitForSeconds(eyeBlinkTime);
            isBlink = true;
            StartCoroutine(Blink());
        }

        private void Update()
        {
            UpdataBlinkValue();
        }
        private void UpdataBlinkValue()
        {
            if (isBlink)
            {
                curEyeValue += curAddValue;
                skinnedMeshRenderer.SetBlendShapeWeight(skinnedMesh.GetBlendShapeIndex("blendShape.eyeBlinkRight"), curEyeValue);
                skinnedMeshRenderer.SetBlendShapeWeight(skinnedMesh.GetBlendShapeIndex("blendShape.eyeBlinkLeft"), curEyeValue);
                if (curEyeValue >= 100)
                {
                    curAddValue = -curAddValue;
                }
                if (curEyeValue <= 0)
                {
                    isBlink = false;
                    curAddValue = -curAddValue;
                }
            }
        }
    }
}
