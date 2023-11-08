using Monos.WSIM.Runtime.Simulations;
using Monos.WSIM.Runtime.Waters;
using NPP.TaskTimers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Monos.WSIM.Runtime.Rains
{
    public class RainManager : MonoBehaviour
    {
        [SerializeField] WaterManager waterManager;
        [SerializeField] TextMeshProUGUI dayTimeTmp, waterLevelGainTmp, rainCategoryTmp;

        FloodSimulatorSetup sims;

        float currentHour = 0;
        float accumulateWaterHeight = 0f;
        bool dayChange = false;
        bool onRain = false;
        int curDay = 0;
        GameObject curRainObj = null;
        Dictionary<Rainfall, GameObject> rainPrefab = new Dictionary<Rainfall, GameObject>();
        string waterGainFormat;

        private void Awake()
        {
            sims = Resources.LoadAll<FloodSimulatorSetup>(GConst.SimulationSetup.FLOOD_SIM_SETUP_DIR_RUNTIME)[0];
            rainPrefab.Add(Rainfall.Clear, null);
            rainPrefab.Add(Rainfall.Light, sims.LightRainPrefab);
            rainPrefab.Add(Rainfall.Low, sims.MediumRainPrefab);
            rainPrefab.Add(Rainfall.Medium, sims.MediumRainPrefab);
            rainPrefab.Add(Rainfall.Dense, sims.HeavyRainPrefab);
            rainPrefab.Add(Rainfall.Extreme, sims.HeavyRainPrefab);
        }

        private void Start()
        {
            TaskTimer.CreateTaskLoop(sims.RealtimeSecondToHour, null, (int i) =>
            {
                dayChange = false;
                if (OkayToAdvanceDay(currentHour))
                {
                    dayChange = true;
                    currentHour = 0;
                    curDay++;
                }
                else
                    currentHour = currentHour + 1 < sims.MinHourToDay ? currentHour + 1 : sims.MinHourToDay;

                CheckTriggerRain();

                dayTimeTmp.text = $"Hour : {currentHour}, Day {curDay}";

            }, true, DelayType.Scaled);
        }

        private void CheckTriggerRain()
        {
            if (curRainObj == null)
                onRain = false;


            if (onRain) return;
            onRain = true;

            switch (sims.ChanceProc)
            {
                case FloodSimulatorSetup.RainChanceProc.OnEveryHour:
                    if (TriggerRain(out Rainfall rainfallHour))
                    {
                        SpawnRain(rainPrefab[rainfallHour], rainfallHour);
                    }
                    break;

                case FloodSimulatorSetup.RainChanceProc.OnEveryDay:
                    if (dayChange)
                    {
                        if (TriggerRain(out Rainfall rainfallDay))
                        {
                            SpawnRain(rainPrefab[rainfallDay], rainfallDay);
                        }
                    }
                    break;
            }
        }

        private bool OkayToAdvanceDay(float currentHour)
        {
            return currentHour >= sims.MinHourToDay - 1;
        }

        private bool TriggerRain(out Rainfall rain)
        {
            rain = Rainfall.Clear;
            rainCategoryTmp.text = $"Rain : {rain.ToString()}";

            float sumProbability = sims.ClearDayChance
                                   + sims.LowRainChance
                                   + sims.MediumRainChance
                                   + sims.HeavyRainChance
                                   + sims.DangerouslyDenseRainChance
                                   + sims.ExtremeRainChance;

            float[] propLoop = new float[]
            {
                sims.ClearDayChance,
                sims.LowRainChance,
                sims.MediumRainChance,
                sims.HeavyRainChance,
                sims.DangerouslyDenseRainChance,
                sims.ExtremeRainChance
            };

            int[] propName = new int[]
            {
                (int)Rainfall.Clear,
                (int)Rainfall.Light,
                (int)Rainfall.Low,
                (int)Rainfall.Medium,
                (int)Rainfall.Dense,
                (int)Rainfall.Extreme
            };

            float randomNumber = Mathf.Floor(Random.Range(0, sumProbability));

            for (int i = 0; i < propLoop.Length; i++)
            {
                if (randomNumber > 0)
                    randomNumber -= propLoop[i];
                else
                {
                    rain = (Rainfall)propName[i];
                    return true;
                }
            }

            return false;
        }

        private void SpawnRain(GameObject rain, Rainfall rainCategory)
        {
            GameObject rainObj = Instantiate(rain);
            rainCategoryTmp.text = $"Rain : {rainCategory.ToString()}";

            curRainObj = rainObj;
            float rainDurationHour = Random.Range(0, sims.RainDuration);
            float waterLevelStoppage = rainDurationHour;

            waterManager.GetMinMaxRainfallByCategory(rainCategory, out float min, out float max);
            float targetWater = waterManager.GetRainfallByMinMax(min, max);
            float targetWaterInc = targetWater;
            float accumulateTargetInc = 0;

            if (sims.ChanceProc == FloodSimulatorSetup.RainChanceProc.OnEveryHour)
                targetWater = (targetWater / sims.MinHourToDay) * rainDurationHour;

            targetWaterInc = targetWater * 0.01f;
            accumulateWaterHeight += targetWater;

            if (sims.ChanceProc == FloodSimulatorSetup.RainChanceProc.OnEveryHour)
                TaskTimer.CreateConditionalTask(0.01f, () => { return waterLevelStoppage == 0; }, (int i) =>
            {
                if (accumulateTargetInc < targetWater)
                {
                    waterLevelStoppage -= 0.01f;
                    accumulateTargetInc = Mathf.Clamp(accumulateTargetInc + targetWaterInc, 0f, targetWater);
                    waterManager.UpdateWaterLevel(targetWaterInc);
                    waterGainFormat = accumulateWaterHeight.ToString();
                    waterLevelGainTmp.text = $"Water Level : +{waterGainFormat} m";
                }

            });

            TaskTimer.CreateTask(rainDurationHour, () =>
            {
                onRain = false;
                GetAvailableParticleSystem(rainObj).ToList().ForEach(x => x.startSpeed = 0f);
                TaskTimer.CreateTask(1f, () =>
                {
                    //if (sims.ChanceProc == FloodSimulatorSetup.RainChanceProc.OnEveryDay)
                    //    waterManager.UpdateWaterLevel(Mathf.Abs(targetWater));
                    Destroy(rainObj);
                });
            });



            ParticleSystem[] GetAvailableParticleSystem(GameObject g)
            {
                ParticleSystem[] par = g.GetComponentsInChildren<ParticleSystem>();
                return par;
            }
        }
    }
}