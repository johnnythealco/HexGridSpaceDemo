using UnityEngine;
using System.Collections;
using Gamelogic.Grids;
using Gamelogic;
using System.Collections.Generic;
using UnityEngine.UI;

using System.Linq;

public class GameManager : GridBehaviour<FlatHexPoint> 

{
	
	#region Variables
	public GameObject unitPrefab;
	public GameObject enemyPrefab;
	public TurnManager turn;
	public Fleet PlayerFleet;
	public Fleet EnemyFleet;

	
	
	private IGrid<JKCell, FlatHexPoint> grid;
	public bool somethingSelected;
	public Unit unitSelected;
	public Dictionary<FlatHexPoint, float> validTargets;
	private FlatHexPoint selectedPoint;
	private Dictionary<FlatHexPoint, float> AvailableMoves;
	private FlatHexPoint selectedTarget;


	
	#endregion

	void OnAwake()
	{
		somethingSelected = false;


		validTargets = new  Dictionary<FlatHexPoint, float>(); 
	}
	
	override public void InitGrid ()
	{

		grid = Grid.CastValues<JKCell, FlatHexPoint> ();

				foreach (var point in Grid)
		{
			grid [point].contents = CellContents.Empty;
			grid [point].Cost = 1.0f;
			grid [point].isAccessible = true;


		}
	

		var PlayerStartPoint = new FlatHexPoint (4, 6);
		var EnemyStartPoint = new FlatHexPoint (18, -1);



		DeployFleet (PlayerFleet, PlayerStartPoint);
		DeployFleet (EnemyFleet, EnemyStartPoint);




		turn.StartPlayerTurn ();
	}
		
	#region Unit & Enemy Creation

	private void CreateUnit (FlatHexPoint point, GameObject prefab, CellContents owner)
	{

		GameObject newUnit = Instantiate (prefab, Map [point], Quaternion.identity) as GameObject;
	
		Unit unit = newUnit.GetComponent<Unit> ();
		grid [point].contents = owner;

		grid [point].unit = unit;
		grid [point].isAccessible = false;

	}

	private void CreateEnemy (FlatHexPoint point)
	{

		GameObject newEnemy = Instantiate (enemyPrefab, Map [point], Quaternion.identity) as GameObject;
		Unit unit = newEnemy.GetComponent<Unit> ();
		grid [point].contents = CellContents.Enemy;

		grid [point].unit = unit;
		grid [point].isAccessible = false;

	}

	private void DeployFleet(Fleet Fleet, FlatHexPoint center)
	{
		var deployment = getDeploymentArea (center, Fleet); 


		for (int i = 0; i < Fleet.ships.Count(); i++) 
		{
			var point = deployment.RandomItem (); 
			CreateUnit (point,Fleet.ships[i], Fleet.owner);
			deployment.Remove (point);
		}
	}




	#endregion
	
	
	#region User Interaction

	public void LeftClickAction ()
	{
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit hit;

		if (Physics.Raycast(ray, out hit))
		{
			Vector3 worldPosition = this.transform.InverseTransformPoint(hit.point);


		var point = Map[worldPosition]; 

		if (turn.PlayersTurn)
		{
			switch (grid [point].contents)
			{
			case CellContents.Player:
				// If Nothing is selected select the Unit	
				if (!somethingSelected)
				{

					SelectUnitAtPoint (point); 
					AvailableMoves = GetAvailableMoves (point);
					HighlightMove (AvailableMoves.Keys);
					HighlightTargets (GetValidTargets (selectedPoint).Keys.ToList()); 

				}
					//deselect the Unit
					else	if(grid[point].unit == unitSelected)
					{
						clearSelection ();
					}
					//Clear selection and select new unit
					if (somethingSelected)
					{
						clearSelection ();
						SelectUnitAtPoint (point); 
						AvailableMoves = GetAvailableMoves (point);
						HighlightMove (AvailableMoves.Keys);
						HighlightTargets (GetValidTargets (selectedPoint).Keys.ToList()); 

					}
				break;

			case CellContents.Empty:
					//Move the selected unit to an empty cell
					if (somethingSelected && AvailableMoves.ContainsKey (point) && unitSelected.remainingActionPoints > 0)
				{
					unitSelected.Face (Map [point]);
					MoveUnitFromPointToPoint (selectedPoint, point);
					turn.EndUnitAction (unitSelected, 1);
					clearSelection ();

				} 
					else 	if (!somethingSelected)
					{
						UnHighlightJKCells ();	
					}
				break;

			case CellContents.Enemy:
					//attack enemy in range
					if (somethingSelected && validTargets.Keys.Contains (point))
				{
						ShowTargetHud (unitSelected, point);
						selectedTarget = point;
						turn.EndUnitAction (unitSelected, 1);


				}
					else 	if (!somethingSelected)
					{
					UnHighlightJKCells ();
					HighlightMove (GetAvailableMoves (point).Keys);
					HighlightTargets (GetValidTargets (point).Keys.ToList()); 
					}
				break;
			}
		
			}
		}
	}

//	public void RightClickAction ()
//	{
//		var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
//
//		RaycastHit hit;
//
//		if (Physics.Raycast (ray, out hit))
//		{
//			Vector3 worldPosition = this.transform.InverseTransformPoint (hit.point);
//
//
//			var point = Map [worldPosition]; 
//
//			ShowTargetHud (point);
//
//		}
//	}

