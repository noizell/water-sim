using Cysharp.Threading.Tasks;
using System.Threading;

namespace NPP.TaskTimers
{
    public class ConditionalTaskDelay : TaskDelay
    {
        protected System.Func<bool> Condition;

        public ConditionalTaskDelay(float delay, System.Func<bool> condition, System.Action onConditionTrue, System.Action<int> onEachInterval, TaskDelayManager taskDelayManager, DelayType delayMode)
        {
            Delay = delay;
            Condition = condition;
            OnDoneCallback = onConditionTrue;
            OnInterval = onEachInterval;
            Manager = taskDelayManager;
            IsRunning = false;
            DelayMode = delayMode;
        }

        public override async UniTask Run()
        {
            IsRunning = true;
            try
            {
                CurrentInterval = 0;
                while (Condition.Invoke() == false)
                {
                    await UniTask.Create(async () =>
                    {
                        CurrentInterval += 1;
                        await UniTask.Delay((int)(MILISECONDS * Delay), delayType: GetDelayType(), cancellationToken: Cancellation.Token);
                        OnInterval?.Invoke(CurrentInterval);
                    });
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
    }
}