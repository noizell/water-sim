using NPP.TaskTimers;
using SuperMaxim.Messaging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Monos.WSIM.Runtime.UI
{

    public class ChangeSpeedMultPayload
    {
        public float SpeedMultipler { get; }

        public ChangeSpeedMultPayload(float val)
        {
            SpeedMultipler = val;
        }
    }

    public class PauseResumePayload
    {
        public bool Paused { get; }

        public PauseResumePayload(bool paused)
        {
            Paused = paused;
        }
    }

    public class TimeController
    {
        public static TimeController Instance;

        public float CurrentSpeedMultiplier { get; private set; }
        public bool Paused { get; private set; }

        static TimeController()
        {
            Instance ??= new TimeController();
            Instance.CurrentSpeedMultiplier = 1;

            Messenger.Default.Subscribe<ChangeSpeedMultPayload>(OnChangeSpeedMult);
            Messenger.Default.Subscribe<PauseResumePayload>(OnNotifiedPaused);
        }

        private static void OnNotifiedPaused(PauseResumePayload payload)
        {
            Instance.Paused = payload.Paused;
        }

        private static void OnChangeSpeedMult(ChangeSpeedMultPayload t)
        {
            Instance.CurrentSpeedMultiplier = t.SpeedMultipler;
        }

        public void Reset()
        {
            CurrentSpeedMultiplier = 1;
            Paused = false;
        }
    }

    public class MenuTimeManager : MonoBehaviour
    {
        [Header("Button Action")]
        [SerializeField] Slider speedMultiplier;
        [SerializeField] Button pauseResumeButton;

        [Header("Misc Settings")]
        [SerializeField] TextMeshProUGUI currentMultDisplayTMP;
        [SerializeField] Image pauseResumeImage;
        [SerializeField] Sprite pauseSprite, resumeSprite;
        [SerializeField] float minSpeed = 1f;
        [SerializeField] float maxSpeed = 5f;
        // Start is called before the first frame update

        TaskDelay pauseToggleDelay;
        bool paused = false;

        private void Awake()
        {
            pauseToggleDelay = TaskTimer.CreateTask(0.0001f);
        }

        void Start()
        {
            speedMultiplier.minValue = minSpeed;
            speedMultiplier.maxValue = maxSpeed;

            if (speedMultiplier != null)
                speedMultiplier.onValueChanged.AddListener(UpdateCurrentMultiplier());
            if (pauseResumeButton != null)
                pauseResumeButton.onClick.AddListener(OnTogglePause);
        }

        private void OnTogglePause()
        {
            if (pauseToggleDelay.Completed())
            {
                paused = !paused;
                pauseResumeImage.sprite = !paused ? pauseSprite : resumeSprite;
                pauseToggleDelay = TaskTimer.CreateTask(0.2f, () =>
                {
                    Messenger.Default.Publish(new PauseResumePayload(paused));
                });
            }
        }

        private UnityAction<float> UpdateCurrentMultiplier()
        {
            return (float val) =>
            {
                currentMultDisplayTMP.text = Mathf.FloorToInt(val).ToString();
                Messenger.Default.Publish(new ChangeSpeedMultPayload(val));
            };
        }
    }
}