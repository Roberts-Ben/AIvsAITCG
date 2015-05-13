using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Saves and displays the stats for each card as they are updated
/// </summary>
public class CardInfo : MonoBehaviour 
{
	public GameObject main;
    public GameObject cardObj;
	public CardManager cardManager;
    // To reference the info stored in the 'CardManager' list
	public Card thisCard;
	// List of each textmesh and image on the card for displaying stats
	public List<TextMesh> textMeshes = new List<TextMesh>();
    public List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    public List<Material> cardTextures = new List<Material>();

	public bool minion;
	/*public bool spell;*/
	public bool charge;
	public bool taunt;

	// Card stats
	public int manaCost;
	public int attackDamage;
	public int healthPointsStatic;
    public int healthPoints;

	public int ID;
	public TextMesh manaText;
	public TextMesh attackText;
	public TextMesh hpText;
    public SpriteRenderer manaImg;
    public SpriteRenderer attackImg;
    public SpriteRenderer hpImg;

    // Genetic Algorithm (GA) variables
	public int turnDrawn;
	public int turnPlayed;
	public float fitness;
	
	void Start () 
	{
		main = GameObject.Find ("Main");
		cardManager = main.GetComponent<CardManager>();
        cardObj = this.gameObject;
		// Check every card in the list
		foreach(Card card in cardManager.cardCollection)
		{
			// When we find THIS card in that list
			if(card.obj == gameObject)
			{
				// Take the ID
				ID = card.uniqueID;
				thisCard = card;
			}
		}
		// Stores each of the textmeshes on the card
		foreach(TextMesh text in GetComponentsInChildren<TextMesh>())
		{
			textMeshes.Add(text);
		}
        foreach(SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
        {
            sprites.Add(sprite);
        }

		// Set each textmesh and get the value from the list of cards
		manaText = textMeshes[2];
        manaImg = sprites [1];
		manaText.renderer.enabled = false;
        manaImg.enabled = false;
        manaText.renderer.material.color = Color.black;
		manaText.text = "" + thisCard.manaCost;

		attackText = textMeshes[1];
        attackImg = sprites [2];
        attackText.renderer.enabled = false;
        attackImg.enabled = false;
        attackText.renderer.material.color = Color.black;
		attackText.text = "" + thisCard.attackPower;

		hpText = textMeshes[0];
        hpImg = sprites [0];
        hpText.renderer.enabled = false;
        hpImg.enabled = false;
        hpText.renderer.material.color = Color.black;
		healthPointsStatic = thisCard.healthPoints;
		hpText.text = "" + healthPoints;
	}

	void Update () 
	{
        // If the card is not currently in play
		if(thisCard.cardPosition == Card.POSITION.DECK || thisCard.cardPosition == Card.POSITION.GRAVEYARD)
		{
            // disable the text and images so they look like blank cards
            cardObj.renderer.material = cardTextures[0];
            manaText.renderer.enabled = false;
            manaImg.enabled = false;
            attackText.renderer.enabled = false;
            attackImg.enabled = false;
            hpText.renderer.enabled = false;
            hpImg.enabled = false;
		}
		// If not then display the card and it's values
		else
		{
            if(charge)
            {
                cardObj.renderer.material = cardTextures[1];
            }
            if(taunt)
            {
                cardObj.renderer.material = cardTextures[2];
            }
            manaText.renderer.enabled = true;
            manaImg.enabled = true;
            attackText.renderer.enabled = true;
            attackImg.enabled = true;
            hpText.renderer.enabled = true;
            hpImg.enabled = true;
			hpText.text = "" + healthPoints;
		}
		hpText.text = "" + healthPoints;
		thisCard.fitness = fitness;
	}
}
