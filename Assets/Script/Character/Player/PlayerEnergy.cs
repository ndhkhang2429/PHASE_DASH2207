using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Energy Settings")]
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private int currentEnergy;
    [SerializeField] private EnergyBar energyBar;

    private void Start()
    {
        currentEnergy = 0;
        energyBar.UpdateEnergyBar(currentEnergy, maxEnergy);
    }

    //neu energy con du thi su dung duoc chieu
    public bool UseEnergy(int amount)
    {
        if(currentEnergy < amount)
        {
            return false;
        }

        currentEnergy -= amount;
        energyBar.UpdateEnergyBar(currentEnergy, maxEnergy);
        return true;
    }

    public void GainEnergy(int amount)
    {
        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        energyBar.UpdateEnergyBar(currentEnergy, maxEnergy);
    }

    public int GetCurrrentEnergy()
    {
        return currentEnergy;
    }

    public int GetMaxEnergy()
    {
        return maxEnergy;
    }
}
