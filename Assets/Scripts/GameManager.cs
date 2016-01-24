using UnityEngine;
using System.Collections;
using Gamelogic.Grids;
using Gamelogic;
using System.Collections.Generic;

using System.Linq;

public class GameManager : GridBehaviour<FlatHexPoint> 

{
	
	#region Variables
	public GameObject unitPrefab;
	public GameObject enemyPrefab;
	public TurnManager turn;
	
	
	private IGrid<JKCell, FlatHexPoint> grid;
	public bool somethingSelected;
	public Unit unitSelected;
	private FlatHexPoint selectedPoint;
	private Dictionary<FlatHexPoint, float> AvailableMoves;
	public List<FlatHexPoint> validTargets;
	private FlatHexPoint enemyPosition; 
	
	#endregion
	
		override public void InitGrid ()
	{
		somethingSelected = false;
		grid = Grid.CastValues<JKCell, FlatHexPoint> ();
		validTargets = new List<FlatHexPoint> ();
		
				foreach (var point in Grid)
		{
			grid [point].contents = CellContents.Empty;
			grid [point].Cost = 1.0f;
			grid [point].IsAccessible = true;
		}
	
	
		var rndUnits = Grid.SampleRandom (3);
		foreach (var point in rndUnits)
		{
			CreateUnit (point);
		}

		var rndEnemies = Grid.SampleRandom (3);
		foreach (var point in rndEnemies)
		{
			 CreateEnemy (point);
		}
	}
		
		#region Unit & Enemy Creation

	private void CreateUnit (FlatHexPoint point)
	{

		GameObject newUnit = Instantiate (unitPrefab, Map [point], Quaternion.identity) as GameObject;
	
		Unit unit = newUnit.GetComponent<Unit> ();
		grid [point].contents = CellContents.Player;

		grid [point].unit = unit;
		grid [point].IsAccessible = false;
		unit.turnmanager = turn;
	}

	private void CreateEnemy (FlatHexPoint point)
	{

		GameObject newEnemy = Instantiate (enemyPrefab, Map [point], Quaternion.identity) as GameObject;
		Unit unit = newEnemy.GetComponent<Unit> ();
		grid [point].contents = CellContents.Enemy;

		grid [point].unit = unit;
		grid [point].IsAccessible = false;
		unit.turnmanager = turn;

	}

	public Unit GetUnitAtPoint(FlatHexPoint point)
	{
		return grid [point].unit;
	}

	#endregion
	
	
	#region User Interaction

	public void OnLeftClick (FlatHexPoint point)
	{


		if (turn.PlayersTurn)
		{
			switch (grid [point].contents)
			{
			case CellContents.Player:
				if (!somethingSelected)
				{
					SelectUnitAtPoint (point); 
					AvailableMoves = GetAvailableMoves (point);
					validTargets = GetValidTargets ();
					HighlightMove (AvailableMoves.Keys);
					HighlightTargets (validTargets);
				}
				break;

			case CellContents.Empty:
				if (somethingSelected && AvailableMoves.ContainsKey (point))
				{
					unitSelected.Face (Map [point]);
					MoveUnitFromPointToPoint (selectedPoint, point);
					EndAction ();
					turn.PlayersTurn = false;

				} 
				break;

			case CellContents.Enemy:
				if (somethingSelected && validTargets.Contains (point))
				{
					// unitSelected.AttackAnnimation ();
					EndAction ();
					turn.PlayersTurn = false;
				}
////				else if (!somethingSelected)
////				{
////					grid [GetClosestPlayer (point)].Color = Color.red;
////				
////				}
				break;
			}
		}
	}

	#endregion

	#region Unit Selection & Movement

	public void MoveUnitFromPointToPoint (FlatHexPoint start, FlatHexPoint end) 
	{
		grid [end].unit = grid[start].unit;														//register the unit at the new location
		grid [end].IsAccessible = false;																	//Mark the JKCell as occupited
		grid[end].contents = grid[start].contents;


		grid [start].unit.Move (GetWaypoints (start, end));

		grid[start].unit = null;																					//unregister the unit from their last location
		grid[start].IsAccessible = true;																	//Make the now empty JKCell accessible
		grid[start].contents = CellContents.Empty;
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
		validTargets.Clear ();
		unitSelected = grid [point].unit;
		somethingSelected = true;
		selectedPoint = point;


	}



	#endregion

	#region Grid Highlighting

	public void HighlightMove (IEnumerable<FlatHexPoint> JKCells)
	{
		//Activates the border sprite on the set of JKCells provided
		foreach (var point in JKCells)
		{
			grid [point].border.enabled = true;
//			grid [point].border.color = Color.blue;
		}

	}

	public void UnHighlightJKCells (IEnumerable<FlatHexPoint> JKCells)
	{
		//deactivates the border on the set of JKCells provided
		foreach (var point in JKCells)
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

	public void EndAction ()
	{
		somethingSelected = false;								//Set somethingSelected to false
		unitSelected = null;									//Set the unitSelected to null
		UnHighlightJKCells (AvailableMoves.Keys);
		UnHighlightJKCells (validTargets);
		AvailableMoves.Clear ();
		validTargets.Clear ();
	}

	#endregion


	#region Moves & Target Algorithms

	public Dictionary<FlatHexPoint, float> GetAvailableMoves (FlatHexPoint point)
	{
		//Returns a C# Dictionary of move points available and their movement cost, 
		//Returns null if there is no unit in the selected JKCell

		if (grid [point].unit != null)
		{

			var AvailableMoves = Algorithms.GetPointsInRangeCost<JKCell, FlatHexPoint>

			(grid, point,
				JKCell => JKCell.IsAccessible,
				(p, q) => (grid [p].Cost + grid [q].Cost / 2.0f),
				grid [point].unit.movement
			);
				
			return AvailableMoves;
		}

		return null;
	}

	public List<FlatHexPoint> GetValidTargets ()
	{
		List<FlatHexPoint> result = new List<FlatHexPoint> ();

		if (unitSelected != null && AvailableMoves != null)
		{
			foreach (var pointInRange in AvailableMoves.Keys)
			{
				//For each point the unit can move to
				//Check what enemies and in range of that point
				var EnemiesInRangeFromPoint = Algorithms.GetPointsInRange<JKCell, FlatHexPoint>
												(grid,
					                            pointInRange,
					JKCell => (JKCell.contents == CellContents.Enemy),
					                            (p, q) => 1,
					                            unitSelected.attackRange);
								
				//Add the any valid target to the result
				//if the target is not already in the list
				foreach (var target in EnemiesInRangeFromPoint)
				{
					if (grid [target].contents == CellContents.Enemy && !result.Contains(target))
					{
						result.Add (target);

					}
						
				}
			}

			return result;
			
		}

		return null;

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
//		result.Remove (start);

		return result;
	}

	public FlatHexPoint GetMaxMove (FlatHexPoint source, FlatHexPoint target)  
	{
		//Get the best complete path to the target
		var path = GetGridPath (source, target);

		//Get a kvp of all available moves and the cost
		var range =  Algorithms.GetPointsInRangeCost<JKCell, FlatHexPoint>
													(grid, source,
														JKCell => JKCell.IsAccessible,
														(p, q) => (grid [p].Cost + grid [q].Cost / 2.0f),
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
				grid [step].Color = Color.blue;
				float thisMoveCost = range [step];

					if (thisMoveCost > maxMoveCost)
					{
						waypoint = step; 
						maxMoveCost = thisMoveCost;
					}
			}
		}

		grid [waypoint].Color = Color.red;
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


	#endregion



}
