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


	private int remainingPlayerMoves;
	private int remainingEnemyMoves;
	// Use this for initialization
	void Start () {


		PlayersTurn = true;
	}
	
	// Update is called once per frame
	void Update ()
	{

		if(!PlayersTurn)
		{
			Dictionary<FlatHexPoint, FlatHexPoint> moves = new Dictionary<FlatHexPoint, FlatHexPoint> ();
			var enemies = BattleManager.GetEnemyPositions ();
			foreach(var enemy in enemies)
			{
				var validTargets = BattleManager.GetValidTargets (enemy);
				if (validTargets.Keys.Count > 0)
				{
					var target = BattleManager.GetClosestPlayer (enemy);
					BattleManager.Attack (enemy, target);
					endEnemyMove ();
				} 
				else
				{
					var player = BattleManager.GetClosestPlayer (enemy);
					var move = BattleManager.GetMaxMove (enemy, player);
					moves.Add (enemy, move);
					endEnemyMove ();
				}
			}
			StartCoroutine (MoveQueue (moves));


		}
	
	}

	#region Turn and Move Management
	public void StartPlayerTurn()
	{
		remainingPlayerMoves =  BattleManager.GetPlayerPositions ().Count ();
	}

	public void EndPlayerTurn()
	{
		PlayersTurn = false;
	}

	public void EndPlayerMove()
	{
		remainingPlayerMoves = remainingPlayerMoves - 1;
		if(remainingPlayerMoves <= 0)
		{
			EndPlayerTurn ();
			startEnemyTurn ();
		}
	}

	private void startEnemyTurn()
	{
		remainingEnemyMoves = BattleManager.GetEnemyPositions ().Count ();
	}

	private void endEnemyTurn()
	{
		PlayersTurn = true;
	}

	private void endEnemyMove()
	{
		remainingEnemyMoves = remainingEnemyMoves - 1;
		if(remainingEnemyMoves <= 0)
		{
			endEnemyTurn ();
			StartPlayerTurn ();
		}
	}
	#endregion

	protected IEnumerator MoveQueue (Dictionary<FlatHexPoint, FlatHexPoint> moveQueue)
	{
			
		var units = moveQueue.Keys.ToList ();

	
		while(!units.IsEmpty())
		{
				if(!Moving)
				{
				BattleManager.MoveUnitFromPointToPoint (units.First (), moveQueue [units.First ()]);
				units.Remove (units.First ());
				}
			yield return null;
		}

		endEnemyTurn ();
		StartPlayerTurn ();
	}
}
