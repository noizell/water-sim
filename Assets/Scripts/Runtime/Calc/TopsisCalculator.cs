using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TopsisCalculator
{
    public class AlternativeElementData
    {
        public float CriteriaValue;
        public float Weight;
        public bool IsBenefit;
        public float Divider;
        public float NormalizedValue;
        public float WeightedNormalizedValue;
        public float IdealPositive;
        public float IdealNegative;

        public AlternativeElementData(float criteriaValue, float weighted, bool cost)
        {
            CriteriaValue = criteriaValue;
            Weight = weighted;
            IsBenefit = cost;
        }
    }

    public class CalculatedData
    {
        public float DiffPositive;
        public float DiffNegative;
        public float PreferentialValue;
        public int Rank;

        public CalculatedData(float diffPositive, float diffNegative)
        {
            DiffPositive = diffPositive;
            DiffNegative = diffNegative;
            PreferentialValue = 0;
        }
    }

    public class RankedData
    {
        public float PreferentialValue;
        public float Rank;

        public RankedData(float prefValue)
        {
            PreferentialValue = prefValue;
        }
    }

    public List<AlternativeElementData[]> AltMatrix;
    public List<CalculatedData> Differential;

    readonly List<float> divider;
    float[] positiveIdealSolution, negativeIdealSolution;

    public TopsisCalculator()
    {
        AltMatrix = new List<AlternativeElementData[]>();
        Differential = new List<CalculatedData>();
        divider = new List<float>();
    }

    public void AddAlternative(params AlternativeElementData[] altVal)
    {
        AltMatrix.Add(altVal);
    }

    public void ClearData()
    {
        AltMatrix = new List<AlternativeElementData[]>();
    }

    public void Normalize()
    {
        float calcPow = 0f;

        //helper for calculate divider, assuming that matrix has square value, ex [3,3];[5,5];etc
        for (int j = 0; j < AltMatrix[0].Length; j++)
        {
            for (int i = 0; i < AltMatrix.Count; i++)
                calcPow += Mathf.Pow(AltMatrix[i][j].CriteriaValue, 2f);

            divider.Add(Mathf.Sqrt(calcPow));
            calcPow = 0;
        }

        //actual calculation.
        for (int i = 0; i < AltMatrix.Count; i++)
        {
            for (int j = 0; j < AltMatrix[i].Length; j++)
            {
                AltMatrix[i][j].Divider = divider[j];
                AltMatrix[i][j].NormalizedValue = AltMatrix[i][j].CriteriaValue / AltMatrix[i][j].Divider;
                AltMatrix[i][j].WeightedNormalizedValue = AltMatrix[i][j].NormalizedValue * AltMatrix[i][j].Weight;
            }
        }
    }

    public void CalculateIdealSolution()
    {
        //calculate helper for positive ideal solution (PIS)  & negative ideal solution (NIS) 
        //Positive Ideal Solution = get max value of current criteria in all alternative if criteria are flagged as 'benefit', otherwise, get min value.
        //Negative Ideal Solution = get min value of current criteria in all alternative if criteria are flagged as 'benefit', otherwise, get max value.

        List<float> columnValue = new List<float>();
        List<List<float>> columnValCollection = new List<List<float>>();

        for (int j = 0; j < AltMatrix[0].Length; j++)
        {
            for (int i = 0; i < AltMatrix.Count; i++)
            {
                columnValue.Add(AltMatrix[i][j].WeightedNormalizedValue);
            }
            columnValCollection.Add(columnValue);
            columnValue = new List<float>();
        }

        for (int j = 0; j < AltMatrix.Count; j++)
        {
            for (int i = 0; i < AltMatrix[j].Length; i++)
            {
                AltMatrix[j][i].IdealPositive = AltMatrix[j][i].IsBenefit ? Mathf.Max(columnValCollection[i].ToArray()) : Mathf.Min(columnValCollection[i].ToArray());
                AltMatrix[j][i].IdealNegative = AltMatrix[j][i].IsBenefit ? Mathf.Min(columnValCollection[i].ToArray()) : Mathf.Max(columnValCollection[i].ToArray());
            }
        }
    }

    public void CalculateDiffIdealSolution()
    {
        float calPositivePow = 0f;
        float calNegativePow = 0f;

        float[] diffPositive = new float[AltMatrix.Count];
        float[] diffNegative = new float[AltMatrix.Count];

        for (int i = 0; i < AltMatrix.Count; i++)
        {
            for (int j = 0; j < AltMatrix[i].Length; j++)
            {
                calPositivePow += Mathf.Pow(AltMatrix[i][j].IdealPositive - AltMatrix[i][j].WeightedNormalizedValue, 2f);
                calNegativePow += Mathf.Pow(AltMatrix[i][j].WeightedNormalizedValue - AltMatrix[i][j].IdealNegative, 2f);
            }

            diffPositive[i] = Mathf.Sqrt(calPositivePow);
            diffNegative[i] = Mathf.Sqrt(calNegativePow);

            calPositivePow = 0;
            calNegativePow = 0;

            Differential.Add(new CalculatedData(diffPositive[i], diffNegative[i]));
        }

    }

    public void CalculatePreferencesValue()
    {
        for (int i = 0; i < Differential.Count; i++)
        {
            Differential[i].PreferentialValue = Differential[i].DiffNegative / (Differential[i].DiffNegative + Differential[i].DiffPositive);
        }

    }

    //ordered descending, meaning data at top is highly recommended.
    /// <summary>
    /// Calculate alternative data and get final preferential value, sorted descending by value.
    /// </summary>
    /// <returns>sorted array of final value.</returns>
    public RankedData[] CalculateAndGetFinalData()
    {
        Normalize();
        CalculateIdealSolution();
        CalculateDiffIdealSolution();
        CalculatePreferencesValue();

        RankedData[] raw = new RankedData[Differential.Count];

        for (int i = 0; i < raw.Length; i++)
        {
            raw[i] = new RankedData(Differential[i].PreferentialValue);
        }

        var sorted = raw.ToList().OrderByDescending(x => x.PreferentialValue).ToArray();

        for (int i = 0; i < sorted.Length; i++)
        {
            sorted[i].Rank = i + 1;
        }

        return sorted;
    }

}
