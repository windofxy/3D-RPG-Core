using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD, PATROL, CHASE, DEAD }

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(CharacterStats))]
public class EnemyController : MonoBehaviour, IEndGameObserver
{
    [Header("Basic Settings")]
    // 视野范围
    public float sightRadius;
    // 是否是站桩敌人（守卫）
    public bool isGuard;
    [Header("Patrol State")]
    // 巡逻范围
    public float patrolRadius;
    // 观察时间
    public float lookAtTime;

    // 组件变量
    private NavMeshAgent agent;
    private Animator anim;
    private CharacterStats characterStats;
    private Collider coll;

    #region 保护变量
    // 攻击目标
    protected GameObject attackTarget;
    #endregion

    #region 私有变量
    // 当前状态
    private EnemyStates enemyStates;
    // 追击速度
    private float speed;
    // 初始坐标
    private Vector3 guardPosition;
    // 初始朝向
    private Quaternion guardRotation;
    // 追击开始时状态
    private EnemyStates stateBeforeChase;
    // 追击冷却时间(s)
    private float chaseCooldownTime;
    // 巡逻坐标点
    private Vector3 patrolWayPoint;
    // 坐标停留时间
    private float remainLookAtTime;
    // 攻击冷却时间
    private float lastAttackTime;
    // 玩家是否死亡
    private bool isPlayerDead;
    #endregion

    #region 动画参数
    // 是否追击
    private bool isChase;
    // 是否死亡
    private bool isDeath;
    #endregion


    void Awake()
    {
        // 获取 NavMeshAgent 组件
        agent = GetComponent<NavMeshAgent>();
        // 获取 Animator 组件
        anim = GetComponent<Animator>();
        // 获取 CharacterStats 组件
        characterStats = GetComponent<CharacterStats>();
        // 获取 Collider 组件
        coll = GetComponent<Collider>();

        speed = agent.speed;
        guardPosition = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
    }

    void OnEnable()
    {
        GameManager.Instance.AddEndGameObserver(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isGuard)
        {
            enemyStates = EnemyStates.GUARD;
        }
        else
        {
            enemyStates = EnemyStates.PATROL;
            GetNewWayPoint();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlayerDead)
        {
            SwitchStates();

            // 更新剩余观察时间
            remainLookAtTime = Mathf.Max(0f, remainLookAtTime - Time.deltaTime);
            // 更新攻击冷却时间
            lastAttackTime = Mathf.Max(0f, lastAttackTime - Time.deltaTime);
        }
        SwitchAnimation();
    }

