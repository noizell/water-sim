using Monos.WSIM.Runtime.Simulations;
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Monos.WSIM.Runtime.Simulations
{
    public class FloodSimulatorSetup : ScriptableObject
    {
        //Time Settings
        [SerializeField] public float RealtimeSecondToHour = 1f;
        [SerializeField] public float MinRealtimeSecondToHour = 0f;
        [SerializeField] public float MaxRealtimeSecondToHour = 200f;
        [SerializeField] public float HourToDay = 24f;
        [SerializeField] public float MinHourToDay = 24f;
        [SerializeField] public float MaxHourToDay = 200f;

        //Rain Settings
        [SerializeField] public GameObject LightRainPrefab, MediumRainPrefab, HeavyRainPrefab;
        [SerializeField] public float RainDuration;
        [SerializeField] public float MinRainDuration = 1f;
        [SerializeField] public float MaxRainDuration = 24f;

        [SerializeField] public float ClearDayChance = 33f;
        [SerializeField] public float LowRainChance = 44f;
        [SerializeField] public float MediumRainChance = 33f;
        [SerializeField] public float HeavyRainChance = 22f;
        [SerializeField] public float DangerouslyDenseRainChance = 22f;
        [SerializeField] public float ExtremeRainChance = 11f;
    }
}

namespace Monos.WSIM.Editors.Simulations
{
    public class AssetHandler
    {
        [OnOpenAsset()]
        public static bool OpenEditor(int instanceId, int line)
        {
            FloodSimulatorSetup obj = EditorUtility.InstanceIDToObject(instanceId) as FloodSimulatorSetup;
            if (obj != null)
            {
                FloodSimulationEditorWindow.OpenWindow(obj);
            }
            return false;
        }
    }

    [CustomEditor(typeof(FloodSimulatorSetup))]
    public class FloodSimulatorSetupEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Simulation Panel", GUILayout.Width(200)))
            {
                FloodSimulationEditorWindow.OpenWindow((FloodSimulatorSetup)target);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }

    [CustomEditor(typeof(FloodSimulatorSetup))]
    public class FloodSimulationEditorWindow : EditorWindow
    {
        static FloodSimulationEditorWindow window;
        SerializedObject FloodSimObj;

        FloodSimulationMainModal mainModal;
        FloodSimulationMainModal.WaterDrainageSettingsModal drainageModal;
        FloodSimulationMainModal.FloodWarningSettingsModal floodWarningModal;
        FloodSimulationMainModal.RainSettingsModal rainModal;
        FloodSimulationMainModal.TimeSettingsModal timeModal;

        FloodMenuEnum curMenu;

        public static void OpenWindow(FloodSimulatorSetup obj)
        {
            window = GetWindow<FloodSimulationEditorWindow>("Simulation Setup");
            window.FloodSimObj = new SerializedObject(obj);
            window.maxSize = new Vector2(800, 625);
            window.minSize = new Vector2(800, 625);
        }

        private void OnGUI()
        {
            mainModal ??= new FloodSimulationMainModal();

            EditorGUILayout.BeginVertical();
            mainModal.DrawMainView(out FloodMenuEnum curMenu);
            DrawCorrectMenuModal(curMenu);
            EditorGUILayout.EndVertical();

            FloodSimObj.ApplyModifiedProperties();
        }

        private void DrawCorrectMenuModal(FloodMenuEnum curMenu)
        {
            switch (curMenu)
            {
                case FloodMenuEnum.Time:
                    this.curMenu = curMenu;
                    timeModal ??= new FloodSimulationMainModal.TimeSettingsModal(FloodSimObj);
                    timeModal.Draw();
                    break;

                case FloodMenuEnum.Rain:
                    this.curMenu = curMenu;
                    rainModal ??= new FloodSimulationMainModal.RainSettingsModal(FloodSimObj);
                    rainModal.Draw();
                    break;

                case FloodMenuEnum.Flood:
                    this.curMenu = curMenu;
                    floodWarningModal ??= new FloodSimulationMainModal.FloodWarningSettingsModal(FloodSimObj);
                    floodWarningModal.Draw();
                    break;

                case FloodMenuEnum.Drainage:
                    this.curMenu = curMenu;
                    drainageModal ??= new FloodSimulationMainModal.WaterDrainageSettingsModal(FloodSimObj);
                    drainageModal.Draw();
                    break;

                default:
                case FloodMenuEnum.None:
                    switch (this.curMenu)
                    {
                        case FloodMenuEnum.Time:
                            timeModal ??= new FloodSimulationMainModal.TimeSettingsModal(FloodSimObj);
                            timeModal.Draw();
                            break;

                        case FloodMenuEnum.Rain:
                            rainModal ??= new FloodSimulationMainModal.RainSettingsModal(FloodSimObj);
                            rainModal.Draw();
                            break;

                        case FloodMenuEnum.Flood:
                            floodWarningModal ??= new FloodSimulationMainModal.FloodWarningSettingsModal(FloodSimObj);
                            floodWarningModal.Draw();
                            break;

                        case FloodMenuEnum.Drainage:
                            drainageModal ??= new FloodSimulationMainModal.WaterDrainageSettingsModal(FloodSimObj);
                            drainageModal.Draw();
                            break;
                    }
                    break;
            }
        }
    }
    public class FloodSimulationMainModal
    {
        public abstract class FloodSimulationModalBase
        {
            protected SerializedObject FloodSim;

