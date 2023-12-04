using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Monos.WSIM.Runtime.Cameras
{
    public class CinematicManager : MonoBehaviour
    {
        [SerializeField] GameObject[] regions;
        [SerializeField] Button changeViewButton;
        [SerializeField] Button changeCameraModeButton;
        [SerializeField] CanvasGroup fader;

        [SerializeField] CinemachineBrain mainCameraBrain;
        int currentRegionId = 0;

        private void Start()
        {
            fader.DOFade(1f, 0.3f).OnComplete(() =>
            {
                fader.DOFade(0f, 1f);
            });

            changeViewButton.onClick.AddListener(() => { SwapRegion(); });
            changeCameraModeButton.onClick.AddListener(() => { ChangeCameraMode(); });
        }

        private void ChangeCameraMode()
        {
            fader.DOFade(1f, 0.02f).OnComplete(() =>
            {
                fader.DOFade(0f, 0.07f);

                mainCameraBrain.enabled = !mainCameraBrain.enabled;
            });
        }

        public void SwapRegion()
        {
            fader.DOFade(1f, 0.3f).OnComplete(() =>
            {
                fader.DOFade(0f, 0.3f);

                for (int i = 0; i < regions.Length; i++)
                    regions[i].SetActive(false);

                currentRegionId = currentRegionId + 1 < regions.Length ? currentRegionId + 1 : 0;
                regions[currentRegionId].SetActive(true);
            });
        }

    }
}
