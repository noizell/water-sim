using Monos.WSIM.Runtime.Simulations;
using NPP.TaskTimers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monos.WSIM.Runtime.Rains
{
    public class RainManager : MonoBehaviour
    {
        FloodSimulatorSetup sims;

        float currentHour = 0;
        float dayLength = 0;

        int curDay = 0;

        private void Awake()
        {
            sims = Resources.LoadAll<FloodSimulatorSetup>(GConst.SimulationSetup.FLOOD_SIM_SETUP_DIR_RUNTIME)[0];
        }

        private void Start()
        {
            TaskTimer.CreateTaskLoop(sims.RealtimeSecondToHour, null, (int i) =>
            {
                if (OkayToAdvanceDay(currentHour))
                {
                    currentHour = 0;
                    curDay++;
                }
                else
                    currentHour = currentHour + 1 < sims.MinHourToDay ? currentHour + 1 : sims.MinHourToDay;

            }, true, DelayType.Realtime);
        }

        private bool OkayToAdvanceDay(float currentHour)
        {
            return currentHour >= sims.MinHourToDay;
        }
    }
}