using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindPlayer : MonoBehaviour
{

    Animator animator;
    int findPlayerHash;
    //Transform enemyTransform;

    //public Transform playerTransform;
    //public Vector3 playerPosition;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInParent<Animator>();
        findPlayerHash = Animator.StringToHash("FindPlayer");
        //playerPosition = playerTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        animator.SetBool(findPlayerHash, true);
        Debug.Log("I find you!");
    }

    //void ChasePlayer() 
    //{
        //enemyTransform.Translate(playerPosition);
    //}
}
