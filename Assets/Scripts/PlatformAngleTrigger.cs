using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlatformAngleTrigger : MonoBehaviour
{

    public AngleTrigger AngleTriggerCallback = new AngleTrigger();

    [System.Serializable]
    public class AngleTrigger : UnityEvent {}

    private void OnTriggerEnter(Collider col)
    {
        AngleTriggerCallback.Invoke();
    }
}
