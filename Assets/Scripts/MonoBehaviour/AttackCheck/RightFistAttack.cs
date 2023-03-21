using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightFistAttack : MonoBehaviour
{
    public Collider weaponCol;
    public void RightFistEnable()
    {
        weaponCol.enabled = true;
    }
    public void RightFistDisable()
    {
        weaponCol.enabled = false;
    }
}
