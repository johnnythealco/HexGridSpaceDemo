using UnityEngine;
using System.Collections;

public class BeamWeapon : Weapon 
{

	private TurnManager turnManager;
	private LineRenderer line;

	void Start()
	{
		turnManager = TurnManager.turn;

	}


	public override void FireAt(Vector3 target)
	{

		StartCoroutine (Beam(target)); 

	}

	protected IEnumerator Beam ( Vector3 target)
	{
		turnManager.Fireing = true;

		GameObject laser = Instantiate (Armament, transform.position, transform.rotation) as GameObject; 
		line = laser.GetComponent<LineRenderer> ();
		line.SetPosition (0, transform.position);
		line.SetPosition (1, target);
		laser.GetComponent<AudioSource>().Play ();
		yield return new WaitForSeconds (0.3f);
		Destroy (laser);

		turnManager.Fireing = false;
	}

}
