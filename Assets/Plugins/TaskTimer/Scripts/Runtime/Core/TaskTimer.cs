using UnityEngine;

namespace NPP.TaskTimers
{

    public static class TaskTimer
    {
        private static bool _initialized;
        private static TaskDelayManager _manager;

        static TaskTimer()
        {
            Initialize();
        }
        /// <summary>
        /// Initialize <see cref="TaskTimer"/>
        /// </summary>
        static void Initialize()
        {
            _initialized = true;

            if (GameObject.Find("Task Delay Manager") == null)
                _manager = new GameObject("Task Delay Manager").AddComponent<TaskDelayManager>();
            else
                _manager = GameObject.Find("Task Delay Manager").GetComponent<TaskDelayManager>();

            GameObject.DontDestroyOnLoad(_manager);
        }
        /// <summary>
        /// Create timed-based task that run for interval amount with delay on each interval.
        /// </summary>
        /// <param name="delay">Delay for each Interval.</param>
        /// <param name="onDoneWaitDelay">Called after task done.</param>
        /// <param name="interval">How many times this task should looped?</param>
        /// <param name="onInterval">Called on each interval.</param>
        /// <param name="immediateRun">Immediate run this task after creation, if set to false, run <see cref="TaskDelay.Run"/> on created <see cref="TaskDelay"/>.</param>
        /// <returns>Created <see cref="TaskDelay"/>.</returns>
        public static TaskDelay CreateTask(float delay, System.Action onDoneWaitDelay = null, int interval = 1, System.Action<int> onInterval = null, bool immediateRun = true, DelayType delayMode = DelayType.Scaled)
        {
            if (!_initialized)
                Initialize();

            return _manager.CreateTask(delay, onDoneWaitDelay, onInterval, interval, immediateRun, false, delayMode);
        }
        /// <summary>
        /// Create loop-based task that infinitely run. use <see cref="StopTask(TaskDelay, bool, System.Action)"/> to stop this task.
        /// </summary>
        /// <param name="delay">Delay for each Interval.</param>
        /// <param name="onDoneWaitDelay">Called after task done.</param>
        /// <param name="onInterval">Called on each interval.</param>
        /// <param name="immediateRun">Immediate run this task after creation, if set to false, run <see cref="TaskDelay.Run"/> on created <see cref="TaskDelay"/>.</param>
        /// <returns>Created <see cref="TaskDelay"/>.</returns>
        public static TaskDelay CreateTaskLoop(float delay, System.Action onDoneWaitDelay = null, System.Action<int> onInterval = null, bool immediateRun = true, DelayType delayMode = DelayType.Scaled)
        {
            if (!_initialized)
                Initialize();

            return _manager.CreateTask(delay, onDoneWaitDelay, onInterval, 0, immediateRun, true, delayMode);
        }
        /// <summary>
        /// Create loop-based task that run until condition fulfilled.
        /// </summary>
        /// <param name="delay">Delay for each Interval.</param>
        /// <param name="condition">Condition to check.</param>
        /// <param name="onConditionTrue">Called when condition is True/></param>
        /// <param name="onEachInterval">Called on each interval.</param>
        /// <param name="immediateRun">Immediate run this task after creation, if set to false, run <see cref="TaskDelay.Run"/> on created <see cref="TaskDelay"/>.</param>
        /// <returns>Created <see cref="TaskDelay"/>.</returns>
        public static TaskDelay CreateConditionalTask(float delay, System.Func<bool> condition, System.Action<int> onEachInterval = null, System.Action onConditionTrue = null, bool immediateRun = true, DelayType delayMode = DelayType.Scaled)
        {
            if (!_initialized)
                Initialize();

            return _manager.CreateConditionalTask(delay, condition, onConditionTrue, onEachInterval, immediateRun, delayMode);
        }
        /// <summary>
        /// Stop selected <see cref="TaskDelay"/> from running.
        /// </summary>
        /// <param name="taskDelay">Selected <see cref="TaskDelay"/></param>
        /// <param name="overrideDoneCallback">Should override OnDoneCallback previously assigned?</param>
        /// <param name="overrideCallback">New OnDoneCallback for <see cref="TaskDelay"/></param>
        public static void StopTask(TaskDelay taskDelay, bool overrideDoneCallback = false, System.Action overrideCallback = null)
        {
            taskDelay.Stop(overrideDoneCallback, overrideCallback);
        }
        /// <summary>
        /// Check if <see cref="TaskDelay"/> is running.
        /// </summary>
        /// <param name="delay">selected <see cref="TaskDelay"/></param>
        /// <returns></returns>
        public static bool Running(TaskDelay delay)
        {
            return delay.Running();
        }
        /// <summary>
        /// Check if <see cref="TaskDelay"/> is completed.
        /// </summary>
        /// <param name="taskDelay">selected <see cref="TaskDelay"/></param>
        /// <returns></returns>
        public static bool Completed(this TaskDelay taskDelay)
        {
            return !taskDelay.Running();
        }
        /// <summary>
        /// Run selected <see cref="TaskDelay"/> if not already running.
        /// </summary>
        /// <param name="taskDelay"></param>
        public static async void Run(TaskDelay taskDelay)
        {
            if (!taskDelay.Running())
                await taskDelay.Run();
        }
        /// <summary>
        /// Will stop all current runnning task that not run manually.
        /// </summary>
        public static void StopAll()
        {
            _manager.StopAll();
        }
        /// <summary>
        /// Only stop all current running task that not run manually and not considered loop task.
        /// </summary>
        public static void StopAllNonLoop()
        {
            _manager.StopAll(false);
        }
    }
}
