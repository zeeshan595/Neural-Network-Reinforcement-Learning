using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObserverCamera : MonoBehaviour
{
    private Camera my_cam;

    private void Start()
    {
        my_cam = GetComponent<Camera>();
    }

	private void Update()
    {
        my_cam.fieldOfView -= Input.mouseScrollDelta.y;
        my_cam.fieldOfView = Mathf.Clamp(my_cam.fieldOfView, 20.0f, 60.0f);

        Vector3 new_position = transform.position;
        
        float mouse_x = (Input.mousePosition.x / Screen.width) - 0.5f;
        float mouse_y = (Input.mousePosition.y / Screen.height) - 0.5f;
        new_position.x = mouse_x * 30.0f;
        new_position.z = mouse_y * 30.0f;


        new_position.x = Mathf.Clamp(new_position.x, -30.0f, 30.0f);
        new_position.z = Mathf.Clamp(new_position.z, -30.0f, 30.0f);
        
        transform.position = Vector3.Lerp(transform.position, new_position, Time.deltaTime * 2.0f);
    }
}
