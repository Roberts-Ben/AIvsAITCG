using UnityEngine;
using System.Collections;

/// <summary>
/// The Hero class stores the variables and objects needed to show the hero's stats
/// </summary>
public class Hero : MonoBehaviour
{
	// Which hero this is referencing, true = hero A, false = hero B
	public bool heroA;
	// The stats that will be shown
	public int totalMana;
	public int currentMana;
	public int health;
	// The objects that will show the stats
	public TextMesh manaText;
	public TextMesh healthText;
	// Referencing the GameManager script as this holds all of the hero stats
	public GameObject main;
	public GameManager gameManager;

	void Start()
	{
        // Get the GameManager script from the object it is attached to
		main = GameObject.Find ("Main");
		gameManager = main.GetComponent<GameManager>();
	}

	void Update()
	{
		// If we are referencing hero A
		if(heroA)
		{
			// Assign the relevant stats
			totalMana = gameManager.heroAMaxMana;
			currentMana = gameManager.heroAMana;
			health = gameManager.heroAHealth;
		}
		// If it is hero B
		else
		{
			totalMana = gameManager.heroBMaxMana;
			currentMana = gameManager.heroBMana;
			health = gameManager.heroBHealth;
		}
		// Show the stats
		manaText.text = "Mana : " + currentMana + " / " + totalMana;
		healthText.text = "Health : " + health;
	}
}
