using System.Collections;
using MotionverseSDK.Core;
namespace MotionverseSDK
{
    public class TaskManager : Singleton<TaskManager>
    {
        public void Create(IEnumerator routine)
        {
            StartCoroutine(routine);
        }
    }
}
