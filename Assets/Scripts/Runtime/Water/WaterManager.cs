using KWS;
using UnityEngine;

namespace Monos.WSIM.Runtime.Waters
{
    public class WaterManager : MonoBehaviour
    {
        [SerializeField] WaterSystem waterSystem;
        [SerializeField] float length, width;
        //[SerializeField] Rainfall rainfallCategory;
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
                //UpdateWaterLevel();
            }
        }


        private void UpdateWaterLevel(bool increment = true)
        {
            //waterSystem.transform.localPosition = new Vector3(
            //    0f,
            //    increment ? waterSystem.transform.localPosition.y
            //+ CalculateHeight(CalculateVolume(GetRandomRainfallByCategory())) : waterSystem.transform.localPosition.y
            //- CalculateHeight(CalculateVolume(GetRandomRainfallByCategory())),
            //    0f);
        }

        public void UpdateWaterLevel(float heightoffset, bool increment = true)
        {
            waterSystem.transform.localPosition = new Vector3(
               0f,
               increment ? waterSystem.transform.localPosition.y + heightoffset : waterSystem.transform.localPosition.y - heightoffset,
               0f);
        }

        //measured in milimeter.
        public void GetMinMaxRainfallByCategory(Rainfall category, out float min, out float max)
        {
            min = max = 0;
            switch (category)
            {
                default:
                case Rainfall.Light:
                    min = 5f;
                    max = 20f;
                    break;

                case Rainfall.Low:
                    min = 20f;
                    max = 50f;
                    break;

                case Rainfall.Medium:
                    min = 50f;
                    max = 100f;
                    break;

                case Rainfall.Dense:
                    min = 100f;
                    max = 150f;
                    break;

                case Rainfall.Extreme:
                    min = 1511f;
                    max = 10000f;
                    break;
            }
        }

        public float GetRainfallByMinMax(float min, float max, bool convertToMeter = true)
        {
            float values = Random.Range(min, max);
            return convertToMeter ? ConvertRainfallToMeter(values) : values;
        }

        //measured in milimeter.
        //private float GetRandomRainfallByCategory(bool convertToMeter = true)
        //{
        //    min = 0f;
        //    max = 1f;

        //    switch (rainfallCategory)
        //    {
        //        default:
        //        case Rainfall.Low:
        //            min = 5f;
        //            max = 20f;
        //            break;

        //        case Rainfall.Medium:
        //            min = 20f;
        //            max = 50f;
        //            break;

        //        case Rainfall.Heavy:
        //            min = 50f;
        //            max = 100f;
        //            break;

        //        case Rainfall.DangerouslyDense:
        //            min = 100f;
        //            max = 150f;
        //            break;

        //        case Rainfall.Extreme:
        //            min = 151f;
        //            max = 99000f;
        //            break;
        //    }

        //    tempRainfall = Random.Range(min, max);

        //    return convertToMeter ? ConvertRainfallToMeter(tempRainfall) : tempRainfall;
        //}

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
        Clear = 0,
        Light = 1,
        Low = 2,
        Medium = 3,
        Dense = 4,
        Extreme = 5,
    }
}