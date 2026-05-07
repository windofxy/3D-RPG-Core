using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public CharacterData_SO characterData_Template;
    public AttackData_SO attackData;

    public event Action<int, int> TakeDamaged;

    internal CharacterData_SO characterData;

    [HideInInspector]
    public bool isCritical;

    private void Awake()
    {
        if (characterData_Template != null)
            characterData = Instantiate(characterData_Template);
    }

    #region CharacterData_SO 访问
    public int MaxHealth
    {
        get => characterData?.maxHealth ?? 0;
        set => characterData.maxHealth = value;
    }

    public int CurrentHealth 
    {
        get => characterData?.currentHealth ?? 0;
        set => characterData.currentHealth = value;
    }

    public int BaseDefence
    {
        get => characterData?.baseDefence ?? 0;
        set => characterData.baseDefence = value;
    }

    public int CurrentDefence
    {
        get => characterData?.currentDefence ?? 0;
        set => characterData.currentDefence = value;
    }

    public int KillExp
    {
        get => characterData?.killExp ?? 0;
        set => characterData.killExp = value;
    }

    public int MaxLevel
    {
        get => characterData?.maxLevel ?? 0;
        set => characterData.maxLevel = value;
    }

    public int CurrentLevel
    {
        get => characterData?.currentLevel ?? 0;
        set => characterData.currentLevel = value;
    }

    public int BaseExp
    {
        get => characterData?.baseExp ?? 0;
        set => characterData.baseExp = value;
    }

    public int CurrentExp
    {
        get => characterData?.currentExp ?? 0;
        set => characterData.currentExp = value;
    }

    public float LevelBuff
    {
        get => characterData?.levelBuff ?? 0f;
        set => characterData.levelBuff = value;
    }

    public float LevelMultiplier
    {
        get => characterData?.LevelMultiplier ?? 0f;
    }
    #endregion

    #region Character Combat

    public bool IsDead
    {
        get => CurrentHealth <= 0;
    }

    public void TakeDamage(CharacterStats attacker, out bool isDead)
    {
        isDead = IsDead;
        // 如果角色已死亡，直接返回
        if (isDead) return;

        int damage = Mathf.Max(attacker.CurrentDamage() - CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        isDead = IsDead;

        // 如果产生暴击，播放受击动画
        if (attacker.isCritical)
        {
            var animator = GetComponent<Animator>();
            if (animator != null) { animator.SetTrigger("Hit"); }
        }

        // 如果角色死亡，给予攻击者经验值
        if (isDead)
        {
            attacker.characterData.AddExp(characterData.killExp);
        }
        
        // 通知伤害事件
        TakeDamaged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(int _damage, out bool isDead, CharacterStats attacker = null)
    {
        isDead = IsDead;
        // 如果角色已死亡，直接返回
        if (isDead) return;

        int damage = Mathf.Max(_damage - CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        isDead = IsDead;

        // 如果角色死亡，给予攻击者经验值
        if (isDead && attacker != null)
        {
            attacker.characterData.AddExp(characterData.killExp);
        }

        // 通知伤害事件
        TakeDamaged?.Invoke(CurrentHealth, MaxHealth);
    }

    private int CurrentDamage()
    {
        int damage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);
        if (isCritical)
        {
            damage = Mathf.FloorToInt(damage * attackData.criticalMultiplier);
            //Debug.Log("暴击！");
        }
        return damage;
    }

    #endregion
}
