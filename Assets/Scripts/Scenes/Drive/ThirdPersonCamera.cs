using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {

	public GameObject target;
    public Vector3 offset;
    public float look_offset = 5.0f;

	private void Update()
    {
        //Position
        Vector3 target_pos = target.transform.position + target.transform.TransformDirection(offset);
        transform.position = target_pos;

        //Rotation
        Vector3 look_offset_vec = target.transform.TransformDirection(Vector3.forward) * look_offset;
        transform.LookAt(target.transform.position + look_offset_vec);
    }
}