    void OnDisable()
    {
        GameManager.Instance?.RemoveEndGameObserver(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        // 在编辑器内绘制敌人视野范围
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    private void SwitchStates()
    {
        if (characterStats.IsDead)
        {
            enemyStates = EnemyStates.DEAD;
        }
        // 追击逻辑：如果发现Player，切换到追击状态
        else if (enemyStates != EnemyStates.CHASE && chaseCooldownTime <= 0f && FoundPlayer())
        {
            //Debug.Log("Player Found!");
            stateBeforeChase = enemyStates;
            enemyStates = EnemyStates.CHASE;
        }

        chaseCooldownTime = Mathf.Max(0f, chaseCooldownTime - Time.deltaTime);

        switch (enemyStates)
        {
            case EnemyStates.GUARD:
                isChase = false;
                agent.isStopped = false;
                agent.speed = speed * 1f;
                // 如果没有到达守卫坐标
                if (transform.position != guardPosition)
                {
                    // 前往守卫坐标
                    agent.SetDestination(guardPosition);
                    // 到达守卫坐标附近后，缓慢转到初始朝向
                    if (Vector3.Distance(transform.position, guardPosition) <= agent.stoppingDistance)
                    {
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
                    }
                }
                break;
            case EnemyStates.PATROL:
                isChase = false;
                agent.isStopped = false;
                agent.speed = speed * 0.5f;
                // 如果敌人到达巡逻坐标点
                if (Vector3.Distance(transform.position, patrolWayPoint) <= agent.stoppingDistance)
                {
                    // 如果剩余观察时间为0
                    if (remainLookAtTime <= 0f)
                    {
                        // 重置剩余观察时间
                        remainLookAtTime = lookAtTime;
                        // 刷新巡逻坐标点
                        GetNewWayPoint();
                    }
                }
                else
                {
                    // 前往巡逻坐标点
                    agent.SetDestination(patrolWayPoint);
                }
                break;
            case EnemyStates.CHASE:
                isChase = true;
                agent.speed = speed;
                agent.isStopped = false;

                // 检测玩家是否在视野内
                if (!FoundPlayer())
                {
                    // 玩家拉脱，回到上一个状态
                    // 设置追击冷却时间
                    chaseCooldownTime = 1f;
                    // 回到之前的状态
                    enemyStates = stateBeforeChase;
                }
                else
                {
                    // 追击玩家
                    agent.SetDestination(attackTarget.transform.position);
                }

                // 检测玩家是否在攻击范围内
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    agent.isStopped = true;

                    if (lastAttackTime <= 0f)
                    {
                        // 重置攻击冷却时间
                        lastAttackTime = characterStats.attackData.coolDown;
                        // 执行攻击
                        Attack();
                    }
                }

                break;
            case EnemyStates.DEAD:
                // 关闭碰撞体组件
                coll.enabled = false;
                // 停止NavMeshAgent并取消阻挡
                agent.radius = 0f;
                agent.isStopped = true;
                // 两秒后销毁敌人
                Destroy(gameObject, 2f);
                break;
        }
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Chase", isChase);
        anim.SetBool("Death", characterStats.IsDead);
    }

    private void ClearAttackTarget()
    {
        // 攻击目标置空
        attackTarget = null;
        // 停止Agent
        agent.ResetPath();
    }

    private void Attack()
    {
        // 朝向目标
        transform.LookAt(attackTarget.transform);
        // 暴击判断
        bool isCritical = Random.value < characterStats.attackData.criticalChance;
        anim.SetBool("Critical", isCritical);
        if (characterStats.attackData.attackRange <= characterStats.attackData.skillRange)
        {
            if (TargetInAttackRange())
            {
                // 近战攻击
                anim.SetTrigger("Attack");
            }
            else if (TargetInSkillRange())
            {
                // 特殊攻击
                anim.SetTrigger("Skill");
            }
        }
        else
        {
            if (TargetInSkillRange())
            {
                // 特殊攻击
                anim.SetTrigger("Skill");
            }
            else if (TargetInAttackRange())
            {
                // 近战攻击
                anim.SetTrigger("Attack");
            }
        }
    }

    // 判断玩家是否在视野内
    private bool FoundPlayer()
    {
        // 获取视野范围内的所有碰撞体
        Collider[] colliders = Physics.OverlapSphere(transform.position, sightRadius);

        foreach (Collider collider in colliders)
        {
            // 如果有碰撞体所属对象标签为Player，则找到玩家
            if (collider.CompareTag("Player"))
            {
                attackTarget = collider.gameObject;
                return true;
            }
        }

        attackTarget = null;
        return false;
    }

    // 判断攻击目标是否在近战攻击范围内
    private bool TargetInAttackRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.attackRange;
        }
        return false;
    }

    // 判断攻击目标是否在远程攻击范围内
    private bool TargetInSkillRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= characterStats.attackData.skillRange;
        }
        return false;
    }

    // 获取新的巡逻坐标点
    private void GetNewWayPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);
        Vector3 randomPoint = new Vector3(guardPosition.x + randomX, transform.position.y, guardPosition.z + randomZ);
        // 获取离随机点最近的导航网格上的一个点
        NavMeshHit hit;
        patrolWayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, 1) ? hit.position : transform.position;
    }

    #region 动画事件
    protected void Hit()
    {
        if (attackTarget == null || attackTarget.IsDestroyed())
        {
            // 清空攻击目标
            ClearAttackTarget();
            return;
        }
        if (transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(characterStats, out bool isDead);
            if (isDead)
            {
                // 清空攻击目标
                ClearAttackTarget();
                return;
            }
        }
    }
    #endregion

    public void EndNotify()
    {
        isChase = false;
        isPlayerDead = true;
        // 清除攻击目标
        ClearAttackTarget();
        // 播放胜利动画
        anim.SetBool("Win", true);
    }
}
