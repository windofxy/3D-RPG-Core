using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Grunt : EnemyController
{
    [Header("Skill")]
    public float kickForce;

    // 击退攻击目标
    public void KickOff()
    {
        if (attackTarget == null || attackTarget.IsDestroyed() || !transform.IsFacingTarget(attackTarget.transform))
            return;
        // 获取击退向量
        Vector3 kickOffDirecton = attackTarget.transform.position - transform.position;
        // 单位化向量
        kickOffDirecton.Normalize();
        // 击退目标
        attackTarget.GetComponent<NavMeshAgent>().isStopped = true;
        attackTarget.GetComponent<NavMeshAgent>().velocity = kickOffDirecton * kickForce;
    }
}