            public FloodSimulationModalBase(SerializedObject FloodSimObj)
            {
                FloodSim = FloodSimObj;
            }

            public virtual void Draw()
            {
                GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Height(520));
                DrawModal();
                GUILayout.EndHorizontal();
            }

            protected abstract void DrawModal();

            public virtual void DrawFloatField(SerializedProperty prop, string labelName, float labelWidth = 120, float propFieldWidth = 100)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labelName, GUILayout.Width(labelWidth));
                prop.floatValue = EditorGUILayout.FloatField(prop.floatValue, EditorStyles.numberField, GUILayout.Width(propFieldWidth));
                EditorGUILayout.EndHorizontal();
            }

            public virtual void DrawSliderField(SerializedProperty prop, float min, float max, string labelName, float labelWidth = 120, float propFieldWidth = 100)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labelName, GUILayout.Width(labelWidth));
                prop.floatValue = EditorGUILayout.Slider(prop.floatValue, min, max, GUILayout.Width(propFieldWidth));
                EditorGUILayout.EndHorizontal();
            }

            public virtual void DrawObjectField(SerializedProperty prop, float width)
            {
                EditorGUILayout.ObjectField(prop, GUILayout.Width(350));
            }

        }

        public class TimeSettingsModal : FloodSimulationModalBase
        {
            protected SerializedProperty RealtimeSecondToHourProp, MinRealtimeSecondToHourProp, MaxRealtimeSecondToHourProp, HourToDayProp, MinHourToDayProp, MaxHourToDayProp;

            public TimeSettingsModal(SerializedObject FloodSimObj) : base(FloodSimObj)
            {
                RealtimeSecondToHourProp ??= FloodSim.FindProperty("RealtimeSecondToHour");
                MinRealtimeSecondToHourProp ??= FloodSim.FindProperty("MinRealtimeSecondToHour");
                MaxRealtimeSecondToHourProp ??= FloodSim.FindProperty("MaxRealtimeSecondToHour");
                HourToDayProp ??= FloodSim.FindProperty("HourToDay");
                MinHourToDayProp ??= FloodSim.FindProperty("MinHourToDay");
                MaxHourToDayProp ??= FloodSim.FindProperty("MaxHourToDay");
            }


            protected override void DrawModal()
            {
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Please set time for simulation, how many second in realtime shall pass within in-game hour(s), how many hour for a day, etc.", EditorStyles.helpBox);

                EditorGUILayout.BeginVertical("box");
                GUILayout.Space(10);
                EditorGUILayout.LabelField("[In-Game Time]", EditorStyles.boldLabel);
                DrawSliderField(RealtimeSecondToHourProp, MinRealtimeSecondToHourProp.floatValue, MaxRealtimeSecondToHourProp.floatValue, "Real-time seconds to Hour", 160, 140);
                DrawSliderField(HourToDayProp, MinHourToDayProp.floatValue, MaxHourToDayProp.floatValue, "Day Length (Hours)", 160, 140);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
        }

        public class RainSettingsModal : FloodSimulationModalBase
        {
            SerializedProperty LightRainPrefabProp, MediumRainPrefabProp, HeavyRainPrefabProp;

            public RainSettingsModal(SerializedObject FloodSimObj) : base(FloodSimObj)
            {
                LightRainPrefabProp ??= FloodSim.FindProperty("LightRainPrefab");
                MediumRainPrefabProp ??= FloodSim.FindProperty("MediumRainPrefab");
                HeavyRainPrefabProp ??= FloodSim.FindProperty("HeavyRainPrefab");
            }

            protected override void DrawModal()
            {
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Setup for rain simulation.", EditorStyles.helpBox);

                EditorGUILayout.BeginVertical("box");
                GUILayout.Space(10);
                DrawObjectField(LightRainPrefabProp, 350f);
                DrawObjectField(MediumRainPrefabProp, 350f);
                DrawObjectField(HeavyRainPrefabProp, 350f);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
        }

        public class WaterDrainageSettingsModal : FloodSimulationModalBase
        {
            public WaterDrainageSettingsModal(SerializedObject FloodSimObj) : base(FloodSimObj)
            {
            }

            protected override void DrawModal()
            {

            }
        }

        public class FloodWarningSettingsModal : FloodSimulationModalBase
        {
            public FloodWarningSettingsModal(SerializedObject FloodSimObj) : base(FloodSimObj)
            {
            }

            protected override void DrawModal()
            {

            }
        }

        public FloodSimulationMainModal()
        {

        }

        public void DrawMainView(out FloodMenuEnum selected)
        {
            selected = FloodMenuEnum.None;

            GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true), GUILayout.Height(90));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Time Settings", GUILayout.Height(70), GUILayout.Width(170)))
                selected = FloodMenuEnum.Time;
            if (GUILayout.Button("Rain Settings", GUILayout.Height(70), GUILayout.Width(170)))
                selected = FloodMenuEnum.Rain;
            if (GUILayout.Button("Water Drainage Settings", GUILayout.Height(70), GUILayout.Width(170)))
                selected = FloodMenuEnum.Drainage;
            if (GUILayout.Button("Flood Warning Settings", GUILayout.Height(70), GUILayout.Width(170)))
                selected = FloodMenuEnum.Flood;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

    public enum FloodMenuEnum
    {
        Time = 0,
        Rain = 1,
        Flood = 2,
        Drainage = 3,
        None = -99
    }
}