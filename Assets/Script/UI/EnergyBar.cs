using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    [SerializeField] private Image fillbar;
    [SerializeField] private TextMeshProUGUI textValue;

    public void UpdateEnergyBar(int curVal, int maxVal)
    {
        fillbar.fillAmount = (float)curVal / (float)maxVal;
        textValue.text = curVal.ToString() + " / " + maxVal.ToString();
    }
}
