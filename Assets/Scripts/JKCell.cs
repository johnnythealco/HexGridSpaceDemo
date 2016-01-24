using UnityEngine;
using System.Collections;
using Gamelogic.Grids;
using Gamelogic;

public class JKCell :  SpriteCell
{
	public Unit unit;
	public SpriteRenderer border;
	public CellContents contents;
	public bool IsAccessible;
	public float Cost;


}

public enum CellContents{Empty, Player, Enemy}