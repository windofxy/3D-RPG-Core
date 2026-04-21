using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    public CharacterData_SO characterData_Template;
    public AttackData_SO attackData;

    public event Action<int, int> TakeDamaged;

    private CharacterData_SO characterData;

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
    #endregion

    #region Character Combat

    public bool IsDead
    {
        get => CurrentHealth <= 0;
    }

    public void TakeDamage(CharacterStats attacker, out bool isDead)
    {
        int damage = Mathf.Max(attacker.CurrentDamage() - CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        isDead = IsDead;

        if (attacker.isCritical)
        {
            var animator = GetComponent<Animator>();
            if (animator != null) { animator.SetTrigger("Hit"); }
        }
        
        // 通知伤害事件
        TakeDamaged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void TakeDamage(int _damage, out bool isDead)
    {
        int damage = Mathf.Max(_damage - CurrentDefence, 0);
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        isDead = IsDead;
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
