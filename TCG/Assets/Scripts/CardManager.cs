using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates the list of cards to be used and stores the variables associated
/// </summary>
public class CardManager : MonoBehaviour 
{
    // The physical object
	public GameObject cardObj;
    // How many cards need to be read and stored
    public int cardTotal;
    // The list that contains every card before the decks are pulled
	public List<Card> cardCollection = new List<Card>();
    // Once the decks are generated, this prevents it from repeating endlessly
    public bool cardsGenerated;

	void Start () 
	{
        // If the decks have not been pulled yet
		cardsGenerated = false;
        // Check every card
		for(int i = 0; i < cardTotal; i++)
		{
            // Load in the prefab
			cardObj = (GameObject)Resources.Load("" + (i + 1));
            // Spawn it in
			GameObject card = (GameObject)Instantiate(cardObj, new Vector3(0, 1, 0), Quaternion.identity);
			
            // Get the script to store the stats
			CardInfo cardInfo = card.GetComponent<CardInfo>();
			
			bool minion = cardInfo.minion;
			/*bool spell = cardInfo.spell;*/
			bool charge = cardInfo.charge;
			bool taunt = cardInfo.taunt;
			int attack = cardInfo.attackDamage;
			int health = cardInfo.healthPoints;
			int mana = cardInfo.manaCost;
			float fitness = cardInfo.fitness;
			
            // Add this card to the list, storing the stats above
			cardCollection.Add(new Card(i, card, fitness, minion, /*spell,*/ charge, taunt, mana, attack, health, health, true, 0));
		}
		cardsGenerated = true;
	}
}
