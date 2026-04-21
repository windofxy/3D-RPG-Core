using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController
{
    [Header("Attack")]
    public float kickForce;

    [Header("Skill")]
    public GameObject rockPrefab;
    public Transform handPos;

    // 击退并伤害攻击目标
    public void KickOff()
    {
        if (attackTarget == null || attackTarget.IsDestroyed() || !transform.IsFacingTarget(attackTarget.transform))
            return;
        // 伤害目标
        Hit();
        // 获取击退向量
        Vector3 kickOffDirecton = attackTarget.transform.position - transform.position;
        // 单位化向量
        kickOffDirecton.Normalize();
        // 击退目标
        Vector3 kickOffVector = kickOffDirecton * kickForce;
        attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
        attackTarget.GetComponent<NavMeshAgent>().velocity = kickOffVector;
    }

    // 扔石头
    public void ThrowRock()
    {
        if (attackTarget == null || attackTarget.IsDestroyed())
            return;
        // 朝向目标
        transform.LookAt(attackTarget.transform);
        // 实例化石头
        var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
        // 设置目标
        rock.GetComponent<Projectile_Rock>().target = attackTarget;
    }
}
