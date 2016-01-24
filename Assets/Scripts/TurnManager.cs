using UnityEngine;
using System.Collections;
using Gamelogic.Grids;
using Gamelogic;
using System.Collections.Generic;
using System.Linq;
public class TurnManager : MonoBehaviour {

	public bool PlayersTurn;
	public bool Moving{ get; set;}


	public GameManager BattleManager;
	// Use this for initialization
	void Start () {

		PlayersTurn = true;
	}
	
	// Update is called once per frame
	void Update () {

		if(!PlayersTurn)
		{
			Dictionary<FlatHexPoint, FlatHexPoint> moves = new Dictionary<FlatHexPoint, FlatHexPoint> ();
			var enemies = BattleManager.GetEnemyPositions ();
			foreach(var enemy in enemies)
			{
				var player = BattleManager.GetClosestPlayer (enemy);
				var move = BattleManager.GetMaxMove (enemy, player);
				moves.Add (enemy, move);
			}
			StartCoroutine (MoveQueue (moves));
			PlayersTurn = true;

		}
	
	}


	protected IEnumerator MoveQueue (Dictionary<FlatHexPoint, FlatHexPoint> moveQueue)
	{
			
		var units = moveQueue.Keys.ToList ();

	
		while(!units.IsEmpty())
		{
				if(!Moving)
				{
				BattleManager.MoveUnitFromPointToPoint (units.First (), moveQueue [units.First ()]);
//				units.First().Move (moveQueue [units.First()]);
//				moveQueue.Remove (units.First ());
				units.Remove (units.First ());
				}
			yield return null;
		}


	}
}
