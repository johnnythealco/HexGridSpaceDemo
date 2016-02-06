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
	public Text Weapon0Text;
	public Text Weapon1Text;


	// Use this for initialization
	void Awake () 
	{
		HUD = this;
		BattleOverText.enabled = false;
		TargetPanel.SetActive (false);

	
	}
	

}