	public void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{

			LeftClickAction ();

		}

		if (Input.GetMouseButtonDown(1))
		{

//			RightClickAction ();

		}
	}


	#endregion

	#region Unit Selection & Movement

	public void MoveUnitFromPointToPoint (FlatHexPoint start, FlatHexPoint end) 
	{
		if (grid[start].unit != null && grid [end].contents == CellContents.Empty && grid [end].isAccessible)
		{
			grid [start].unit.Move (GetWaypoints (start, end));

			grid [end].contents = grid [start].contents;
			grid [end].unit = grid [start].unit;														//register the unit at the new location
			grid [end].isAccessible = false;																	//Mark the JKCell as occupited

			grid [start].unit = null;																					//unregister the unit from their last location
			grid [start].isAccessible = true;																	//Make the now empty JKCell accessible
			grid [start].Cost = 1;
			grid [start].contents = CellContents.Empty;
		}
	}


	public List<Vector3> GetWaypoints(FlatHexPoint start, FlatHexPoint end)
	{
		var path = GetGridPath(start, end); 														//Get the grid path to the target
		List<Vector3> waypoints = new List<Vector3> ();										//Create an empty of waypoints

		foreach(var waypoint in path)
		{
			waypoints.Add (Map [waypoint]);														//Add each step on the pat to the list of waypoints
		}
		return waypoints;
	}


	public void SelectUnitAtPoint (FlatHexPoint point)
	{
		
		unitSelected = grid [point].unit;
		somethingSelected = true;
		selectedPoint = point;
		validTargets = GetValidTargets (point);


	}



	#endregion


	#region Grid Highlighting

	private void ShowTargetHud(Unit unit, FlatHexPoint target)
	{
		BattleHUD.HUD.TargetHealth.value = grid [target].unit.health;
		BattleHUD.HUD.TargetText.text = grid [target].unit.name;
		BattleHUD.HUD.TargetImage.sprite = grid [target].unit.image;
		BattleHUD.HUD.TargetPanel.SetActive (true);
		BattleHUD.HUD.Weapon0Text.text = unit.weapons [0].weaponName;
		BattleHUD.HUD.Weapon1Text.text = unit.weapons [1].weaponName;
	}

	public void HighlightMove (IEnumerable<FlatHexPoint> JKCells)
	{
		//Activates the border sprite on the set of JKCells provided
		foreach (var point in JKCells)
		{
			grid [point].border.enabled = true;
			grid [point].border.color = Color.blue;
		}

	}

	public void UnHighlightJKCells ()
	{
		//deactivates the border on the set of JKCells provided
		foreach (var point in Grid)
		{
			grid [point].border.enabled = false;
		}

	}

	public void HighlightTargets (List<FlatHexPoint> targets)
	{

		foreach (var point in targets)
		{
			grid [point].border.enabled = true;
			grid [point].border.color = Color.red;

		}

	}

	void clearSelection ()
	{
		somethingSelected = false;								//Set somethingSelected to false
		unitSelected = null;									//Set the unitSelected to null

		UnHighlightJKCells ();
		AvailableMoves.Clear ();
		validTargets.Clear ();
	}

	#endregion


	#region Move & Target Algorithms

	public Dictionary<FlatHexPoint, float> GetAvailableMoves (FlatHexPoint point)
	{
		//Returns a C# Dictionary of move points available and their movement cost, 
		//Returns null if there is no unit in the selected JKCell

		if (grid [point].unit != null)
		{

			var AvailableMoves = Algorithms.GetPointsInRangeCost<JKCell, FlatHexPoint>

			(grid, point,
				JKCell => JKCell.isAccessible,
//				(p, q) => (grid [p].Cost + grid [q].Cost / 2.0f),
				(p, q) => grid [q].Cost,
				grid [point].unit.movement
			);
				
			return AvailableMoves;
		}

		return null;
	}


	public Dictionary<FlatHexPoint, float> GetValidTargets (FlatHexPoint point)
	{
		float maxAttackRange = grid [point].unit.movement + grid [point].unit.weapons[0].attackRange;
		Dictionary<FlatHexPoint, float> result = new Dictionary<FlatHexPoint, float> ();
		List<FlatHexPoint> enemies = new List<FlatHexPoint> ();

		if(grid[point].contents == CellContents.Player)
		{
			enemies = GetEnemyPositions ();
		}
		else if(grid[point].contents == CellContents.Enemy)
		{
			enemies = GetPlayerPositions ();
		}

	
		foreach (var enemy in enemies)
		{
			var path = GetGridPath (point, enemy);
			{
				float pathcost = 0.0f;
				foreach(var step in path) 
				{
					pathcost = pathcost + grid [step].Cost; 
				}
				if(pathcost <= maxAttackRange)
				{
					result.Add (enemy, pathcost);
				}
			}

		}

			return result;
		}

	public List<FlatHexPoint> GetGridPath (FlatHexPoint start, FlatHexPoint end)
	{
		List<FlatHexPoint> result = new List<FlatHexPoint> ();

		var path = Algorithms.AStar<JKCell, FlatHexPoint>
			(grid, start, end
			,
			(p, q) => p.DistanceFrom(q),
			c => true,
			(p, q) =>  (grid [p].Cost + grid [q].Cost / 2) 
		);

		result = path.ToList ();

//		foreach(var step in path.ToList ())
//		{
//			if(grid[step].isAccessible)
//			{
//				result.Add (step);
//			}
//		}


		return result;
	}

	public FlatHexPoint GetMaxMove (FlatHexPoint source, FlatHexPoint target)  
	{
		//Get the best complete path to the target
		var path = GetGridPath (source, target);

		//Get a kvp of all available moves and the cost
		var range =  Algorithms.GetPointsInRangeCost<JKCell, FlatHexPoint>
													(grid, source,
														JKCell => JKCell.isAccessible,
														(p, q) => grid [q].Cost,
														grid [source].unit.movement);

		//Start point
		FlatHexPoint waypoint = source;

		//itterate through every step in the path that is in range 
		//Return the point with the highest cost
	
		foreach (var step in path)
		{
			float maxMoveCost = 0.0f;
			if (range.Keys.Contains (step))
			{
//				grid [step].Color = Color.blue;
				float thisMoveCost = range [step];

					if (thisMoveCost > maxMoveCost)
					{
						waypoint = step; 
						maxMoveCost = thisMoveCost;
					}
			}
		}

//		grid [waypoint].Color = Color.red;
		return waypoint;
	}

	public List<FlatHexPoint> GetEnemyPositions()
	{
		List<FlatHexPoint> result = new List<FlatHexPoint> ();

		foreach (var point in Grid)
		{
			if (grid [point].contents == CellContents.Enemy)
				result.Add (point);
	
		}
		return result;
	}
  

	public List<FlatHexPoint> GetPlayerPositions()
	{
		List<FlatHexPoint> result = new List<FlatHexPoint> ();

		foreach (var point in Grid)
		{
			if (grid [point].contents == CellContents.Player)
				result.Add (point);

		}
		return result;
	}

	public FlatHexPoint GetClosestPlayer(FlatHexPoint point)
	{
		
		var playerPositions = GetPlayerPositions ();
		float lowestCost = 10000000f;
		FlatHexPoint closestPlayer = new FlatHexPoint ();

		foreach( var playerPosition in playerPositions )
		{
			var path = GetGridPath (point, playerPosition); 
				float pathCost = 0.0f;
				foreach(var step in path)
				{
				pathCost = pathCost + grid [step].Cost;
				}
			if( pathCost < lowestCost)
			{
				lowestCost = pathCost;
				closestPlayer = playerPosition;
			}

		}
		return closestPlayer;
	}




	public List<FlatHexPoint> getDeploymentArea(FlatHexPoint point, Fleet fleet)
	{
		var result = Algorithms.GetPointsInRange<JKCell, FlatHexPoint>
		(grid, point,
			JKCell => JKCell.isAccessible,
			(p, q) => 2,
			fleet.ships.Count()
		);
		return result.ToList();
	}


	#endregion

	public void WeaponSelect(int slot)
	{
		Attack (selectedPoint, selectedTarget, slot);
	}

	public void Attack(FlatHexPoint source, FlatHexPoint destination, int slot)
	{
		if (grid [destination].unit != null && grid[source].unit != null)
		{
			
			var attacker = grid [source].unit;
			var target = grid [destination].unit;

			attacker.Face (Map [destination]);
			attacker.weapons[slot].FireAt (Map [destination]); 

			target.health = target.health - attacker.weapons[slot].damage;

			ShowTargetHud (attacker, destination);


			if(target.health <= 0)
			{
				BattleHUD.HUD.TargetPanel.SetActive (false);
				StartCoroutine (UnitDestruction (destination));
			}	
			clearSelection ();
			turn.EndPlayerMove ();
		}
	}

	public void CheckIfBattleOver()
	{
		int playerUnits = GetPlayerPositions ().Count ();
		int enemyUnits = GetEnemyPositions ().Count ();

		if (playerUnits <= 0)
		{
			BattleHUD.HUD.BattleOverText.text = "! Defeat !"; 
			BattleHUD.HUD.BattleOverText.enabled = true;
		}
		else if (enemyUnits <= 0 )
		{
			BattleHUD.HUD.BattleOverText.text = "! Victory !";
			BattleHUD.HUD.BattleOverText.enabled = true; 
		}
		
	}

	protected IEnumerator UnitDestruction ( FlatHexPoint unit)
	{
		turn.Moving = true;
		while (grid [unit].unit != null)
		{
			if (!turn.Fireing)
			{
				grid [unit].contents = CellContents.Empty;
				grid [unit].unit.DestroyUnit ();
				grid [unit].isAccessible = true;
				turn.Moving = false;
			}
			CheckIfBattleOver ();
			yield return null;
		
		}
	}


}
