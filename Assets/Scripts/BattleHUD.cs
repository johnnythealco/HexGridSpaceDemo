using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour 
{
	public static BattleHUD HUD;
	public GameObject TargetPanel;
	public Text TargetText;
	public Image TargetImage;
	public  Slider TargetHealth;
	public GameManager grid;
	public Text BattleOverText;

	// Use this for initialization
	void Start () 
	{
		HUD = this;
	
	}
	

}
