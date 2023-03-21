using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCheck : MonoBehaviour
{
    public Collider weaponCol;
    public void AttackEnable()
    {
        weaponCol.enabled = true;
    }
    public void AttackDisable()
    {
        weaponCol.enabled = false;
    }
    //public void LeftFistEnable()
    //{
    //    weaponCol.enabled = true;
    //}
    //public void LeftFistDisable()
    //{
    //    weaponCol.enabled = false;
    //}
    //public void RightFistEnable()
    //{
    //    weaponCol.enabled = true;
    //}
    //public void RightFistDisable()
    //{
    //    weaponCol.enabled = false;
    //}
}
