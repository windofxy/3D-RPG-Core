using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(CharacterStats))]
public class PlayerController : MonoBehaviour, IEndGameObserver
{
    // 组件变量
    NavMeshAgent agent;
    Animator anim;
    CharacterStats characterStats;

    #region 私有变量
    // 攻击目标GameObject
    private GameObject attackTarget;
    // 攻击冷却时间
    private float lastAttackTime;
    // Agent停止距离
    private float stoppingDistance;
    // 攻击协程
    private Coroutine attackCoroutine;
    #endregion

    void Awake()
    {
        // 获取 NavMeshAgent 组件
        agent = GetComponent<NavMeshAgent>();
        // 获取 Animator 组件
        anim = GetComponent<Animator>();
        // 获取 CharacterStats 组件
        characterStats = GetComponent<CharacterStats>();

        stoppingDistance = agent.stoppingDistance;
    }

    void OnEnable()
    {
        GameManager.Instance.AddEndGameObserver(this);

        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EnemyClicked;
    }

    // Start is called before the first frame update
    void Start()
    {
        // 将玩家注册到GameManager
        GameManager.Instance.RegisterPlayer(characterStats);
    }

    // Update is called once per frame
    void Update()
    {
        // 玩家死亡，广播结束事件
        if(characterStats.IsDead)
            GameManager.Instance.NotifyEndGameObservers();

        // 每帧更新动画参数
        SwitchAnimation();

        // 减少攻击冷却时间
        lastAttackTime = Mathf.Max(0f, lastAttackTime - Time.deltaTime);
    }

    void OnDisable()
    {
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EnemyClicked;

        GameManager.Instance.RemoveEndGameObserver(this);
    }

    public void MoveToTarget(Vector3 target)
    {
        // 清空攻击目标
        ClearAttackTarget();
        agent.isStopped = false;
        agent.destination = target;
    }

    private void SwitchAnimation()
    {
        // 使用 NavMeshAgent 的速度更新行走动画参数
        anim.SetFloat("Speed", agent.velocity.sqrMagnitude);
        anim.SetBool("Death", characterStats.IsDead);
    }

    private void EnemyClicked(GameObject target)
    {
        if (target != null)
        {
            // 设置攻击目标
            attackTarget = target;
            // 停止攻击协程
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            // 提前计算一次暴击，防止值未计算
            characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
            // 启动协程，移动到攻击目标的位置
            attackCoroutine = StartCoroutine(MoveToAttackTarget());
        } 
    }

    private void ClearAttackTarget()
    {
        // 停止攻击协程
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        // 攻击目标置空
        attackTarget = null;
        // 恢复停止距离
        agent.stoppingDistance = stoppingDistance;
    }

    // 移动到攻击目标的位置，使用协程
    private IEnumerator MoveToAttackTarget()
    {
        // 攻击循环
        while (!attackTarget.IsDestroyed())
        {
            agent.isStopped = false;
            agent.stoppingDistance = characterStats.attackData.attackRange;
            // 前往目标，直到进入攻击范围
            while (!attackTarget.IsDestroyed() && Vector3.Distance(attackTarget.transform.position, transform.position) > characterStats.attackData.attackRange)
            {
                agent.SetDestination(attackTarget.transform.position);
                // 控制权交还给引擎
                yield return null;
            }
            agent.isStopped = true;

            // 执行攻击
            if (!attackTarget.IsDestroyed() && lastAttackTime <= 0f)
            {
                // 重置冷却时间
                lastAttackTime = characterStats.attackData.coolDown;
                // 攻击
                Attack();
            }
            // 控制权交还给引擎
            yield return null;
        }
    }

    private void Attack()
    {
        // 朝向目标
        transform.LookAt(attackTarget.transform);
        // 暴击判断
        characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
        // 播放攻击动画
        anim.SetBool("Critical", characterStats.isCritical);
        anim.SetTrigger("Attack");
    }

    #region 动画事件
    void Hit()
    {
        if (attackTarget == null || attackTarget.IsDestroyed())
        {
            // 清空攻击目标
            ClearAttackTarget();
            return;
        }
        if (attackTarget.CompareTag("Enemy"))
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
        else if (attackTarget.CompareTag("Attackable"))
        {
            var rock = attackTarget.GetComponent<Projectile_Rock>();
            if (rock && rock.rockStates == Projectile_Rock.Projectile_Rock_States.HitNothing)
            {
                // 设置石头攻击者
                rock.attacker = characterStats;
                // 更新石头状态
                rock.rockStates = Projectile_Rock.Projectile_Rock_States.HitEnemy;
                // 设置石头初速度
                rock.GetComponent<Rigidbody>().velocity = Vector3.one;
                // 击飞石头
                rock.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
            }
        }
        //Debug.Log("玩家攻击");
    }

    public void EndNotify()
    {
        ClearAttackTarget();
        agent.isStopped = true;
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EnemyClicked;
    }
    #endregion
}
