using SuperMaxim.Messaging;
using TMPro;
using UnityEngine;

namespace Monos.WSIM.Runtime.UI
{
    public class RoadnameNotificator : MonoBehaviour
    {
        [System.Serializable]
        public struct RoadnameData
        {
            public string RoadName;
            public GameObject Environment;

            public RoadnameData(string roadName, GameObject environment)
            {
                RoadName = roadName;
                Environment = environment;
            }
        }

        public class RoadNamePayload
        {
            public GameObject activeEnvironment { get; }

            public RoadNamePayload(GameObject activeEnvironment)
            {
                this.activeEnvironment = activeEnvironment;
            }
        }

        [SerializeField] RoadnameData[] roadData;
        [SerializeField] TextMeshProUGUI roadNameTMP;

        private void Start()
        {
            roadNameTMP.text = roadData[0].RoadName;
            Messenger.Default.Subscribe<RoadNamePayload>(OnRoadChanges);
        }

        private void OnRoadChanges(RoadNamePayload payload)
        {
            for (int i = 0; i < roadData.Length; i++)
            {
                if (roadData[i].Environment == payload.activeEnvironment)
                {
                    roadNameTMP.text = roadData[i].RoadName;
                    break;
                }
            }
        }
    }
}