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
}
