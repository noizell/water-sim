using Cysharp.Threading.Tasks;
using Monos.WSIM.Runtime.Simulations;
using Monos.WSIM.Runtime.UI;
using Monos.WSIM.Runtime.Waters;
using NPP.TaskTimers;
using SuperMaxim.Messaging;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Monos.WSIM.Runtime.Rains
{

    public class RainManager : MonoBehaviour
    {
        [SerializeField] WaterManager waterManager;
        [SerializeField] TextMeshProUGUI dayTimeTmp, waterLevelGainTmp, rainCategoryTmp;
        [SerializeField] int startingRainCount = 5;

        FloodSimulatorSetup sims;

        float currentHour = 0;
        float accumulateWaterHeight = 0f;
        bool dayChange = false;
        bool onRain = false;
        int curDay = 0;
        GameObject curRainObj = null;
        Dictionary<Rainfall, GameObject> rainPrefab = new Dictionary<Rainfall, GameObject>();
        string waterGainFormat;

        TaskDelay simulationWaiter;

        List<TaskDelay> rainWatchers = new List<TaskDelay>();

        Dictionary<Rainfall, List<RainParticleController>> cachedRain = new Dictionary<Rainfall, List<RainParticleController>>();

        private void Awake()
        {
            sims = Resources.LoadAll<FloodSimulatorSetup>(GConst.SimulationSetup.FLOOD_SIM_SETUP_DIR_RUNTIME)[0];
            rainPrefab.Add(Rainfall.Clear, null);
            rainPrefab.Add(Rainfall.Light, sims.LightRainPrefab);
            rainPrefab.Add(Rainfall.Low, sims.MediumRainPrefab);
            rainPrefab.Add(Rainfall.Medium, sims.MediumRainPrefab);
            rainPrefab.Add(Rainfall.Dense, sims.HeavyRainPrefab);
            rainPrefab.Add(Rainfall.Extreme, sims.HeavyRainPrefab);

            CacheRains();

            simulationWaiter = TaskTimer.CreateTask(0.00001f);
        }

        private void CacheRains()
        {
            cachedRain.Add(Rainfall.Clear, new List<RainParticleController>());
            cachedRain.Add(Rainfall.Light, new List<RainParticleController>());
            cachedRain.Add(Rainfall.Low, new List<RainParticleController>());
            cachedRain.Add(Rainfall.Medium, new List<RainParticleController>());
            cachedRain.Add(Rainfall.Dense, new List<RainParticleController>());
            cachedRain.Add(Rainfall.Extreme, new List<RainParticleController>());

            for (int i = 0; i < startingRainCount; i++)
            {
                cachedRain[Rainfall.Clear].Add(null);
                cachedRain[Rainfall.Light].Add(Instantiate(rainPrefab[Rainfall.Light]).GetComponent<RainParticleController>());
                cachedRain[Rainfall.Low].Add(Instantiate(rainPrefab[Rainfall.Low]).GetComponent<RainParticleController>());
                cachedRain[Rainfall.Medium].Add(Instantiate(rainPrefab[Rainfall.Medium]).GetComponent<RainParticleController>());
                cachedRain[Rainfall.Dense].Add(Instantiate(rainPrefab[Rainfall.Dense]).GetComponent<RainParticleController>());
                cachedRain[Rainfall.Extreme].Add(Instantiate(rainPrefab[Rainfall.Extreme]).GetComponent<RainParticleController>());
            }
            for (int i = 0; i < startingRainCount; i++)
            {
                //cachedRain[Rainfall.Light][i].SetActive(false);
                //cachedRain[Rainfall.Low][i].SetActive(false);
                //cachedRain[Rainfall.Medium][i].SetActive(false);
                //cachedRain[Rainfall.Dense][i].SetActive(false);
                //cachedRain[Rainfall.Extreme][i].SetActive(false);

                cachedRain[Rainfall.Light][i].ToggleActive(false);
                cachedRain[Rainfall.Low][i].ToggleActive(false);
                cachedRain[Rainfall.Medium][i].ToggleActive(false);
                cachedRain[Rainfall.Dense][i].ToggleActive(false);
                cachedRain[Rainfall.Extreme][i].ToggleActive(false);
            }
        }

        private RainParticleController FindAvailableOrSpawnNewRain(Rainfall rain)
        {
            if (rain == Rainfall.Clear) return null;
            var r = cachedRain[rain].Find(x => !x.Active);
            if (r == null)
            {
                var rainP = Instantiate(rainPrefab[rain].GetComponent<RainParticleController>());
                rainP.ToggleActive(false);
                cachedRain[rain].Add(rainP);
                return cachedRain[rain].Find(x => !x.Active);
            }

            return r;
        }

        private void UpdateSimulationTime(System.Action onUpdate)
        {
            if (simulationWaiter.Completed())
            {
                simulationWaiter = TaskTimer.CreateTask(sims.RealtimeSecondToHour / TimeController.Instance.CurrentSpeedMultiplier, () =>
                {
                    UpdateSimulationTime(onUpdate);
                    if (TimeController.Instance.Paused) return;
                    onUpdate?.Invoke();
                });
            }
        }

        private void Start()
        {
            //allowing for dynamically adjust speed mult.
            if (simulationWaiter.Completed())
            {
                simulationWaiter = TaskTimer.CreateTask(sims.RealtimeSecondToHour / TimeController.Instance.CurrentSpeedMultiplier, () =>
                {
                    UpdateSimulationTime(AdvanceTime);
                    if (TimeController.Instance.Paused) return;
                    AdvanceTime();
                });
            }

            //for pausing/resuming rain
            TaskTimer.CreateTaskLoop(0.001f, null, (int i) =>
            {
                if (TimeController.Instance.Paused)
                    PauseRain(true);
                else
                    PauseRain(false);

                void PauseRain(bool pause)
                {
                    for (int j = 0; j < rainWatchers.Count; j++)
                    {
                        if (rainWatchers[j].Completed()) continue;
                        rainWatchers[j].Pause(pause);
                    }
                }
            });

            //periodically remove completed rain watcher.
            TaskTimer.CreateTaskLoop(2f, null, (int iv) =>
            {
                for (global::System.Int32 i = rainWatchers.Count - (1); i >= 0; i--)
                {
                    if (rainWatchers[i].Completed())
                        rainWatchers.RemoveAt(i);
                }
            });

            void AdvanceTime()
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
            }
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

        float[] propLoop;
        int[] propName;
        float sumProbability, randomNumber;
        private bool TriggerRain(out Rainfall rain)
        {
            rain = Rainfall.Clear;
            rainCategoryTmp.text = $"Rain : {rain.ToString()}";

            sumProbability = sims.ClearDayChance
                                   + sims.LowRainChance
                                   + sims.MediumRainChance
                                   + sims.HeavyRainChance
                                   + sims.DangerouslyDenseRainChance
                                   + sims.ExtremeRainChance;

            propLoop = new float[]
            {
                sims.ClearDayChance,
                sims.LowRainChance,
                sims.MediumRainChance,
                sims.HeavyRainChance,
                sims.DangerouslyDenseRainChance,
                sims.ExtremeRainChance
            };

            propName = new int[]
            {
                (int)Rainfall.Clear,
                (int)Rainfall.Light,
                (int)Rainfall.Low,
                (int)Rainfall.Medium,
                (int)Rainfall.Dense,
                (int)Rainfall.Extreme
            };

            randomNumber = Mathf.Floor(Random.Range(0, sumProbability));

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
            //GameObject rainObj = Instantiate(rain);
            var rainObj = FindAvailableOrSpawnNewRain(rainCategory);
            rainObj.ToggleActive(true);
            rainCategoryTmp.text = $"Rain : {rainCategory.ToString()}";

            curRainObj = rainObj.gameObject;
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

            }, () =>
            {
                Messenger.Default.Publish(new WaterLevelPayload(accumulateWaterHeight));
            });

            rainWatchers.Add(TaskTimer.CreateTask(rainDurationHour, () =>
            {
                onRain = false;
                if (rainObj == null) return;
                //GetAvailableParticleSystem(rainObj).ToList().ForEach(x => x.startSpeed = 0f);
                TaskTimer.CreateTask(0.1f, async () =>
                {
                    await UniTask.WaitUntil(() => !TimeController.Instance.Paused);
                    rainObj.ToggleActive(false);
                    curRainObj = null;
                });
            }));

            ParticleSystem[] GetAvailableParticleSystem(GameObject g)
            {
                if (g == null) return null;
                ParticleSystem[] par = g.GetComponentsInChildren<ParticleSystem>();
                return par;
            }
        }
    }
}