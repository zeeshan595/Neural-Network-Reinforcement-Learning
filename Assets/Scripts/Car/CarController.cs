using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
	public enum DriveType
	{
		FRONT_WHEEL,
		REAR_WHEEL,
		FOUR_WHEEL
	};

	//Get Car Objects
	public	Rigidbody				body				= null;
	public 	WheelCollider			front_left;
	public 	WheelCollider			front_right;
	public 	WheelCollider			rear_left;
	public 	WheelCollider			rear_right;

	//Car Properties
	public 	float 					acceleration		= 50.0f;
	public 	float					max_speed			= 180.0f;
	public 	float 					brake_force			= 100.0f;
	public	DriveType				drive_type			= DriveType.FRONT_WHEEL;
	public 	float					anti_roll_force		= 100000.0f;
	public 	float					wheels_offset		= 0.5f;
	public 	float					resistance			= 10.0f;

	//Get Wheel Mesh
	private GameObject				front_left_mesh		= null;
	private GameObject				front_right_mesh	= null;
	private GameObject				rear_left_mesh		= null;
	private GameObject				rear_right_mesh		= null;

	private void Start()
	{
		front_left_mesh 	= front_left.gameObject.transform.GetChild(0).gameObject;
		front_right_mesh 	= front_right.gameObject.transform.GetChild(0).gameObject;
		rear_left_mesh 		= rear_left.gameObject.transform.GetChild(0).gameObject;
		rear_right_mesh		= rear_right.gameObject.transform.GetChild(0).gameObject;
	}

	private void Update()
	{
		//Update wheels height		
		UpdateWheelMesh();
		//Simulate anti roll-bar
		AntiRollBar();
	}

	public void Steer(float angle)
	{
		//Turn car
		front_left.steerAngle 	= angle * 35.0f;
		front_right.steerAngle 	= angle * 35.0f;
		//Update wheel's rotation
		front_left_mesh.transform.localRotation 	= Quaternion.Euler(new Vector3(0, angle * 35.0f, 90.0f));
		front_right_mesh.transform.localRotation 	= Quaternion.Euler(new Vector3(0, angle * 35.0f, 90.0f));
	}

	public void Accelerate(float gas)
	{
		//Remove all Break Force Before accelerating
		front_left.brakeTorque	= 0.0f;
		front_right.brakeTorque	= 0.0f;
		rear_left.brakeTorque	= 0.0f;
		rear_right.brakeTorque	= 0.0f;

		if (body.velocity.magnitude < max_speed)
		{
			if (drive_type == DriveType.FRONT_WHEEL || drive_type == DriveType.FOUR_WHEEL)
			{
				front_left.motorTorque 	= gas * acceleration * 10.0f;
				front_right.motorTorque	= gas * acceleration * 10.0f;
			}
			if (drive_type == DriveType.REAR_WHEEL || drive_type == DriveType.FOUR_WHEEL)
			{
				rear_left.motorTorque 	= gas * acceleration * 10.0f;
				rear_right.motorTorque	= gas * acceleration * 10.0f;
			}
		}
		else
		{
			front_left.motorTorque 	= 0.0f;
			front_right.motorTorque	= 0.0f;
			rear_left.motorTorque 	= 0.0f;
			rear_right.motorTorque	= 0.0f;
		}
	}

	public void Brake()
	{
		front_left.brakeTorque	= brake_force * 10.0f;
		front_right.brakeTorque	= brake_force * 10.0f;
		rear_left.brakeTorque	= brake_force * 10.0f;
		rear_right.brakeTorque	= brake_force * 10.0f;
	}

	private void AntiRollBar()
	{
		WheelHit hit;
		float travelL = 1.0f;
		float travelR = 1.0f;
	
		bool groundedL = front_left.GetGroundHit(out hit);
		if (groundedL)
			travelL = (-front_left.transform.InverseTransformPoint(hit.point).y - front_left.radius) / front_left.suspensionDistance;
	
		bool groundedR = front_right.GetGroundHit(out hit);
		if (groundedR)
			travelR = (-front_right.transform.InverseTransformPoint(hit.point).y - front_right.radius) / front_right.suspensionDistance;
	
		float antiRollForce = (travelL - travelR) * anti_roll_force;
	
		if (groundedL)
			body.AddForceAtPosition(front_left.transform.up * -antiRollForce, front_left.transform.position); 
		if (groundedR)
			body.AddForceAtPosition(front_right.transform.up * antiRollForce, front_right.transform.position); 
	}

	private void UpdateWheelMesh()
	{
		WheelHit hit;
		if (front_left.GetGroundHit(out hit))
		{
			front_left_mesh.transform.localPosition = new Vector3(
				0,
				hit.point.y - front_left.transform.position.y + wheels_offset,
				0
			);
		}
		if (front_right.GetGroundHit(out hit))
		{
			front_right_mesh.transform.localPosition = new Vector3(
				0,
				hit.point.y - front_right.transform.position.y + wheels_offset,
				0
			);
		}
		if (rear_left.GetGroundHit(out hit))
		{
			rear_left_mesh.transform.localPosition = new Vector3(
				0,
				hit.point.y - rear_left.transform.position.y + wheels_offset,
				0
			);
		}
		if (rear_right.GetGroundHit(out hit))
		{
			rear_right_mesh.transform.localPosition = new Vector3(
				0,
				hit.point.y - rear_right.transform.position.y + wheels_offset,
				0
			);
		}
	}
}