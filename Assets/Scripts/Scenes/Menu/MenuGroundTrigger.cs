using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuGroundTrigger : MonoBehaviour
{
    public GameObject ground;
    public GameObject mover;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Car")
        {
            ground.transform.position = mover.transform.position;
        }
    }
}