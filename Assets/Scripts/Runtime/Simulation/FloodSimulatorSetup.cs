﻿using Monos.WSIM.Runtime.Simulations;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using UnityEngine;

namespace Monos.WSIM.Runtime.Simulations
{
    public class FloodSimulatorSetup : ScriptableObject
    {
        //Time Settings
        public float RealtimeSecondToHour = 1f;
        public float MinRealtimeSecondToHour = 0f;
        public float MaxRealtimeSecondToHour = 200f;
        public float HourToDay = 24f;
        public float MinHourToDay = 24f;
        public float MaxHourToDay = 200f;

        //Rain Settings
        public GameObject LightRainPrefab, MediumRainPrefab, HeavyRainPrefab;
        public int RainDuration = 1;
        public int MinRainDuration = 1;
        public int MaxRainDuration = 24;

        public float ClearDayChance = 33f;
        public float LowRainChance = 44f;
        public float MediumRainChance = 33f;
        public float HeavyRainChance = 22f;
        public float DangerouslyDenseRainChance = 22f;
        public float ExtremeRainChance = 11f;

        public RainChanceProc ChanceProc;

        public enum RainChanceProc
        {
            OnEveryHour = 0,
            OnEveryDay = 1
        }
    }
}

#region EDITOR ONLY
namespace Monos.WSIM.Editors.Simulations
{
#if UNITY_EDITOR
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

            public virtual void DrawSliderIntField(SerializedProperty prop, int min, int max, string labelName, float labelWidth = 140, float propFieldWidth = 120)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labelName, GUILayout.Width(labelWidth));
                prop.intValue = (int)EditorGUILayout.Slider(prop.intValue, min, max, GUILayout.Width(propFieldWidth));
                EditorGUILayout.EndHorizontal();
            }

            public virtual void DrawObjectField(SerializedProperty prop, float width)
            {
                EditorGUILayout.ObjectField(prop, GUILayout.Width(350));
            }

            public virtual void DrawLabelField(string labelDisplay, float width = 120, float height = 20)
            {
                EditorGUILayout.LabelField(labelDisplay, EditorStyles.boldLabel, GUILayout.Width(width), GUILayout.Height(height));
            }

            public virtual void DrawEnumField(SerializedProperty prop, string enumLabel, EnumDescStruct[] enumDesc, float labelWidth = 120, float labelHeight = 20, float width = 140, float height = 30)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(FindCorrectEnum(prop.enumValueIndex), EditorStyles.helpBox, GUILayout.Width(300), GUILayout.Height(20));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(enumLabel, GUILayout.Width(labelWidth), GUILayout.Height(labelHeight));
                EditorGUILayout.PropertyField(prop, GUIContent.none, GUILayout.Width(width), GUILayout.Height(height));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                string FindCorrectEnum(int index)
                {
                    for (int i = 0; i < enumDesc.Length; i++)
                    {
                        if (enumDesc[i].EnumIndex == index)
                            return enumDesc[i].EnumDesc;
                    }
                    return "";
                }
            }

            public struct EnumDescStruct
            {
                public int EnumIndex;
                public string EnumDesc;

                public EnumDescStruct(int enumIndex, string enumDesc)
                {
                    EnumIndex = enumIndex;
                    EnumDesc = enumDesc;
                }
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
                GUILayout.Label("Simulation time setup, how many second in realtime shall pass within in-game hour(s), how many hour for a day, etc.", EditorStyles.largeLabel);

                EditorGUILayout.BeginVertical("box");
                GUILayout.Space(10);
                DrawLabelField("[In-Game Time]");
                DrawSliderField(RealtimeSecondToHourProp, MinRealtimeSecondToHourProp.floatValue, MaxRealtimeSecondToHourProp.floatValue, "Real-time seconds to Hour", 160, 140);
                DrawSliderField(HourToDayProp, MinHourToDayProp.floatValue, MaxHourToDayProp.floatValue, "Day Length (Hours)", 160, 140);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
        }

        public class RainSettingsModal : FloodSimulationModalBase
        {
            SerializedProperty LightRainPrefabProp, MediumRainPrefabProp, HeavyRainPrefabProp;
            SerializedProperty ClearDayChanceProp, LowRainChanceProp, MediumRainChanceProp, HeavyRainChanceProp, DangerouslyDenseRainChanceProp, ExtremeRainChanceProp;
            SerializedProperty ChanceProcProp;
            SerializedProperty RainDurationProp, MinRainDurationProp, MaxRainDurationProp;

            public RainSettingsModal(SerializedObject FloodSimObj) : base(FloodSimObj)
            {
                LightRainPrefabProp ??= FloodSim.FindProperty("LightRainPrefab");
                MediumRainPrefabProp ??= FloodSim.FindProperty("MediumRainPrefab");
                HeavyRainPrefabProp ??= FloodSim.FindProperty("HeavyRainPrefab");
                ClearDayChanceProp ??= FloodSim.FindProperty("ClearDayChance");
                LowRainChanceProp ??= FloodSim.FindProperty("LowRainChance");
                MediumRainChanceProp ??= FloodSim.FindProperty("MediumRainChance");
                HeavyRainChanceProp ??= FloodSim.FindProperty("HeavyRainChance");
                DangerouslyDenseRainChanceProp ??= FloodSim.FindProperty("DangerouslyDenseRainChance");
                ExtremeRainChanceProp ??= FloodSim.FindProperty("ExtremeRainChance");
                ChanceProcProp ??= FloodSim.FindProperty("ChanceProc");
                RainDurationProp ??= FloodSim.FindProperty("RainDuration");
                MinRainDurationProp ??= FloodSim.FindProperty("MinRainDuration");
                MaxRainDurationProp ??= FloodSim.FindProperty("MaxRainDuration");
            }

            protected override void DrawModal()
            {
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Setup for rain simulation.", EditorStyles.largeLabel);

                EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
                GUILayout.Space(10);
                DrawLabelField("[Rain Prefabs]");
                DrawObjectField(LightRainPrefabProp, 350f);
                DrawObjectField(MediumRainPrefabProp, 350f);
                DrawObjectField(HeavyRainPrefabProp, 350f);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
                GUILayout.Space(10);
                DrawLabelField("[Rain Chance]");
                DrawSliderField(ClearDayChanceProp, 0, 100, "Clear Day Chance", 160, 140);
                DrawSliderField(LowRainChanceProp, 0, 100, "Low Rain Chance", 160, 140);
                DrawSliderField(MediumRainChanceProp, 0, 100, "Medium Rain Chance", 160, 140);
                DrawSliderField(HeavyRainChanceProp, 0, 100, "Heavy Rain Chance", 160, 140);
                DrawSliderField(DangerouslyDenseRainChanceProp, 0, 100, "Dense Rain Chance", 160, 140);
                DrawSliderField(ExtremeRainChanceProp, 0, 100, "Extreme Rain Chance", 160, 140);
                GUILayout.Space(10);
                DrawLabelField("[Rain Behaviour]", 140);
                DrawEnumField(ChanceProcProp, "Procs Behaviour", new EnumDescStruct[]
                {
                    new EnumDescStruct(0, "Rain calculated every hour passed."),
                    new EnumDescStruct(1, "Rain calculated on each day pass.")
                });
                DrawSliderIntField(RainDurationProp, MinRainDurationProp.intValue, MaxRainDurationProp.intValue, "Average Rain Duration (floor to Int)", 160f, 140f);
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
#endif
    public enum FloodMenuEnum
    {
        Time = 0,
        Rain = 1,
        Flood = 2,
        Drainage = 3,
        None = -99
    }
}
#endregion