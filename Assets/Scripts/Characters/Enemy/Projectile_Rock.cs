using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Projectile_Rock : MonoBehaviour
{
    #region 配置项
    [Header("Basic Settings")]
    public float force;
    public int damage;
    public GameObject breakEffect;
    #endregion

    #region 组件变量
    private Rigidbody rb;
    #endregion

    #region 枚举
    public enum Projectile_Rock_States { HitPlayer, HitEnemy, HitNothing }
    #endregion

    #region 公有变量
    [HideInInspector]
    public GameObject target;
    [HideInInspector]
    public Projectile_Rock_States rockStates;
    #endregion

    #region 私有变量
    private Vector3 direction;
    #endregion

    void Awake()
    {
        // 获取 Rigidbody 组件
        rb = GetComponent<Rigidbody>();
        rockStates = Projectile_Rock_States.HitPlayer;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = Vector3.one;
        FlyToTarget();
    }

    void FixedUpdate()
    {
        // 速度平方小于1时
        if (rb.velocity.sqrMagnitude < 1f)
        {
            // 更新石头状态
            rockStates = Projectile_Rock_States.HitNothing;
        }
    }

    public void FlyToTarget()
    {
        // 计算玩家方向
        direction = (target.transform.position - transform.position + Vector3.up).normalized;
        // 给 Rigidbody 添加朝向玩家方向的力
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (rockStates)
        {
            case Projectile_Rock_States.HitPlayer:
                if (collision.gameObject.CompareTag("Player"))
                {
                    // 击飞玩家
                    collision.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    collision.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;
                    // 播放击晕动画
                    collision.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    // 造成伤害
                    collision.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, out bool _);
                    // 更新石头状态
                    rockStates = Projectile_Rock_States.HitNothing;
                }
                break;
            case Projectile_Rock_States.HitEnemy:
                if (collision.gameObject.GetComponent<Golem>())
                {
                    // 造成伤害
                    collision.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, out bool _);
                    // 更新石头状态
                    rockStates = Projectile_Rock_States.HitNothing;
                    // 播放粒子效果
                    Instantiate(breakEffect, transform.position, Quaternion.LookRotation((transform.position - collision.transform.position).normalized));
                    // 销毁石头
                    Destroy(gameObject);
                }
                break;
            case Projectile_Rock_States.HitNothing:
                break;
        }
    }
}
