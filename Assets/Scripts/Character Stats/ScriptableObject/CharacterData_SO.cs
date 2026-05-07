using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Data/Character Data")]
public class CharacterData_SO : ScriptableObject
{
    [Header("Stats Info")]
    public int maxHealth; // 最大血量
    public int currentHealth; // 当前血量
    public int baseDefence; // 基本防御力
    public int currentDefence; // 当前防御力

    [Header("Kill")]
    public int killExp; // 击杀时给予击杀者经验值

    [Header("Level")]
    public int maxLevel; // 最大等级
    public int currentLevel; // 当前等级
    public int baseExp; // 升到下一级所需经验
    public int currentExp; // 当前经验值
    public float levelBuff; // 升级属性加成

    // 每级属性加成乘数
    public float LevelMultiplier
    {
        get { return 1 + (currentLevel - 1) * levelBuff; }
    }

    // 添加经验值
    public void AddExp(int exp)
    {
        currentExp += exp;

        while (baseExp != 0 && currentExp >= baseExp)
        {
            LevelUp();
        }
    }

    // 升级
    private void LevelUp()
    {
        if (currentLevel >= maxLevel) return;

        // 等级和经验需求变更
        currentLevel = Mathf.Clamp(currentLevel + 1, 0, maxLevel);
        baseExp += (int)(baseExp * LevelMultiplier);

        // 属性变更
        maxHealth = (int)(maxHealth * LevelMultiplier);
        currentHealth = maxHealth;


        Debug.LogFormat("Level up! Max health: {0}", maxHealth);
    }
}
