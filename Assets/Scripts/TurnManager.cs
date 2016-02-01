using UnityEngine;
using System.Collections;
using Gamelogic.Grids;
using Gamelogic;
using System.Collections.Generic;
using System.Linq;
public class TurnManager : MonoBehaviour {

	public bool PlayersTurn;
	public bool Moving{ get; set;}
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


	void Start () {


		PlayersTurn = true;
	}
	
	// Update is called once per frame
	void Update ()
	{

		if(!PlayersTurn && !Moving)
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
//					BattleManager.Attack (enemy, target);
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
		BattleManager.CheckIfBattleOver ();
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
		BattleManager.CheckIfBattleOver ();
		remainingEnemyMoves = remainingEnemyMoves - 1;
		if(remainingEnemyMoves <= 0)
		{
			endEnemyTurn ();
			StartPlayerTurn ();
		}
	}
	#endregion

	protected IEnumerator AttackQueue (Dictionary<FlatHexPoint, FlatHexPoint> attackQueue)
	{
		yield return new WaitForSeconds (0.5f);
		var units = attackQueue.Keys.ToList ();


		while(!units.IsEmpty())
		{
			if(!Moving)
			{
				BattleManager.Attack (units.First (), attackQueue [units.First ()]);

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
