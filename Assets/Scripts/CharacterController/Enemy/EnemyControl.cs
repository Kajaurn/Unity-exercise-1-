using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyControl : MonoBehaviour,IEndGameObserver
{
    public enum EnemyState
    {
        Guard,
        Patrol,
        Chase,
        Dead
    }

    private NavMeshAgent agent;
    private Transform enemyTransform;
    private Animator animator;
    private CharacterStats enemyStats;
    private Collider[] colliders;

    [Header("Basic Settings")]
    public float sightRadius;
    private GameObject attackTarget;

    public bool isGuard;

    private float enemySpeed;

    public float lookAtTime;
    private float remainLookAtTime;

    public float rotateSpeed;

    [Header("Patrol State")]
    public float patrolRange;
    private Vector3 wayPoint;
    private Vector3 guardPosition;
    private Quaternion guardRotation;
    
    
    int walkHash;
    int chaseHash;
    int followHash;
    int enemyAttackHash;
    int enemySkillHash;
    int criticalHash;
    int deadHash;

    bool isWalking;
    bool isChasing;
    bool isFollowing;
    bool isDead;
    bool playerDead = false;

    [HideInInspector]
    public EnemyState enemyState;

    private float lastAttackTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemySpeed = agent.speed;
        enemyTransform = transform;
        enemyStats = GetComponent<CharacterStats>();
        colliders = GetComponentsInChildren<Collider>();
        
        animator = GetComponent<Animator>();
        walkHash = Animator.StringToHash("Walk");
        chaseHash = Animator.StringToHash("Chase");
        followHash = Animator.StringToHash("Follow");
        enemyAttackHash = Animator.StringToHash("EnemyAttack");
        enemySkillHash = Animator.StringToHash("EnemySkill");
        criticalHash = Animator.StringToHash("Critical");
        deadHash = Animator.StringToHash("Dead");
        
        guardPosition = enemyTransform.position;
        guardRotation = enemyTransform.rotation;

        remainLookAtTime = lookAtTime;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (isGuard)
        {
            enemyState = EnemyState.Guard;
        }
        else
        {
            enemyState = EnemyState.Patrol;
            GetNewWayPoint();
        }
        //FIXME:做场景切换的时候改掉
        GameManager.Instance.AddObserver(this);
    }

    //切换场景时启用
    //private void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}

    private void OnDisable()
    {
        if (!GameManager.IsInitialized)
        {
            return;
        }
        GameManager.Instance.RemoveObserver(this);
    }

    // Update is called once per frame
    void Update()
    {
        EnemyDead();
        if (!playerDead)
        {
            SetupAnimator();
            SwitchEnemyState();
            EnemyBehaviour();
            lastAttackTime -= Time.deltaTime;
        }
    }

    void SetupAnimator()
    {
        animator.SetBool(walkHash, isWalking);
        animator.SetBool(chaseHash, isChasing);
        animator.SetBool(followHash, isFollowing);
        animator.SetBool(criticalHash, enemyStats.isCritical);
        animator.SetBool(deadHash, isDead);
    }

    void SwitchEnemyState()
    {
        if (isDead)
        {
            enemyState = EnemyState.Dead;
        }
        else if (FoundPlayer())
        {
            enemyState = EnemyState.Chase;
            //Debug.Log("I find you!");
        }
    }
    
    void EnemyBehaviour() 
    {
        if (enemyState == EnemyState.Guard) 
        {
            isChasing = false;
            
            if (enemyTransform.position != guardPosition)
            {
                isWalking = true;
                agent.speed = enemySpeed * 0.5f;
                agent.isStopped = false;
                agent.destination = guardPosition;

                if (Vector3.SqrMagnitude(guardPosition - enemyTransform.position) <= agent.stoppingDistance)
                {
                    isWalking = false;
                    enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, guardRotation, rotateSpeed);
                }
            }
        }
        else if (enemyState == EnemyState.Patrol) 
        {
            isChasing = false;
            agent.speed = enemySpeed * 0.5f;
            if (Vector3.Distance(wayPoint, enemyTransform.position) <= agent.stoppingDistance)
            {
                isWalking = false;
                if (remainLookAtTime > 0)
                {
                    remainLookAtTime -= Time.deltaTime;
                }
                else
                {
                    GetNewWayPoint();
                }
            }
            else
            {
                isWalking = true;
                agent.destination = wayPoint;
            }
        }
        else if (enemyState == EnemyState.Chase)
        {
            agent.speed = enemySpeed;
            ChasePlayer();

            //配合动画
            isWalking = false;
            isChasing = true;
        }
        else if (enemyState == EnemyState.Dead)
        {
            foreach(Collider col in colliders)
            {
                col.enabled = false;
            }
            //agent.enabled = false;
            agent.radius = 0;

            //Destroy(gameObject, 2f);
        }
    }

    bool FoundPlayer()
    {
        var colliders = Physics.OverlapSphere(enemyTransform.position, sightRadius);
        foreach(var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }

    bool TargetInAttackRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, enemyTransform.position) <= enemyStats.attackData.attackRange;
        }
        else if (TargetInSkillRange())
        {
            return false;
        }
        else
        {
            return false;
        }
    }

    bool TargetInSkillRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(attackTarget.transform.position, enemyTransform.position) <= enemyStats.attackData.skillRange;
        }
        else
        {
            return false;
        }
    }

    void ChasePlayer()
    {
        if (!FoundPlayer())
        {
            //玩家脱离追击范围则返回上一个状态
            isFollowing = false;
            if (remainLookAtTime > 0)
            {
                agent.destination = enemyTransform.position;
                remainLookAtTime -= Time.deltaTime;
            }
            else if (isGuard) 
            {
                enemyState = EnemyState.Guard;
            }
            else
            {
                enemyState = EnemyState.Patrol;
            }
        }
        //追Player
        else
        {
            isFollowing = true;
            agent.isStopped = false;
            agent.destination = attackTarget.transform.position;
        }
        //玩家进入攻击范围内则攻击
        if (TargetInAttackRange() || TargetInSkillRange())
        {
            isFollowing = false;
            agent.isStopped = true;
            //FIXME:敌人转向太灵活，需要用其他方法改变转向行为，或者换一个长一点的动作测试动画开始播放之后是否转向(估计长动画没问题)
            enemyTransform.LookAt(attackTarget.transform);
            if (lastAttackTime < 0)
            {
                lastAttackTime = enemyStats.attackData.coolDown;
                //暴击判断
                enemyStats.isCritical = Random.value < enemyStats.attackData.criticalChance;
                //执行攻击
                Attack();
            }
        }
    }

    void Attack()
    {
        //if (TargetInAttackRange() && TargetInSkillRange())
        //{
            
        //}
        //enemyTransform.LookAt(attackTarget.transform);
        if (TargetInAttackRange())
        {
            //近身攻击动画
            animator.SetTrigger(enemyAttackHash);
        }
        if (TargetInSkillRange())
        {
            //远程/技能攻击动画
            animator.SetTrigger(enemySkillHash);
        }
    }

    void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;
        
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);
        Vector3 randomPoint = new Vector3(guardPosition.x + randomX, enemyTransform.position.y, guardPosition.z + randomZ);

        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : enemyTransform.position;
    }

    void EnemyDead()
    {
        if (enemyStats.CurrentHealth == 0)
        {
            isDead = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        enemyTransform = transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemyTransform.position, sightRadius);
    }

    public void EndNotify()
    {
        //FIXME:返回巡逻或守卫状态
        if (enemyState != EnemyState.Dead)
        {
            playerDead = true;
            isChasing = false;
            if (enemyTransform.position != guardPosition)
            {
                isWalking = true;
                agent.speed = enemySpeed * 0.5f;
                agent.isStopped = false;
                agent.destination = guardPosition;

                if (Vector3.SqrMagnitude(guardPosition - enemyTransform.position) <= agent.stoppingDistance)
                {
                    isWalking = false;
                    enemyTransform.rotation = Quaternion.Lerp(enemyTransform.rotation, guardRotation, rotateSpeed);
                }
            }
            SetupAnimator();
        }
        //以上仅为敌人击杀玩家后返回初始位置

        //Debug.Log("You Dead.");
    }
}
