using UnityEngine;
using System.Collections;

// Makes this visible in the editor
[System.Serializable]
/// <summary>
/// The class that stores card values on generation and keeps track of the card's position
/// </summary>
public class Card 
{
	// Needed for storing card info
	public int uniqueID;
	public GameObject obj;
	public float fitness;

	// Variables for card stats and types
	public bool minion;
	/*public bool spell;*/
	public bool charge;
	public bool taunt;
	public int manaCost;
	public int attackPower;
    public int healthPointsStatic;
	public int healthPoints;

    // Used in combat
	public bool canAttack;
	public bool canKill;
	public bool canSurvive;

    // Where the card currently is
	public enum POSITION
	{
		UNUSED,
		DECK,
		HAND,
		INPLAY,
		GRAVEYARD
	}
	public POSITION cardPosition;

    // The weight of paying or attacking with this card at any given time
    public int playValue = 0;
		
	// For storing minion card data
	public Card(int _ID, GameObject _obj, float _fitness, bool _minion, /*bool _spell,*/ bool _charge, bool _taunt, int _mana, int _power, int _healthStatic, int _health, bool _canAttack, int _playValue)
	{
		uniqueID = _ID;
		obj = _obj;
		fitness = _fitness;
		minion = _minion;
		/*spell = _spell;*/
		charge = _charge;
		taunt = _taunt;
		manaCost = _mana;
		attackPower = _power;
        healthPointsStatic = _healthStatic;
		healthPoints = _health;
		canAttack = _canAttack;
        playValue = _playValue;
	}
}
