using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace NPP.TaskTimers
{
    public class AppliedEffect
    {
        public int StartIndex;
        public int EndIndex;
        public string EffInfo;
        public float[] EffValues;

        public AppliedEffect(int index, string effInfo, params float[] effValues)
        {
            this.StartIndex = index;
            this.EffInfo = effInfo;
            this.EffValues = effValues;
        }

        public AppliedEffect(int index, int endIndex, string effInfo, params float[] effValues)
        {
            this.StartIndex = index;
            this.EndIndex = endIndex;
            this.EffInfo = effInfo;
            this.EffValues = effValues;
        }
    }

    public static class TextEffectWriter
    {
        static Regex mainTextPattern = new Regex(@"<([^>]*)>");
        static Regex textEffectValuePattern = new Regex("([1-9]|[1-9]\\.,\\.[1-9])");
        static Regex textEffectPattern = new Regex("[a-zA-Z]");
        static Regex isClosingEffectPattern = new Regex(@"\/[a-zA-Z]");

        static Queue<AppliedEffect> appliedEffect = new Queue<AppliedEffect>();

        //static int charIndex = 0;

        public static void Write(TextMeshProUGUI tmpObject, string text, float characterDelay = .03f)
        {
            tmpObject.SetText(text);
            var textInfo = tmpObject.textInfo;
            tmpObject.maxVisibleCharacters = 0;

            TaskTimer.CreateConditionalTask(characterDelay, () => { return tmpObject.maxVisibleCharacters >= text.Length; },
                (int i) =>
                {
                    tmpObject.maxVisibleCharacters = i - 1 % text.Length;
                });
        }

        //example : This is <d:1> your <w:1>mission</w>.
        public static void Write(TextMeshProUGUI tmpObject, string text, TextWriterInfo infos)
        {
            Queue<AppliedEffect> appliedEffect = new Queue<AppliedEffect>();

            tmpObject.SetText(text);
            var textMesh = tmpObject.GetComponent<TMP_Text>();
            var textInfo = tmpObject.textInfo;
            int endIndexEffect = -1;
            int startIndexEffect = 0;
            int charIndex = 0;
            bool onProc = false;
            string tempText = text;
            int startIndexSearch = 0;
            string displayText = text;
            //find pattern.
            var patterns = mainTextPattern.Matches(text);
            MatchCollection hasValue = null;
            foreach (var item in patterns)
            {
                var effects = textEffectPattern.Match(item.ToString());

                switch (effects.ToString())
                {
                    case TextPatternInfo.TEXT_DELAY:
                        hasValue = textEffectValuePattern.Matches(item.ToString());
                        if (!string.IsNullOrEmpty(hasValue.ToString()))
                        {
                            //Debug.Log($"has Text Delay with value of {hasValue.Count}");
                            int startOfEffect = tempText.IndexOf(item.ToString(), startIndexSearch);
                            startIndexSearch = startOfEffect + 1;
                            tempText = tempText.Remove(startIndexSearch - 1, item.ToString().Length);

                            //Debug.Log($"indexes found {startIndexSearch},{startOfEffect} | {tempText}");

                            appliedEffect.Enqueue(new AppliedEffect(startOfEffect, TextPatternInfo.TEXT_DELAY, float.Parse(hasValue[0].ToString())));
                            displayText = tempText;
                        }
                        break;

                    case TextPatternInfo.TEXT_WOBBLE:

                        Match isClosedEffect = isClosingEffectPattern.Match(item.ToString());
                        if (!string.IsNullOrEmpty(isClosedEffect.ToString()))
                        {
                            //Debug.Log($"is closing effect, start at index {text.IndexOf(isClosedEffect.ToString())}");
                            //int startOfEffect = tempText.IndexOf(item.ToString(), startIndexSearch);
                            //startIndexSearch = startOfEffect + 1;
                            //tempText = tempText.Remove(startIndexSearch - 1, item.ToString().Length);

                            //appliedEffect.Enqueue(new AppliedEffect(startIndexEffect, startOfEffect, TextPatternInfo.TEXT_WOBBLE, float.Parse(hasValue[0].ToString()), float.Parse(hasValue[1].ToString()), float.Parse(hasValue[2].ToString())));

                            //displayText = tempText;
                            break;
                        }

                        hasValue = textEffectValuePattern.Matches(item.ToString());
                        if (!string.IsNullOrEmpty(hasValue.ToString()))
                        {
                            //Debug.Log($"has text wobble with value of {hasValue.Count}");
                            //w[0] = time,
                            //w[1] = Sin Value (x)
                            //w[2] = Cos Value (y)
                            //startIndexEffect = tempText.LastIndexOf(item.ToString(), startIndexSearch);
                            //startIndexSearch = startIndexEffect + 1;
                            //tempText = tempText.Remove(startIndexSearch - 1, item.ToString().Length);

                            //displayText = tempText;
                        }
                        break;

                }
            }

            tmpObject.SetText(displayText);

            textMesh.ForceMeshUpdate();

            var mesh = textMesh.mesh;
            var vertices = mesh.vertices;
            tmpObject.maxVisibleCharacters = 0;

            AppliedEffect app = null;
            TaskDelay waitText = TaskTimer.CreateTask(.01f);

            TaskTimer.CreateConditionalTask(infos.CharacterDelay, () => { return tmpObject.maxVisibleCharacters >= displayText.Length; }, (int i) =>
            {
                textMesh.ForceMeshUpdate();
                mesh = textMesh.mesh;
                vertices = mesh.vertices;

                if (appliedEffect.Count != 0)
                {

                    if (!onProc)
                    {
                        onProc = true;
                        app = appliedEffect.Dequeue();
                    }
                }


                if (app != null)
                {
                    if (charIndex == app.StartIndex - 1)
                    {
                        if (app.EffInfo == TextPatternInfo.TEXT_DELAY)
                        {
                            if (waitText.Completed() && onProc)
                            {
                                waitText = TaskTimer.CreateTask(app.EffValues[0], () =>
                                 {
                                     onProc = false;
                                     charIndex++;
                                 });
                            }
                        }
                        else if (app.EffInfo == TextPatternInfo.TEXT_WOBBLE)
                        {
                            if (waitText.Completed() && onProc)
                            {
                                waitText = TaskTimer.CreateTask(.1f, () =>
                                {
                                    onProc = false;
                                }, 100, (int j) =>
                                {
                                    Debug.Log($"heee {j}");
                                    var c = textInfo.characterInfo[charIndex > app.StartIndex && charIndex < app.EndIndex ? charIndex : app.StartIndex];
                                    int indexes = c.vertexIndex;

                                    Vector3 offset = CharWobble(Time.time + charIndex, float.Parse(hasValue[1].ToString()), float.Parse(hasValue[2].ToString()));
                                    vertices[indexes] += offset;
                                    vertices[indexes + 1] += offset;
                                    vertices[indexes + 2] += offset;
                                    vertices[indexes + 3] += offset;
                                    charIndex = Mathf.Clamp(charIndex + 1, 0, displayText.Length);
                                });
                            }
                        }
                    }
                    else
                    {
                        charIndex++;
                    }
                }
                else
                {
                    charIndex++;
                }

                mesh.vertices = vertices;
                textMesh.canvasRenderer.SetMesh(mesh);
                tmpObject.maxVisibleCharacters = charIndex % text.Length + 1;
            });
        }

        private static Vector2 CharWobble(float time, float xValue, float yValue)
        {
            return new Vector2(Mathf.Sin(xValue * time), Mathf.Cos(yValue * time));
        }
    }

    public struct TextWriterInfo
    {
        public float CharacterDelay;

        public TextWriterInfo(float characterDelay, string tagToCheck)
        {
            CharacterDelay = characterDelay;
        }

        public TextWriterInfo(float characterDelay)
        {
            CharacterDelay = characterDelay;
        }
    }

    public interface ITextWriterEffect { }

    public class TextPatternInfo
    {
        public const string TEXT_WOBBLE = "w";
        public const string TEXT_DELAY = "d";
    }
}
