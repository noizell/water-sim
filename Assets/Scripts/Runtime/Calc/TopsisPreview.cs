using UnityEngine;

public class TopsisPreview : MonoBehaviour
{
    private void Start()
    {
        TopsisCalculator tc = new TopsisCalculator();

        //A1
        tc.AddAlternative(new TopsisCalculator.AlternativeElementData[] {
            new TopsisCalculator.AlternativeElementData(.75f, 5f, false),
            new TopsisCalculator.AlternativeElementData(2000f, 3f, true),
            new TopsisCalculator.AlternativeElementData(18, 4f, false),
            new TopsisCalculator.AlternativeElementData(50, 4f, true),
            new TopsisCalculator.AlternativeElementData(500, 2f, false)
        });

        //A2
        tc.AddAlternative(new TopsisCalculator.AlternativeElementData[] {
            new TopsisCalculator.AlternativeElementData(.5f, 5f, false),
            new TopsisCalculator.AlternativeElementData(1500f, 3f, true),
            new TopsisCalculator.AlternativeElementData(20f, 4f, false),
            new TopsisCalculator.AlternativeElementData(40f, 4f, true),
            new TopsisCalculator.AlternativeElementData(450f, 2f, false)
        });

        //A3
        tc.AddAlternative(new TopsisCalculator.AlternativeElementData[] {
            new TopsisCalculator.AlternativeElementData(.9f, 5f, false),
            new TopsisCalculator.AlternativeElementData(2050, 3f, true),
            new TopsisCalculator.AlternativeElementData(35f, 4f, false),
            new TopsisCalculator.AlternativeElementData(35f, 4f, true),
            new TopsisCalculator.AlternativeElementData(800f, 2f, false)
        });


        var finalData = tc.CalculateAndGetFinalData();

        foreach (var data in finalData)
        {
            Debug.Log($"data with value {data.PreferentialValue} is ranked {data.Rank}");
        }

        tc.ClearData();
    }
}
