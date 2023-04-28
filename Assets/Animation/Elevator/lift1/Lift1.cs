using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lift1 : MonoBehaviour
{
    Animator animator;

    public enum Lift1Posture
    {
        top,
        bottom
    }
    [HideInInspector]
    public Lift1Posture lift1Posture = Lift1Posture.top;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        
        //if (other.tag == "PlayerWeapon")
        //{
            Debug.Log("set");
            if (lift1Posture == Lift1Posture.top)
            {
            Debug.Log("down");
                animator.SetTrigger("Down");
                lift1Posture = Lift1Posture.bottom;
            Debug.Log(lift1Posture);
            }
            else
            {
                animator.SetTrigger("Up");
                lift1Posture = Lift1Posture.top;
            }
        //}
    }
}
