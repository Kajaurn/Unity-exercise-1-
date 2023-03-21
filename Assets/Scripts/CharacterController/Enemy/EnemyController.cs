using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    Animator animator;
    CharacterController enemyController;
    Transform enemyTransform;
    Transform targetTransform;

    Vector3 enemyMovement;

    Vector3 enemyMove;
    float distance;

    //float enemySpeed = 5;

    

    public enum EnemyState 
    {
        Guard,
        Patrol,
        Chase,
        Attack,
        Dead
    }
    public EnemyState enemyState = EnemyState.Patrol;

    int enemyAttackHash;
    int findPlayerHash;

    GameObject attackTarget;

    public float sightRadius;
    // Start is called before the first frame update
    void Start()
    {
        enemyTransform = transform;
        animator = GetComponent<Animator>();
        findPlayerHash = Animator.StringToHash("FindPlayer");
        enemyAttackHash = Animator.StringToHash("EnemyAttack");
        enemyController = GetComponent<CharacterController>();
        
        targetTransform = GameObject.FindGameObjectWithTag("Player").transform;

        EnemyTarget();
    }

    // Update is called once per frame
    void Update()
    {
        //SwitchEnemyStates();
        //SetupAnimator();
        //MoveEnemy();
    }

    void SwitchEnemyStates() 
    {
        if (FoundPlayer()) 
        {
            enemyState = EnemyState.Chase;
            //enemyController.Move(attackTarget.transform.position);
            Debug.Log("I find you!");
        }
    }

    void SetupAnimator() 
    {
        if (enemyState == EnemyState.Chase) 
        {
            //animator.SetBool(findPlayerHash, true);

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

        return false;
    }

    void EnemyTarget() 
    {
        //Ä¿±êÎ»ÖÃ
        enemyMovement.x = Random.Range(-1.0f, 1.0f);
        enemyMovement.z = Random.Range(-1.0f, 1.0f);
        enemyMovement.y = 0;
    }
    
    void MoveEnemy() 
    {
        if (enemyTransform.position != enemyMovement) 
        {
            enemyController.Move(enemyMovement * Time.deltaTime);
        }
        else 
        {
            EnemyTarget();
        }
    }

    private void OnAnimatorMove()
    {
        if (enemyTransform.position != targetTransform.position)
        {
            //enemyController.Move(enemyMovement * Time.deltaTime);

            distance = (targetTransform.position - enemyTransform.position).magnitude;

            enemyMove = distance * Vector3.forward;

            float rad = Mathf.Acos((targetTransform.position - enemyTransform.position).z / distance);
            enemyTransform.Rotate(0, rad * 10 * Time.deltaTime, 0);

            //enemyMove = enemyTransform.InverseTransformVector(enemyMove);

            //enemyController.Move(enemyMove * Time.deltaTime);
            //Debug.Log(enemyMove);
            Debug.Log(targetTransform.position);
        }
        else
        {
            //enemyMove = Vector3.zero;
            //enemyController.Move(enemyMove);
        }
    }

}
