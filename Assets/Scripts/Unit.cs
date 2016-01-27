using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour {

	#region Variables
	public float movement = 3;
	public float moveTime = 0.1f;
	public float rotateTime = 2.0f;
	public int health = 100;
	public int damage = 50;
	public float attackRange = 1;
	public TurnManager turnmanager; 
	
	private float inverseMoveTime;
	#endregion

	#region Start & Update
	protected virtual void Start ()
	{
		inverseMoveTime = 1 / moveTime;
		this.transform.Rotate (0, -90, 90);

	}


	void Update () {

	}
	#endregion

	#region Getters & Setters
	public void SetPosition (Vector3 newPosition)
	{
		transform.position = newPosition;
	}

	public Vector3 GetPosition ()
	{
		return transform.position;
	}
	#endregion
	
	#region Movement & Rotation
	public void Move (List<Vector3> waypoints)
	{

		StartCoroutine (SmoothMovement (waypoints));
	}

	public void Face(Vector3 position)
	{

		transform.LookAt (position, transform.up);
	


//
	}
	#endregion

	public void TakeDamage(int damage)
	{
		health = health - damage;

		if(health <= 0)
		{
			Destroy (this.gameObject);
		}
	}



	#region Co-Routines
	protected IEnumerator SmoothMovement (List<Vector3> waypoints)
	{
		 turnmanager.Moving = true; 
//		 animator.SetBool ("Walking", true);
		foreach (var waypoint in waypoints)
		{
			Face (waypoint);
			float sqrRemainingDistance = (transform.position - waypoint).sqrMagnitude; //sqrMagnitude is cheaper on the CPU than Magnitude

			while (sqrRemainingDistance > float.Epsilon) //Epsion is the smallest value that a float can have different from zero.
			{
				Vector3 newPosition = Vector3.MoveTowards (transform.position, waypoint, inverseMoveTime * Time.deltaTime);
				transform.position = newPosition;
				sqrRemainingDistance = (transform.position - waypoint).sqrMagnitude;

				yield return null;

			}
		}
		// animator.SetBool ("Walking", false);
		 turnmanager.Moving = false;

	}
	#endregion
}
