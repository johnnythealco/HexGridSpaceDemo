using UnityEngine;
using System.Collections;

public class ProjectileWeapon : Weapon
{
	public float shotSpeed = 5000f;

	private TurnManager turnManager;


	void Start()
	{
		turnManager = TurnManager.turn;

	}

	public override void FireAt(Vector3 target)
	{

		StartCoroutine (projectile(target)); 

	}

	protected IEnumerator projectile ( Vector3 target) 
	{

		turnManager.Fireing = true;
		GameObject projectile = Instantiate (Armament, transform.position, transform.rotation) as GameObject;
		projectile.GetComponent<AudioSource>().Play ();
		float sqrRemainingDistance = (projectile.transform.position - target).sqrMagnitude; //sqrMagnitude is cheaper on the CPU than Magnitude 

		while (sqrRemainingDistance > float.Epsilon) //Epsion is the smallest value that a float can have different from zero.
		{
			Vector3 newPosition = Vector3.MoveTowards (projectile.transform.position, target, shotSpeed * Time.deltaTime);  
			projectile.transform.position = newPosition;
			sqrRemainingDistance = (projectile.transform.position - target).sqrMagnitude;

			yield return null;

		}
		while(projectile.GetComponent<AudioSource>().isPlaying)
		{
			yield return null;
		}

		turnManager.Fireing = false;
		Destroy (projectile);

	}

}
