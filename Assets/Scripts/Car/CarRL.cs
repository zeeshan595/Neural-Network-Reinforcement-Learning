using System.Collections.Generic;
using System.Threading;
using System;
using UnityEngine;

public class CarRL : MonoBehaviour
{

    private     CarAI           car_ai          = null;
    private     Rigidbody       car_body        = null;

	private void Start()
    {
        car_ai = GetComponent<CarAI>();
        car_body = gameObject.transform.GetChild(0).gameObject.GetComponent<Rigidbody>();

    }
}