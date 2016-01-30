using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	public GameObject Bolt;
	public Transform spawnPoint;
	public float shotSpeed = 5000f;

	private TurnManager turnManager;
	private LineRenderer line;

	void Start()
	{
		turnManager = GetComponent<Unit> ().turnmanager;

	}

	public void FireAt(Vector3 target)
	{
		
		GameObject laser = Instantiate (Bolt, spawnPoint.position, spawnPoint.rotation) as GameObject;
		line = laser.GetComponent<LineRenderer> ();
		line.SetPosition (0, spawnPoint.position);
		line.SetPosition (1, target);
//		StartCoroutine (SmoothMovement (target));
		GetComponent<AudioSource>().Play ();
		Destroy (laser, 0.3f);


	}

	protected IEnumerator SmoothMovement ( Vector3 waypoint) 
	{
		
		turnManager.Moving = true;
		GameObject bolt = Instantiate (Bolt, spawnPoint.position, spawnPoint.rotation) as GameObject;
		float sqrRemainingDistance = (bolt.transform.position - waypoint).sqrMagnitude; //sqrMagnitude is cheaper on the CPU than Magnitude 

			while (sqrRemainingDistance > float.Epsilon) //Epsion is the smallest value that a float can have different from zero.
			{
			Vector3 newPosition = Vector3.MoveTowards (bolt.transform.position, waypoint, shotSpeed * Time.deltaTime);  
				bolt.transform.position = newPosition;
				sqrRemainingDistance = (bolt.transform.position - waypoint).sqrMagnitude;

				yield return null;

			}

		turnManager.Moving = false;
		Destroy (bolt);

	}

}
