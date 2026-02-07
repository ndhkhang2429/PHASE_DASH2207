using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProfile", menuName = "Data/PlayerProfile")]
public class PlayerProfile : ScriptableObject
{
    [field: SerializeField] public int PlayerHealth { get; private set; }
    [field: SerializeField] public int PlayerHealthMax { get; private set; }

    public void SetPlayerHealth(int value) => this.PlayerHealth = value;
    public void SetPlayerHealthMax(int value) => this.PlayerHealthMax = value;
}
