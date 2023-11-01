using Monos.WSIM.Runtime.Simulations;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager.UI;
#endif
using UnityEngine;
using UnityEngine.Windows;

namespace Monos.WSIM.Debugs
{
    public static class WaterSimDebug
    {
#if UNITY_EDITOR
        [MenuItem("Flood Sim/Game/Setup Simulation")]
        private static void SetupWaterSim()
        {
            var sim = ScriptableObject.CreateInstance<FloodSimulatorSetup>();
            if (!System.IO.Directory.Exists(GConst.SimulationSetup.FLOOD_SIM_SETUP_DIR))
                System.IO.Directory.CreateDirectory(GConst.SimulationSetup.FLOOD_SIM_SETUP_DIR);


            if (System.IO.File.Exists(Path.Combine(GConst.SimulationSetup.FLOOD_SIM_SETUP_DIR, $"SimulationSetup.asset")))
            {
                var t = AssetDatabase.LoadAssetAtPath<FloodSimulatorSetup>(Path.Combine(GConst.SimulationSetup.FLOOD_SIM_SETUP_DIR, $"SimulationSetup.asset"));
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = t;
                return;
            }

            AssetDatabase.CreateAsset(sim, Path.Combine(GConst.SimulationSetup.FLOOD_SIM_SETUP_DIR, $"SimulationSetup.asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = sim;


        }
#endif
    }
}

public class GConst
{
    public class SimulationSetup
    {
        public const string FLOOD_SIM_SETUP_DIR = "Assets/Resources/SimData";
        public const string FLOOD_SIM_SETUP_DIR_RUNTIME = "SimData";
    }

    public class RainSys
    {
        public const string LIGHT_RAIN = "Low";
        public const string MEDIUM_RAIN = "Medium";
        public const string HEAVY_RAIN = "Heavy";
        public const string CLEAR_SKY = "Clear";
    }
}