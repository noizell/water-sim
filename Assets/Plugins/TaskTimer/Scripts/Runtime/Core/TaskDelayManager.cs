using System.Collections.Generic;
using UnityEngine;

namespace NPP.TaskTimers
{
    public class TaskDelayManager : MonoBehaviour
    {
        protected Queue<TaskDelay> DelayList;
        protected Queue<TaskDelay> StopList;
        protected TaskDelay CurTask;
        protected TaskDelay StopTask;

        ConditionalTaskDelay _conditionalCachedTask;
        TaskDelay _cachedTask;

        private void Awake()
        {
            DelayList = new Queue<TaskDelay>();
            StopList = new Queue<TaskDelay>();
        }

        private async void Update()
        {
            while (DelayList.Count != 0)
            {
                CurTask = DelayList.Dequeue();
                StopList.Enqueue(CurTask);
                await CurTask.Run();
            }
        }

        private void OnDestroy()
        {
            StopAll();
        }

        public void Enqueue(TaskDelay task)
        {
            DelayList.Enqueue(task);
        }

        public TaskDelay CreateConditionalTask(float delay, System.Func<bool> condition, System.Action onConditionTrue, System.Action<int> onEachInterval, bool immediateRun, DelayType delayMode)
        {
            _conditionalCachedTask = new ConditionalTaskDelay(delay, condition, onConditionTrue, onEachInterval, this, delayMode);
            if (immediateRun)
                Enqueue(_conditionalCachedTask);

            return _conditionalCachedTask;
        }

        public TaskDelay CreateTask(float delay, System.Action onDoneWaitDelay, System.Action<int> onInterval, int interval, bool immediateRun, bool loop, DelayType delayMode)
        {
            _cachedTask = new TaskDelay(delay, onDoneWaitDelay, interval, onInterval, loop, this, delayMode);
            if (immediateRun)
                Enqueue(_cachedTask);

            return _cachedTask;
        }

        public void StopAll(bool includeLoop = true)
        {
            while (StopList.Count != 0)
            {
                StopTask = StopList.Dequeue();

                if (includeLoop)
                    if (StopTask.Looping())
                        if (StopTask.Running())
                        {
                            StopTask.Stop();
                            continue;
                        }

                if (StopTask.Running())
                    StopTask.Stop();
            }
        }
    }
}
