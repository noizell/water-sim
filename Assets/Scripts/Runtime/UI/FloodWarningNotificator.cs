using DG.Tweening;
using SuperMaxim.Messaging;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Monos.WSIM.Runtime.UI
{
    public class FloodWarningNotificator : MonoBehaviour
    {
        [Header("Main Setup")]
        [SerializeField] TextMeshProUGUI displayWarningTMP;
        [SerializeField] FloodWarningStruct[] warningSetup;

        float waterLevel = 0f;

        private void Start()
        {
            //order by lowest value.
            warningSetup = warningSetup.ToList().OrderBy(x => x.MaxHeight).ToArray();

            Messenger.Default.Subscribe<WaterLevelPayload>(OnWaterLevelChanged);
        }

        private void OnWaterLevelChanged(WaterLevelPayload payload)
        {
            waterLevel = payload.currentWaterLevel;

            for (int i = 0; i < warningSetup.Length; i++)
            {
                if (waterLevel < warningSetup[i].MaxHeight)
                {
                    displayWarningTMP.DOColor(warningSetup[i].WarningDisplayColor, 1.4f);
                    displayWarningTMP.text = $"{warningSetup[i].WarningDisplay}";
                    break;
                }

            }

        }
    }

    public class WaterLevelPayload
    {
        public float currentWaterLevel { get; }

        public WaterLevelPayload(float currentWaterLevel)
        {
            this.currentWaterLevel = currentWaterLevel;
        }
    }

    [System.Serializable]
    public struct FloodWarningStruct
    {
        public string WarningDisplay;
        public float MaxHeight;
        public Color WarningDisplayColor;

        public FloodWarningStruct(string warningDisplay, float maxHeight, Color warningDisplayColor)
        {
            WarningDisplay = warningDisplay;
            MaxHeight = maxHeight;
            WarningDisplayColor = warningDisplayColor;
        }
    }
}