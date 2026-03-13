using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillBar;
    [SerializeField] private TextMeshProUGUI valueText;

    public void UpdateBar(int curVal, int maxVal)
    {
        fillBar.fillAmount = (float)curVal / (float)maxVal;
        valueText.text = curVal.ToString() + " / " + maxVal.ToString();
    }
}
