using UnityEngine;
using System.Collections;
using Gamelogic.Grids;
using Gamelogic;
using System.Collections.Generic;
using System.Linq;
public class TurnManager : MonoBehaviour {

	public bool PlayersTurn;
	public bool Moving{ get; set;}
	public bool Fireing {get; set;}
	public static TurnManager turn;



	public GameManager BattleManager;


	private int remainingPlayerMoves;
	private int remainingEnemyMoves;

	void Awake()
	{
		if(turn == null)
		{
			turn = this;
		}
	}



	

	void Update ()
	{

		if(!PlayersTurn && !Moving && !Fireing)
		{
			EnemyMoveAndAttack ();
		}
	
	}

	#region Turn and Move Management
	public void EndUnitAction(Unit unit,int APCost)
	{
		BattleManager.CheckIfBattleOver ();
		unit.remainingActionPoints = unit.remainingActionPoints - APCost;

		if(unit.remainingActionPoints <= 0)
		{
			EndPlayerMove ();
		}
	}

	public void StartPlayerTurn()
	{
		PlayersTurn = true;
		remainingPlayerMoves =  BattleManager.GetPlayerPositions ().Count ();

		foreach ( var obj in BattleManager.PlayerFleet.ships)
		{
			var unit = obj.GetComponent<Unit> ();
			unit.remainingActionPoints = unit.actionPoints;
		}
	}


	public void EndPlayerMove()
	{

		remainingPlayerMoves = remainingPlayerMoves - 1;
		if(remainingPlayerMoves <= 0)
		{
			startEnemyTurn ();
		}
	}

	private void startEnemyTurn()
	{
		PlayersTurn = false;
		remainingEnemyMoves = BattleManager.GetEnemyPositions ().Count (); 
	}


	private void endEnemyMove()
	{
		BattleManager.CheckIfBattleOver ();
		remainingEnemyMoves = remainingEnemyMoves - 1;
		if(remainingEnemyMoves <= 0)
		{
			StartPlayerTurn ();
		}
	}
	#endregion

	void EnemyMoveAndAttack()
	{
		Dictionary<FlatHexPoint, FlatHexPoint> attacks = new Dictionary<FlatHexPoint, FlatHexPoint> ();
		Dictionary<FlatHexPoint, FlatHexPoint> moves = new Dictionary<FlatHexPoint, FlatHexPoint> ();
		var enemies = BattleManager.GetEnemyPositions ();
		foreach(var enemy in enemies)
		{
			var validTargets = BattleManager.GetValidTargets (enemy);
			if (validTargets.Keys.Count > 0)
			{
				var target = BattleManager.GetClosestPlayer (enemy);
				attacks.Add (enemy, target);

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
		StartCoroutine (AttackQueue (attacks));
		StartCoroutine (MoveQueue (moves));


	}

	protected IEnumerator AttackQueue (Dictionary<FlatHexPoint, FlatHexPoint> attackQueue)
	{
		yield return new WaitForSeconds (0.8f);
		var units = attackQueue.Keys.ToList ();


		while(!units.IsEmpty())
		{
			if(!Moving && !Fireing)
			{
				BattleManager.Attack (units.First (), attackQueue [units.First ()], 0);

				units.Remove (units.First ());
				yield return new WaitForSeconds (0.3f);
			}
			yield return null;
		}


	}

	protected IEnumerator MoveQueue (Dictionary<FlatHexPoint, FlatHexPoint> moveQueue)
	{
			
		var units = moveQueue.Keys.ToList ();

	
		while(!units.IsEmpty())
		{
			if(!Moving&& !Fireing)
				{
				BattleManager.MoveUnitFromPointToPoint (units.First (), moveQueue [units.First ()]);
				units.Remove (units.First ());
				}
			yield return null;
		}

		StartPlayerTurn ();
	}
}
