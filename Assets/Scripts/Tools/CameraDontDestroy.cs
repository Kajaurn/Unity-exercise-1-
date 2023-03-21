using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDontDestroy : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
