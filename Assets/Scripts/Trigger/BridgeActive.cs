using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeActive : MonoBehaviour
{
    public GameObject bridge;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PlayerWeapon")
        {
            bridge.SetActive(true);
        }
    }
}
