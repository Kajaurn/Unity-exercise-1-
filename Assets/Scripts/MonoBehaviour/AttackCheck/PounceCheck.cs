using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PounceCheck : MonoBehaviour
{
    public Collider pounceCollider;
    public void PounceEnable()
    {
        pounceCollider.enabled = true;
    }
    public void PounceDisable()
    {
        pounceCollider.enabled = false;
    }
}
