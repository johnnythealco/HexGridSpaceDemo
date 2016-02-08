using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Unit : MonoBehaviour {

	#region Variables
	public string unitName;
	public Sprite image;
	public float movement = 3;
	public float MoveSpeed = 100f;

	public float health = 100;
	public int actionPoints = 2;
	public GameObject Explosion;



	public Weapon[] weapons;





	
	private float inverseMoveTime;
	public int remainingActionPoints;
	#endregion

	#region Start & Update
	protected virtual void Start ()
	{

		this.transform.Rotate (0, -90, 90);


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

		transform.LookAt (position,transform.up );
	}
	#endregion



	public void DestroyUnit()
	{
		GameObject explosion = Instantiate (Explosion, this.transform.position, this.transform.rotation) as GameObject;
	Destroy (this.gameObject);
	}



	#region Co-Routines
	protected IEnumerator SmoothMovement (List<Vector3> waypoints)
	{
		TurnManager.turn.Moving = true; 
//		 animator.SetBool ("Walking", true);
		foreach (var waypoint in waypoints)
		{
			Face (waypoint);
			float sqrRemainingDistance = (transform.position - waypoint).sqrMagnitude; //sqrMagnitude is cheaper on the CPU than Magnitude

			while (sqrRemainingDistance > float.Epsilon) //Epsion is the smallest value that a float can have different from zero.
			{
				Vector3 newPosition = Vector3.MoveTowards (transform.position, waypoint,MoveSpeed * Time.deltaTime);
				transform.position = newPosition;
				sqrRemainingDistance = (transform.position - waypoint).sqrMagnitude;

				yield return null;

			}
		}
		// animator.SetBool ("Walking", false);
		TurnManager.turn.Moving = false;

	}
	#endregion
}
