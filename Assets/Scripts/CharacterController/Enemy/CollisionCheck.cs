using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCheck : MonoBehaviour
{
    Animator animator;
    CharacterStats enemyStats;

    int deadHash;
    int reactionHash;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInParent<Animator>();
        deadHash = Animator.StringToHash("Dead");
        reactionHash = Animator.StringToHash("Reaction");

        enemyStats = GetComponentInParent<CharacterStats>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //TODO:ʵ�ֹ����ж��͹�����ֵ����
    private void OnTriggerEnter(Collider other)
    {
        //animator.SetBool(deadHash, true);
        animator.SetTrigger(reactionHash);
        Debug.Log("hit");
        Debug.Log(other.name);

        var targetStats = other.GetComponentInParent<CharacterStats>();
        enemyStats.TakeDamage(targetStats, enemyStats);
    }
}
