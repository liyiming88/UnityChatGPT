using System;
namespace MotionverseSDK.Core
{
    public class PETimeTask
    {
        /// <summary>
        /// 全局任务ID
        /// </summary>
        public string taskID;
        /// <summary>
        /// 首次延迟时间
        /// </summary>
        public float firstDelayTime;
        /// <summary>
        /// 回调函数
        /// </summary>
        public Action callback;
        /// <summary>
        /// 之后每次循环调用延迟的时间
        /// </summary>
        public float delayTime;
        /// <summary>
        /// 执行次数
        /// </summary>
        public int count;

        public PETimeTask(Action callback, float firstDelayTime, float delayTime, int count)
        {
            this.taskID = Guid.NewGuid().ToString();
            this.callback = callback;
            this.firstDelayTime = firstDelayTime;
            this.delayTime = delayTime;
            this.count = count;
        }
    }
    public class PEFrameTask
    {
        /// <summary>
        /// 全局任务ID
        /// </summary>
        public string taskID;
        /// <summary>
        /// 首次延迟帧数
        /// </summary>
        public int firstDelayFrame;
        /// <summary>
        /// 回调函数
        /// </summary>
        public Action callback;
        /// <summary>
        /// 之后每次循环调用的延迟帧数
        /// </summary>
        public int delayFrame;
        /// <summary>
        /// 执行次数
        /// </summary>
        public int count;

        public PEFrameTask(Action callback, int firstDelayFrame, int delayFrame, int count)
        {
            this.taskID = Guid.NewGuid().ToString();
            this.callback = callback;
            this.firstDelayFrame = firstDelayFrame;
            this.delayFrame = delayFrame;
            this.count = count;
        }
    }
    public enum PETimeTaskUnit
    {
        Millisecond,
        Second,
        Minute,
        Hour,
        Day
    }
}
