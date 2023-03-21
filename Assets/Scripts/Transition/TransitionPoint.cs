using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionPoint : MonoBehaviour
{
    public enum TransitionType
    {
        SameScene,
        DifferentScene
    }

    [Header("Transition Info")]
    public string sceneName;
    public TransitionType transitionType;
    public TransitionDestination.DestinationTag destinationTag;

    private bool canTrans;

    private bool isTransition;

    private void Update()
    {
        Transitional();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canTrans = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canTrans = false;
        }
    }

    private void Transitional()
    {
        isTransition = GameManager.Instance.playerController.isTransition;
        //Debug.Log(isTransition);
        if (isTransition && canTrans)
        {
            //TODO:ScreenController´«ËÍ
            SceneController.Instance.TransitionToDestination(this);
            Debug.Log("Transition");
        }
    }
}
