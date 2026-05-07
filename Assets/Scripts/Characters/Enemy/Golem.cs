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

    #region 私有变量
    GameObject currentRock = null;
    #endregion

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

    // 生成石头
    public void SpawnRock()
    {
        if (currentRock != null && !currentRock.IsDestroyed())
        {
            Destroy(currentRock);
            currentRock = null;
        }
        // 实例化石头
        currentRock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
        currentRock.transform.SetParent(handPos);
        currentRock.transform.localPosition = Vector3.zero;
        currentRock.transform.localRotation = Quaternion.identity;
    }

    // 扔石头
    public void ThrowRock()
    {
        if (attackTarget == null || attackTarget.IsDestroyed())
        {
            if (currentRock != null && !currentRock.IsDestroyed())
            {
                Destroy(currentRock);
            }
            currentRock = null;
            return;
        }

        // 朝向目标
        transform.LookAt(attackTarget.transform);
        // 获取石头组件
        var rock = currentRock.GetComponent<Projectile_Rock>();
        rock.transform.SetParent(null);
        // 设置攻击者
        rock.attacker = characterStats;
        // 设置目标
        rock.target = attackTarget;
        // 更新石头状态
        rock.rockStates = Projectile_Rock.Projectile_Rock_States.HitPlayer;
        rock.FlyToTarget();
        // 取消石头
        currentRock = null;
    }
}
