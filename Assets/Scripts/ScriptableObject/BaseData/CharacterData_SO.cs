using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = ("New Data"), menuName = ("Character Stats/Data"))]
public class CharacterData_SO : ScriptableObject
{
    [Header("State Info")]
    public float maxHealth;
    public float currentHealth;
    public float baseDefence;
    public float currentDefence;

    [Header("Kill")]
    public int killPoint;

    [Header("Level")]
    public int currentLevel;
    public int maxLevel;
    
    public int baseExp;
    public int currentExp;

    public float levelBuff;

    public float LevelMultiplier
    {
        get
        {
            return 1 + (currentLevel - 1) * levelBuff;
        }
    }

    public void UpdateExp(int point)
    {
        currentExp += point;
        if (currentExp >= baseExp)
        {
            currentExp = currentExp - baseExp;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentLevel = Mathf.Clamp(currentLevel + 1, 0, maxLevel);
        baseExp = (int)(baseExp * LevelMultiplier);

        maxHealth = (int)(maxHealth * LevelMultiplier);
        currentHealth = maxHealth;

        baseDefence = (int)(baseDefence * LevelMultiplier);
        currentDefence = baseDefence;

        Debug.Log("Level Up!" + currentLevel + "maxHealth=" + maxHealth + "baseDefence=" + baseDefence);
    }
}
