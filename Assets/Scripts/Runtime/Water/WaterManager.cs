using KWS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monos.WSIM.Runtime.Waters
{
    public class WaterManager : MonoBehaviour
    {
        [SerializeField] WaterSystem waterSystem;
        [SerializeField] float length, width;
        [SerializeField] Rainfall rainfallCategory;
        [SerializeField] float defaultWaterDepth = 500f;
        public const float MM_DIVIDER = 1000f;

        float min, max, tempRainfall;
        float curWaterHeight;

        private void Start()
        {
            if (waterSystem != null)
            {
                curWaterHeight = waterSystem.transform.localPosition.y;
                waterSystem.MeshSize = new Vector3(length, defaultWaterDepth, width);
                waterSystem.transform.localPosition = new Vector3(0f, curWaterHeight + (CalculateHeight(CalculateVolume(GetRandomRainfallByCategory()))), 0f);
            }
        }

        //measured in mm.
        private float GetRandomRainfallByCategory(bool convert = true)
        {
            min = 0f;
            max = 1f;

            switch (rainfallCategory)
            {
                default:
                case Rainfall.Low:
                    min = 5f;
                    max = 20f;
                    break;

                case Rainfall.Medium:
                    min = 20f;
                    max = 50f;
                    break;

                case Rainfall.Heavy:
                    min = 50f;
                    max = 100f;
                    break;

                case Rainfall.DangerouslyDense:
                    min = 100f;
                    max = 150f;
                    break;

                case Rainfall.Extreme:
                    min = 151f;
                    max = 99000f;
                    break;
            }

            tempRainfall = Random.Range(min, max);

            return convert ? ConvertRainfallToMeter(tempRainfall) : tempRainfall;
        }

        private float ConvertRainfallToMeter(float rainfall)
        {
            return rainfall / MM_DIVIDER;
        }

        private float CalculateVolume(float rainfall)
        {
            return rainfall * (length * width);
        }

        private float CalculateHeight(float volume)
        {
            return volume / (length * width);
        }
    }

    public enum Rainfall
    {
        Low,
        Medium,
        Heavy,
        DangerouslyDense,
        Extreme
    }
}