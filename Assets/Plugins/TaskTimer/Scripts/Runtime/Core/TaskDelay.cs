using Cysharp.Threading.Tasks;
using System.Threading;

namespace NPP.TaskTimers
{
    public class TaskDelay
    {
        protected float Delay;
        protected int Interval;
        protected UniTask Task;
        protected System.Action OnDoneCallback;
        protected System.Action<int> OnInterval;
        protected bool IsRunning;
        protected bool StopRequest;
        protected bool Loop;
        protected int CurrentInterval;
        protected TaskDelayManager Manager;
        protected DelayType DelayMode;
        public const int MILISECONDS = 1000;

        protected CancellationTokenSource Cancellation = new CancellationTokenSource();

        public TaskDelay()
        {
            Delay = 1f;
            Interval = 1;
            IsRunning = false;
            DelayMode = DelayType.Scaled;
        }

        public TaskDelay(float delay, System.Action onDoneCallback = null, int interval = 1, System.Action<int> onInterval = null, bool loop = false, TaskDelayManager taskDelayManager = null, DelayType delayMode = DelayType.Scaled)
        {
            Delay = delay;
            OnDoneCallback = onDoneCallback;
            OnInterval = onInterval;
            Interval = interval;
            Loop = loop;
            Manager = taskDelayManager;
            IsRunning = false;
            DelayMode = delayMode;
        }

        public bool Running()
        {
            return IsRunning;
        }

        public bool Looping()
        {
            return Loop;
        }
        /// <summary>
        /// Stop selected task, may override <see cref="OnDoneCallback"/>'s value that previously assigned.
        /// </summary>
        /// <param name="overrideDoneCallback">Allow to override previous <see cref="OnDoneCallback"/> with new one</param>
        /// <param name="doneCallback">New callback value for <see cref="OnDoneCallback"/> </param>
        public virtual void Stop(bool overrideDoneCallback = false, System.Action doneCallback = null)
        {
            if (IsRunning)
            {
                if (overrideDoneCallback)
                    OnDoneCallback = doneCallback;

                Cancellation.Cancel();
            }
        }
        /// <summary>
        /// Enqueue task so it will run by <see cref="TaskDelayManager"/>.
        /// </summary>
        public void Enqueue()
        {
            Manager.Enqueue(this);
        }
        /// <summary>
        /// Run Task manually, if you do not wish to, use <see cref="Enqueue()"/> instead.
        /// </summary>
        /// <returns></returns>
        public virtual async UniTask Run()
        {
            IsRunning = true;
            try
            {
                CurrentInterval = 0;

                if (Loop)
                {
                    while (Loop)
                    {
                        await UniTask.Create(async () =>
                        {
                            CurrentInterval += 1;
                            await UniTask.Delay((int)(MILISECONDS * Delay), delayType: GetDelayType(), cancellationToken: Cancellation.Token);
                            OnInterval?.Invoke(CurrentInterval);
                        });
                    }
                }
                else
                {
                    while (Interval > 0)
                    {
                        await UniTask.Create(async () =>
                        {
                            CurrentInterval += 1;
                            await UniTask.Delay((int)(MILISECONDS * Delay), delayType: GetDelayType(), cancellationToken: Cancellation.Token);
                            OnInterval?.Invoke(CurrentInterval);
                            Interval--;
                        });
                    }
                }

                await UniTask.Create(() =>
                {
                    IsRunning = false;
                    OnDoneCallback?.Invoke();

                    Cancellation = new CancellationTokenSource();
                    return UniTask.CompletedTask;
                });
            }
            catch (System.OperationCanceledException)
            {
                if (Cancellation.IsCancellationRequested)
                {
                    await UniTask.Create(() =>
                    {
                        IsRunning = false;
                        OnDoneCallback?.Invoke();

                        Cancellation = new CancellationTokenSource();
                        return UniTask.CompletedTask;
                    });
                }
            }
        }

        protected virtual Cysharp.Threading.Tasks.DelayType GetDelayType()
        {
            switch (DelayMode)
            {
                case DelayType.Realtime:
                    return Cysharp.Threading.Tasks.DelayType.Realtime;

                case DelayType.Unscaled:
                    return Cysharp.Threading.Tasks.DelayType.UnscaledDeltaTime;

                default:
                    return Cysharp.Threading.Tasks.DelayType.DeltaTime;
            }
        }
    }

    public enum DelayType
    {
        /// <summary>
        /// Will run on <see cref="UnityEngine.Time.timeScale"/> values.
        /// </summary>
        Scaled = 0,
        /// <summary>
        /// Independent from <see cref="UnityEngine.Time.timeScale"/> values, even <see cref="UnityEngine.Time.timeScale"/> is 0 it will still update.
        /// </summary>
        Unscaled = 1,
        /// <summary>
        /// Run delay on real-time.
        /// </summary>
        Realtime = 2
    }
}