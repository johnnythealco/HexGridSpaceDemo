using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	public GameObject Bolt;
	public Transform spawnPoint;
	public float shotSpeed = 5000f;


	void OnStart()
	{

	}

	public void FireAt(Vector3 target)
	{
		

		StartCoroutine (SmoothMovement (target));


	}

	protected IEnumerator SmoothMovement ( Vector3 waypoint) 
	{
			GameObject bolt = Instantiate (Bolt, spawnPoint.position, spawnPoint.rotation) as GameObject;
			float sqrRemainingDistance = (bolt.transform.position - waypoint).sqrMagnitude; //sqrMagnitude is cheaper on the CPU than Magnitude 

			while (sqrRemainingDistance > float.Epsilon) //Epsion is the smallest value that a float can have different from zero.
			{
			Vector3 newPosition = Vector3.MoveTowards (bolt.transform.position, waypoint, shotSpeed * Time.deltaTime);  
				bolt.transform.position = newPosition;
				sqrRemainingDistance = (bolt.transform.position - waypoint).sqrMagnitude;

				yield return null;

			}


	}

}
