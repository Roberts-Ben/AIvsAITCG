using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

// Make this class visible in the editor
[System.Serializable]
/// <summary>
/// Store any combinations of cards that we might be able to play
/// </summary>
public class PotentialPlays
{
    // The cards in this play
	public List<Card> cards = new List<Card>();
    // How strong the play is
    public int playID;
    public int totalAttack;
	public int sing_Attack;
	public int sing_Health;

	public PotentialPlays (List<Card> _cards, int _totalAttack, int _playID)
	{
        // If there are any cards stored
		if(_cards.Count > 0)
		{
            // Check each one
			foreach(Card card in _cards)
			{
                // Store it in the list for use later
				cards.Add(card);
			}
		}
		totalAttack = _totalAttack;
		playID = _playID;
	}
}

[System.Serializable]
/// <summary>
/// Store any combinations of cards that we might be able to attack with
/// </summary>
public class CombatPotentialPlays
{
	public List<Card> cards = new List<Card>();
    // The card or hero being attacked
	public Card enemyInPlayCard; 
	public int heroHealth;
    // Can the card survive the attack or kill what it is aiming for?
	public bool survival = false;
	public bool kill = false;
    // How strong the play is
	public int totalAttack = 0;
	public int power = 0;
	public int playID = 0;
	
    // Attacking an enemy card
	public CombatPotentialPlays (List<Card> _cards, Card _enemyInPlayCard, bool _survival, bool _kill, int _totalAttack, int _power, int _playID)
	{
		if(_cards.Count > 0)
		{
			foreach(Card card in _cards)
			{
				cards.Add(card);
			}
		}
		enemyInPlayCard = _enemyInPlayCard;
		survival = _survival;
		kill = _kill;
		totalAttack = _totalAttack;
		power = _power;
		playID = _playID;
	}

    // Attacking the enemy hero
	public CombatPotentialPlays (List<Card> _cards, int _heroHealth, int _totalAttack, int _power, int _playID)
	{
		if(_cards.Count > 0)
		{
			foreach(Card card in _cards)
			{
				cards.Add(card);
			}
		}
		heroHealth = _heroHealth;
		totalAttack = _totalAttack;
		power = _power;
		playID = _playID;
	}
}

/// <summary>
/// This is where all gameplay and UI related actions occur
/// </summary>
public class GameManager : MonoBehaviour
{
    // What turn/round is being played
    public int drawCount;
    public bool heroA;
    public bool firstTurn;
    private int currentRound = 1;
    private int currentTurn = 1;
    private bool gameActive;
	private bool paused;
    // Stats for each hero
    public int heroAHealth;
    public int heroBHealth;
    public int heroAMana;
	public int heroAMaxMana;
    public int heroBMana;
	public int heroBMaxMana;
    // Decks for each hero
    public int deckCapacityA;
    public int deckCapacityB;
    public bool checkedBestCards;
    private int deckSizeA;
    private int deckSizeB;
    // The 'best' deck
    public List<Card> bestDeckComposition = new List<Card>();
    // How many 'best' cards we are picking before random ones
	public int maxFitnessCount;
    // Delay between each turn/phase change to see what happened
	public float turnTimer;
	public bool waitForInvoke;
    // Stats for the playes being calculated
	public int playToPlay;
	public int highestAtt;
	public int playID;
	public int combatPlayToPlay;
	public int highestPower;
	public int combatPlayID;
	public bool reEvaluateCombat;
    // The 5 positions a card can be played to
	public List<GameObject> playPositionsA = new List<GameObject>();
	public List<GameObject> playPositionsB = new List<GameObject>();
    // The various places cards can be for each hero (Horribly un-optimised)
    public List<Card> deckA = new List<Card>();
    public List<Card> handA = new List<Card>();
    public List<Card> deckB = new List<Card>();
    public List<Card> handB = new List<Card>();
    public List<Card> inPlayA = new List<Card>();
    public List<Card> inPlayB = new List<Card>();
    public List<Card> graveyardA = new List<Card>();
    public List<Card> graveyardB = new List<Card>();
    // The cards being considered to play/attack with
    public List<Card> cardsPlayable = new List<Card>();
	public List<PotentialPlays> potentialPlay = new List<PotentialPlays>();
	public List<Card> bestHand = new List<Card>();
	public List<CombatPotentialPlays> combatPotentialPlay = new List<CombatPotentialPlays>();
	public List<Card> combatCard = new List<Card>();
	public bool canSurvive; 
	public bool canKill;
    // Checking if the cards have all been evaluated yet or not
    public List<Card> bestCards = new List<Card>();
    public bool checkedCards;
	public bool readyToPlay;
	public bool turnEnded;
	public bool playsCreated;
    // UI elements
	public Button startRound;
	public Button nextRound;
	public Text nextRoundText;
	public Text currentPhaseText;
	public Text pauseText;
	public Text turnHistory;
	public Slider speed;
    // What phase it currently is for each hero
    public enum TURNSTATE
    {
        DRAW,
        PLAY,
        COMBAT,
        ENDED,
        RESET
    }
    public TURNSTATE turnState;
    private CardManager m_cardManager;

    void Start()
    {
		// Get this script so that we can reference variables inside it 
        m_cardManager = GetComponent<CardManager>();
    }

    void Update()
	{
        // If the list of cards have been generated and the game is still going
        if (m_cardManager.cardsGenerated && gameActive)
        {
            // Draw the decks for both heroes
            DrawDecks(deckA, deckSizeA, deckCapacityA);
            DrawDecks(deckB, deckSizeB, deckCapacityB);

            // Update the text in the top right to show what phase it is
			currentPhaseText.text = "Current Phase: Pre-Game";

			// Place the cards in the appropriate places
            foreach (Card card in m_cardManager.cardCollection)
            {
                // Unused cards
                card.obj.transform.position = new Vector3(20, 1, 0);
            }
            foreach (Card card in deckA)
            {
                // Deck A
                card.obj.transform.position = new Vector3(13.75f, 1, -6.8f);
				card.cardPosition = Card.POSITION.DECK;
            }
            foreach (Card card in deckB)
            {
                // Deck B
                card.obj.transform.position = new Vector3(-13.75f, 1, 6.8f);
				card.cardPosition = Card.POSITION.DECK;
            }
            // Stop pulling cards into the decks
            m_cardManager.cardsGenerated = false;
        }
        // If the game has ended, stop it
        if (turnState == TURNSTATE.RESET)
        {
			gameActive = false;
			nextRoundText.text = ("Start Round " + currentRound);
			nextRound.gameObject.SetActive(true);
        }

		// While the game can still be played
		if (gameActive)
        {
            StateCheck();
        }
	}
    // Call this function after a delay, this passes through the active hero and all of their cards
	void TurnInvoke()
	{
		Turn(true, heroAHealth, heroAMana, deckA, handA, cardsPlayable, inPlayA, inPlayB);
	}

	void TurnInvokeB()
	{
		Turn(false, heroBHealth, heroBMana, deckB, handB, cardsPlayable, inPlayB, inPlayA);
	}

    /// <summary>
    /// Draw a deck of 15 cards for both heroes
    /// </summary>
    /// <param name="deck">Deck.</param>
    /// <param name="deckSize">Deck size.</param>
    /// <param name="deckCapacity">Deck capacity.</param>
    public void  DrawDecks(List<Card> deck, int deckSize, int deckCapacity)
    {
        // While the hero has less than 15 cards
        while (deckSize < deckCapacity)
        {
            // If this is the first round, the decks will be purely random
            if(currentRound == 1 || checkedBestCards)
            {
                // Pull random card from the collection
                int rand = Random.Range(0, m_cardManager.cardCollection.Count);
                
                Card card = m_cardManager.cardCollection[rand];
                // Remove it so that it can't be picked again
                m_cardManager.cardCollection.Remove(card);
                // Add it to the hero's deck
                deck.Add(card);
                deckSize++;
            }
            else
            {
                // If we still have 'best' cards left to choose
                if(!checkedBestCards)
                {
                    // Store the current 'best' card
                    float highestFitness = 0f;
                    Card highestCard = null;
                    if(deck.Count != maxFitnessCount)
                    {
                        highestFitness = 0f;
                        highestCard = null;
                        // Check each card in the collection
                        foreach(Card cardToCheck in m_cardManager.cardCollection)
                        {
                            // If it is 'better' than the one currently stored
                            if(cardToCheck.fitness > highestFitness)
                            {
                                // Update it to reflect this
                                highestCard = cardToCheck;
                                highestFitness = highestCard.fitness;
                                checkedBestCards = false;
                            }
                        }
                        // If we have checked every card and found the 'best'
                        if(highestCard != null)
                        {
                            Card card = highestCard;
                            // Remove it so that it can't be picked again
                            m_cardManager.cardCollection.Remove(card);
                            // Add it to the hero's deck
                            deck.Add(card);
                            deckSize++;
                        }
                    }
                    // If we have run out of cards that are above '0' fitness i.e. never used
                    if(highestCard == null)
                    {
                        // Stop checking for 'best' cards and pick random ones form here on
                        checkedBestCards = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if the game is still active and switch turns when needed
    /// </summary>
    void StateCheck()
    {
        // If any hero can still draw, play cards or is alive
        if(deckA.Count > 0 || handA.Count > 0 || deckB.Count > 0 || handB.Count > 0 || inPlayA.Count > 0 || inPlayB.Count > 0 || heroAHealth > 0 || heroBHealth > 0)
        {
            // If an enemy card has been killed mid combat, pick a new target with the remaining cards
            if(turnState == TURNSTATE.COMBAT && reEvaluateCombat)
            {
                // If it is hero A's turn, call the function with their cards being used
                if(heroA)
                {
                    waitForInvoke = true;
                    reEvaluateCombat = false;
                    Invoke("TurnInvoke", turnTimer);
                }
                else
                {
                    waitForInvoke = true;
                    reEvaluateCombat = false;
                    Invoke("TurnInvokeB", turnTimer);
                }
            }
            // Once the turn is over, switch the active hero
            if(turnState == TURNSTATE.ENDED)
            {
                if(heroA)
                {
                    // If this is the first turn of the round the heroes draw more cards
                    if(firstTurn)
                    {
                        drawCount = 4;
                        
                    }
                    heroA = false;
                }
                else
                {
                    if(firstTurn)
                    {
                        firstTurn = false;
                    }
                    drawCount = 1;
                    heroA = true;
                }
                currentTurn++;
                turnState = TURNSTATE.DRAW;
            }
            // If the turn is not yet over, play the heroes turn until they run out of moves
            else
            {
                if(heroA)
                {
                    if(!waitForInvoke)
                    {
                        waitForInvoke = true;
                        Invoke("TurnInvoke", turnTimer);
                    }
                }
                else
                {
                    if(!waitForInvoke)
                    {
                        waitForInvoke = true;
                        Invoke("TurnInvokeB", turnTimer);
                    }
                }
            }
        }
        // If a hero runs out of HP, end the current round
        if(heroAHealth <= 0 || heroBHealth <= 0)
        {
            if(heroAHealth <= 0)
            {
                turnHistory.text += ("\n----------\n Hero B has won this game");
            }
            else if(heroBHealth <= 0)
            {
                turnHistory.text += ("\n----------\n Hero A has won this game");
            }
            currentRound++;
            currentPhaseText.text = "Current Phase: Finished";
            turnState = TURNSTATE.RESET;
        }
        // Update the speed of the game via the slider
        turnTimer = speed.value;
    }

	/// <summary>
	/// One turn involves drawing, playing, atacking and resolving.
	/// Pass in the required lists of cards depending on which players turn it is.
	/// </summary>
	/// <param name="heroATurn">Determines which hero's turn it is.</param>
	/// <param name="heroHP">Remaining health of the hero.</param>
	/// <param name="heroMana">total/available mana.</param>
	/// <param name="deck">The hero's current deck.</param>
	/// <param name="hand">The cards the hero has in their hand.</param>
	/// <param name="playable">The cards that the hero can physically play this turn.</param>
	/// <param name="inPlay">The cards that the hero has in play.</param>
    /// /// <param name="inPlay">The cards that the enemy hero has in play.</param>
	public void Turn(bool heroATurn, int heroHP, int heroMana, List<Card> deck, List<Card> hand, List<Card> playable, List<Card> inPlay, List<Card> inPlayEnemy)
	{
		switch(turnState)
		{
            // If it is the draw phase, pull the required number of cards from the deck
            case TURNSTATE.DRAW:
                currentPhaseText.text = "Current Phase: Draw";
    			if(heroA)
    			{	
    				if (firstTurn)
    				{
    					turnHistory.text += ("\n----------\n Hero A has drawn " + drawCount + " Cards");
    				}
    				else
    				{
    					turnHistory.text += ("\n----------\n Hero A has drawn a card");
    				}
    			}
    			else
    			{
    				if (firstTurn)
    				{
    					turnHistory.text += ("\n----------\n Hero B has drawn " + drawCount + " Cards");
    				}
    				else
    				{
    					turnHistory.text += ("\n----------\n Hero B has drawn a card");
    				}
    			}
    			playID = 0;
    			float handPos = -3.64f;
                // If the hero still has cards to draw and has the space
    			if(deck.Count > 0)
    			{
					// Draw the number of cards needed
    				for (int i = 0; i < drawCount; i++)
    				{
						// Pick a random card from the deck
    					int rand = Random.Range(0, deck.Count);
    					Card card = deck[rand];
						// If we have space for the card
    					if(handA.Count < 6 || handB.Count < 6)
    					{
						// Add it to the hand and remove it from the deck
    						hand.Add(card);
    						CardInfo cardInfo = card.obj.GetComponent<CardInfo>();
    						cardInfo.fitness += 0.2f;
    						cardInfo.turnDrawn = currentTurn;
    						deck.Remove(card);
    					}
    				}
    			}
    			// If we have no cards left in the deck, remove a hitpoint
    			else
    			{
    				if(heroA)
    				{
                        turnHistory.text += ("\n----------\n Hero A cannot draw a card and has lost 1HP ");
    					heroAHealth -= 1;
    				}
    				else
    				{
                        turnHistory.text += ("\n----------\n Hero B cannot draw a card and has lost 1HP ");
    					heroBHealth -= 1;
    				}
    			}
                // Organise the cards left in the heroes hand
    			foreach (Card card in hand)
    			{
    				if(heroATurn)
    				{
    					card.obj.transform.position = new Vector3(handPos, 1, -6.78f);
    				}
    				else
    				{
    					card.obj.transform.position = new Vector3(handPos - 2.66f, 1, 6.78f);
    				}
    				card.cardPosition = Card.POSITION.HAND;
    				handPos += 2.46f;
    			}
                // Set each card that is already in play from the last round to be able to attack
    			foreach(Card card in inPlay)
    			{
    				card.canAttack = true;
    			}
                // Add another 1 mana to the pool if they are not already at max
    			if(heroATurn && heroAMaxMana < 10)
    			{
    				heroAMaxMana++;
    			}
    			else if(!heroATurn && heroBMaxMana < 10)
    			{
    				heroBMaxMana++;
    			}
    			heroAMana = heroAMaxMana;
    			heroBMana = heroBMaxMana;
                // Switch to the next phase
    			readyToPlay = false;
    			turnState = TURNSTATE.PLAY;
                // Wherever this line appears, the 'InvokeTurn' function will be called after a delay
    			waitForInvoke = false;
    			break;
		
            case TURNSTATE.PLAY:
    			currentPhaseText.text = "Current Phase: Play";
    			float playPos = -4.6f;

    			if(!readyToPlay)
    			{
                    // If there are less than 5 cards already in play
    				if(inPlay.Count < 5)
    				{
    					foreach (Card card in hand)
    					{
                            // If the card costs less than the heroes total mana, we can possibly play it so store it 
    						if (card.manaCost <= heroMana && (inPlay.Count + cardsPlayable.Count < 5) && card.minion)
    						{
    							cardsPlayable.Add(card);
    						}
    						/*else if (card.manaCost <= heroMana && card.spell)
    						{
    							cardsPlayable.Add(card);
    						}*/
    					}
    				}
                    // If we have no more possible cards to play, move to the next phase
    				if(cardsPlayable.Count == 0)
    				{
    					turnState = TURNSTATE.COMBAT;
    				}

    				// Check the cards the hero can play this turn
    				for(int i = 0; i < cardsPlayable.Count; i++)
    				{
    					bestCards.Clear ();
                        // Store the total attack power of the first card in this list as a potential play
    					Card cardToCompare = cardsPlayable[i];
    					bestCards.Add (cardToCompare);
						// Store the play with the cards associated with it
    					potentialPlay.Add(new PotentialPlays(bestCards, cardToCompare.attackPower, playID));
    					playID++;
    			
    					int startingCard = 0;
    					int totalMana = 0; 
    					int prevMana = 0;
    					
                        // If we can possibly play more than one card, start looping through the rest
    					for(int d = 0; d < cardsPlayable.Count; d++)
    					{
    						// The total mana cost of this play is updated with the first card
    						totalMana = cardToCompare.manaCost;

                            // After checking the first potential card in the list with every other combination, we want to check the second card in the list on it's own, the with other combinations
    						for(int c = startingCard; c < cardsPlayable.Count; c++)
    						{
    							bestCards.Clear ();
    							bestCards.Add (cardToCompare);
    							// Check the other cards that are playable
    							if(cardToCompare != cardsPlayable[c] && !bestCards.Contains(cardsPlayable[c]))
    							{
    								totalMana += cardsPlayable[c].manaCost;
    								// Store any combinations and possible moves for this turn (Compares two cards)
    								if (totalMana <= heroMana)
    								{
    									bestCards.Add(cardsPlayable[c]);
    									int totalAtt = 0;
    									// Total the attack of the cards
    									for(int b = 0; b < bestCards.Count; b++)
    									{
    										totalAtt += bestCards[b].attackPower;
    									}
    									potentialPlay.Add(new PotentialPlays(bestCards, totalAtt, playID));
    									playID++;
    								}
                                    // If we don't have enough mana for this conbo, cancel the storage
    								else 
    								{
    									totalMana -= cardsPlayable[c].manaCost;
    								}

    								prevMana = totalMana;
                                    // After checking two cards, check the rest
    								foreach(Card nextCard in cardsPlayable)
    								{
    									if(!bestCards.Contains(nextCard))
    									{
    										prevMana += nextCard.manaCost;
    										// Store any combinations and possible moves for this turn (Compares the remaining cards) 
    										if (prevMana <= heroMana)
    										{
    											bestCards.Add(nextCard);
    											int totalAtt = 0;
    											int totMana = 0;
    											for(int o = 0; o < bestCards.Count; o++)
    											{
    												totalAtt += bestCards[o].attackPower;
    												totMana += bestCards[o].manaCost;
    											}
    											potentialPlay.Add(new PotentialPlays(bestCards, totalAtt, playID));
    											playID++;
    										}
    										else 
    										{
    											prevMana -= nextCard.manaCost;
    										}
    									}
    								}
    							}
    						}
    						startingCard++;
    					}
    					bestCards.Clear();
                        // Once every combo is checked we move on
    					playsCreated = true;
    				}

    				if(playsCreated)
    				{
                        // Get the 'value' of each potential play
    					highestAtt = 0;
    					playToPlay = 0;

    					PotentialPlays playToCompare = potentialPlay[0];
						// Get the plays we have and compare the cards they contain against the other plays to get the best one
    					foreach(PotentialPlays plays in potentialPlay)
    					{
    						if(plays != playToCompare)
    						{
                                // If this play is 'stronger', store it as the best
    							if(plays.totalAttack >= highestAtt)
    							{
    								highestAtt = plays.totalAttack;
    								playToPlay = plays.playID;
    							}
    						}
    					}
    					PotentialPlays bestHandPotential = potentialPlay[playToPlay];

    					bestHand.Clear();
                        // Move the cards into a list that will move the cards from the hand to the field
    					foreach(Card c in bestHandPotential.cards)
    					{
    						bestHand.Add (c);
    					}
                        // Move on to playing the cards
    					playsCreated = false;
    					readyToPlay = true;
    					waitForInvoke = false;
    					break;
    				}
    			}

    			if (readyToPlay)
    			{
    				// If the hero is out of moves, end the play phase and clear the lists that were used
    				if (bestHand.Count == 0 || heroMana == 0)
    				{
    					cardsPlayable.Clear();
    					potentialPlay.Clear();
    					bestHand.Clear ();
    					combatCard.Clear ();
    					turnState = TURNSTATE.COMBAT;
    				}
                    // The cards that the hero is actually going to play are stored here
    	            foreach(Card bestCard in bestHand)
    	            {
                        // If the card is a minion, we place it on the field
    					if(bestCard.minion)
    					{
    						inPlay.Add(bestCard);
    		                if (heroATurn)
    						{
                                // Find a space for this card
                                FindCardSlot(inPlayA, playPositionsA, bestCard);
    		                }
    		                else
    		                {
                                FindCardSlot(inPlayB, playPositionsB, bestCard);
    						}
    	                }
						// UNUSED - This would have played the spell card immediately and removed it after
    					/*else if(bestCard.spell)
    					{
    						if(inPlayEnemy.Count > 0)
    						{
    							Debug.Log ("Spell Used: " + bestCard.manaCost + " " + bestCard.attackPower);
    							//bestCard.cardPosition = Card.POSITION.INPLAY;

    							// Killing first card in the loop ALWAYS
    							foreach(Card card in inPlayEnemy)
    							{
    								Debug.Log (card);
    								if(bestCard.attackPower >= card.healthPoints)
    								{
    									card.healthPoints -= bestCard.attackPower;

    									if(heroATurn)
    									{
    										card.obj.transform.position = new Vector3(13.75f, 1, -2.39f);
    										bestCard.obj.transform.position = new Vector3(-13.75f, 1, 2.39f);
    									}
    									else
    									{
    										card.obj.transform.position = new Vector3(-13.75f, 1, 2.39f);
    										bestCard.obj.transform.position = new Vector3(13.75f, 1, -2.39f);
    									}

    									CardInfo spellCardInfo = bestCard.obj.GetComponent<CardInfo>();
    									spellCardInfo.fitness += 0.5f;
    									CardInfo enemyCardInfo = card.obj.GetComponent<CardInfo>();
    									enemyCardInfo.fitness -= 0.5f;

    									bestCard.cardPosition = Card.POSITION.GRAVEYARD;
    									card.cardPosition = Card.POSITION.GRAVEYARD;

    									if(heroA)
    									{
    										turnHistory.text += ("\n----------\n Hero A has attacked and used a spell card with " + bestCard.manaCost + " mana cost and " + bestCard.attackPower + " Attack");
    									}
    									else
    									{
    										turnHistory.text += ("\n----------\n Hero B has attacked and  used a spell card with " + bestCard.manaCost + " mana cost and " + bestCard.attackPower + " Attack");
    									}
    									inPlayEnemy.Remove(card);
    									bestHand.Remove(bestCard);
    									waitForInvoke = false;
    									break;
    								}
    								else
    								{
    									if(bestCard.attackPower < card.healthPoints)
    									{
    										card.healthPoints -= bestCard.attackPower;

    										if(heroATurn)
    										{
    											bestCard.obj.transform.position = new Vector3(13.75f, 1, 2.39f);
    										}
    										else
    										{
    											bestCard.obj.transform.position = new Vector3(-13.75f, 1, -2.39f);
    										}

    										CardInfo spellCardInfo = bestCard.obj.GetComponent<CardInfo>();
    										spellCardInfo.fitness += 0.2f;
    										Debug.Log ("spell used but could not kill");
    										bestCard.cardPosition = Card.POSITION.GRAVEYARD;

    										if(heroA)
    										{
    											turnHistory.text += ("\n----------\n Hero A has attacked and used a spell card with " + bestCard.manaCost + " mana cost and " + bestCard.attackPower + " Attack");
    										}
    										else
    										{
    											turnHistory.text += ("\n----------\n Hero B has attacked and  used a spell card with " + bestCard.manaCost + " mana cost and " + bestCard.attackPower + " Attack");
    										}
    										
    										bestHand.Remove(bestCard);
    										waitForInvoke = false;
    										break;
    									}
    								}
    							}
    						}
    					}*/
    					// Update the fitness of the card to reflect how long it took to be played
    					CardInfo cardInfo = bestCard.obj.GetComponent<CardInfo>();
    					cardInfo.turnPlayed = currentTurn;
    					cardInfo.fitness += (cardInfo.turnDrawn / currentTurn);
                        // Display text on the left textbox to show what cards have been played
    					if(heroA)
    					{
    						turnHistory.text += ("\n----------\n Hero A has played a " + cardInfo.manaCost + " Mana card with " + cardInfo.attackDamage + " Attack points and " + cardInfo.healthPoints + " health");
    					}
    					else
    					{
    						turnHistory.text += ("\n----------\n Hero B has played a " + cardInfo.manaCost + " Mana card with " + cardInfo.attackDamage + " Attack points and " + cardInfo.healthPoints + " health");
    					}

    					playPos += 2.97f;
                        // Update the remaining mana for this turn
    	                if (heroATurn)
    	                {
    	                    heroAMana -= bestCard.manaCost;
    	                }
    	                else
    	                {
    	                    heroBMana -= bestCard.manaCost;
    	                }
                        // Remove the cards form the hand so that they are not considered next turn
    					bestHand.Remove(bestCard);
    					hand.Remove(bestCard);
    					break;
    				}

    	    		waitForInvoke = false;
    	     		break;
    			}

    			foreach(Card card in inPlay)
    			{
                    // Update the fitness for the cards that are in play to reflect how long they have been alive
    				CardInfo cardInfo = card.obj.GetComponent<CardInfo>();
    				if(cardInfo.turnPlayed != currentTurn)
    				{
    					cardInfo.fitness += ((currentTurn - cardInfo.turnPlayed) / 2);
    				}
    			}

    			waitForInvoke = false;
    			break;

            case TURNSTATE.COMBAT:
                currentPhaseText.text = "Current Phase: Combat";
                if (turnEnded == false)
    			{
                    combatPotentialPlay.Clear();
    				combatCard.Clear ();
    				combatPlayID = 0;
    				int allCardsCombinedAttack = 0;
                    // If we have cards in play this turn
    				if(inPlay.Count > 0)
    				{
    					// If no enemy cards found
    					if(inPlayEnemy.Count < 1)
    					{
    						int totalAtt = 0;
    						// If the card can attack, add it's attack power to a list of potential attacks
    						foreach(Card soloCard in inPlay)
    						{
								// Check the first card in play against the enemies
                                if(soloCard.canAttack)
    							{
    								combatCard.Add (soloCard);
    								totalAtt += soloCard.attackPower;
    							}
    						}

    						// Store the hero as a target
    						int enemyHero = 0;
    						if(heroA)
    						{
    							enemyHero = heroBHealth;
    						}
    						else
    						{
    							enemyHero = heroAHealth;
    						}
    						if(combatCard.Count > 0)
    						{
    							combatPotentialPlay.Add(new CombatPotentialPlays(combatCard, enemyHero, totalAtt, totalAtt, combatPlayID));
    						}
    					}
                        // If the enemy has cards in play
    					else
    					{
						    // Check each card they have
    						for(int i = 0; i < inPlayEnemy.Count; i++)
    						{
    							combatCard.Clear ();
    							int startingCombatCard = 0;
    							allCardsCombinedAttack = 0;
    	
    							// Find the first enemy card
    							Card enemyCard = inPlayEnemy[i];

                                // If it has 'taunt', we are forced to attack it rather than considering other cards
    							if(enemyCard.taunt)
    							{
    								foreach(Card cardsInPlay in inPlay)
    								{
                                        // Throw everything at this card (Not a smart move but the cards needs to be removed ASAP to consider other attacks)
    									if(!combatCard.Contains(cardsInPlay))
    									{
    										combatCard.Add (cardsInPlay);
    										allCardsCombinedAttack += cardsInPlay.attackPower;
    									}
    								}
    								combatPotentialPlay.Add(new CombatPotentialPlays(combatCard, enemyCard, canSurvive, canKill, allCardsCombinedAttack, allCardsCombinedAttack, combatPlayID));
    							}
                                // If there are no 'taunt' cards
    							else
    							{
    								// Start comparing this heroes cards against the enemies
    								for(int c = startingCombatCard; c < inPlay.Count; c++)
    								{
    									combatCard.Clear ();

    									// Check if the card can attack
    									if(inPlay[c].canAttack)
    									{
    										combatCard.Add(inPlay[c]);
    										Card singleCard = combatCard[0];
                                            // Store the attack of this card
    										allCardsCombinedAttack += singleCard.attackPower;

                                            CheckOutcome(singleCard, enemyCard, allCardsCombinedAttack);
    										
                                            // If this hero has more than one card in play
                                            if(inPlay.Count > 1)
    										{
    											// Start comparing combos with other cards
    											for(int r = 0; r < inPlay.Count ; r++)
    											{
    												// Two card comparison
    												if(!combatCard.Contains(inPlay[r]) && inPlay[r].canAttack)
    												{
    													combatCard.Add(inPlay[r]);
    													singleCard = combatCard[1];

                                                        CheckOutcome(inPlay[r], enemyCard, allCardsCombinedAttack);

                                                        // If we still have more cards
    													if(inPlay.Count > 2)
    													{
    														// Start comparing 3, 4 and 5 card combos
    														foreach(Card nextInPlayCard in inPlay)
    														{
    															if(!combatCard.Contains(nextInPlayCard) && !combatCard.Contains(inPlay[r]) && inPlay[r].canAttack)
    															{
    																combatCard.Add(inPlay[r]);
    																singleCard = nextInPlayCard;

                                                                    CheckOutcome(singleCard, enemyCard, allCardsCombinedAttack);
															    }
														    }
													    }
												    }
											    }
                                            }
										    startingCombatCard++;
									    }
								    }
							    }
						    }
					    }
                        // If we have an attack to play this turn
                        if(combatPotentialPlay.Count > 0)
    					{
    						CombatPotentialPlays combatPlayToCompare = combatPotentialPlay[0];
    						highestPower = 0;
    						combatPlayToPlay = 0;
                            // Check each play and store the 'strongest'
    						foreach(CombatPotentialPlays combatPlays in combatPotentialPlay)
    						{
    							if(combatPlays != combatPlayToCompare)
    							{
    								if(combatPlays.power >= highestPower)
    								{
    									highestPower = combatPlays.power;
    									combatPlayToPlay = combatPlays.playID;
    								}
    							}
						    }
    						CombatPotentialPlays bestCombatPotential = combatPotentialPlay[combatPlayToPlay];
                            // Store the thing we are attacking
    						Card enemyCardToAttack = bestCombatPotential.enemyInPlayCard;
    						int enemyHealth = bestCombatPotential.heroHealth;
                            // For each card that we are attacking with
    						foreach(Card cardToAttack in bestCombatPotential.cards)
    						{
    							int cardHealth = cardToAttack.healthPoints;
                                // If we attack a card
    							if(bestCombatPotential.heroHealth == 0)
    							{
                                    // Update the health of that card
    								enemyHealth = enemyCardToAttack.healthPoints;
    								cardHealth -= enemyCardToAttack.attackPower;
								    enemyHealth -= cardToAttack.attackPower; 
                                    // If the card we attacked with is defeated
                                    if(cardHealth <= 0)
    								{
                                        // Move it to the graveyard
    									inPlay.Remove(cardToAttack);
    									if(heroATurn)
    									{
    										cardToAttack.obj.transform.position = new Vector3(13.75f, 1, -2.39f);
    									}
    									else
    									{
    										cardToAttack.obj.transform.position = new Vector3(-13.75f, 1, 2.39f);
    									}
                                        // Update the fitness of the cards involved
    									CardInfo cardInfo = cardToAttack.obj.GetComponent<CardInfo>();
    									cardInfo.fitness -= 0.5f;
    									CardInfo cardInfoEnemy = enemyCardToAttack.obj.GetComponent<CardInfo>();
    									cardInfoEnemy.fitness += 0.5f;

    									cardToAttack.cardPosition = Card.POSITION.GRAVEYARD;
    									if(heroA)
    									{
    										turnHistory.text += ("\n----------\n Hero A has attacked and lost a card with " + cardToAttack.manaCost + " mana cost and " + cardToAttack.attackPower + " Attack");
    									}
    									else
    									{
    										turnHistory.text += ("\n----------\n Hero B has attacked and lost a card with " + cardToAttack.manaCost + " mana cost and " + cardToAttack.attackPower + " Attack");
    									}
    								}
                                    // If the card survives, update it's health still
                                    else
    								{
    									cardToAttack.healthPoints = cardHealth;
    								
    									if(heroA)
    									{
    										turnHistory.text += ("\n----------\n Hero A has attacked a card with " + cardToAttack.manaCost + " mana cost and " + cardToAttack.attackPower + " Attack");
    									}
    									else
    									{
    										turnHistory.text += ("\n----------\n Hero B has attacked a card with " + cardToAttack.manaCost + " mana cost and " + cardToAttack.attackPower + " Attack");
    									}
    								}
								    // If the enemy card is defeated
								    if(enemyHealth <= 0)
								    {
    									inPlayEnemy.Remove(enemyCardToAttack);
    									if(heroATurn)
    									{
    										enemyCardToAttack.obj.transform.position = new Vector3(-13.75f, 1, 2.39f);
    									}
    									else
    									{
    										enemyCardToAttack.obj.transform.position = new Vector3(13.75f, 1, -2.39f);
    									}

    									CardInfo cardInfo = cardToAttack.obj.GetComponent<CardInfo>();
    									cardInfo.fitness += 1.5f;
    									enemyCardToAttack.cardPosition = Card.POSITION.GRAVEYARD;
    									if(combatCard.Count > 1)
    									{
    										combatCard.Clear ();
    									}
    									if(heroA)
    									{
    										turnHistory.text += ("\n----------\n Hero A has attacked and destroyed a card with " + enemyCardToAttack.manaCost + " mana cost and " + enemyCardToAttack.attackPower + " Attack");
    									}
    									else
    									{
    										turnHistory.text += ("\n----------\n Hero B has attacked and destroyed a card with " + enemyCardToAttack.manaCost + " mana cost and " + enemyCardToAttack.attackPower + " Attack");
    									}
                                        // Make sure the card that attacked cannot go again this turn
    									cardToAttack.canAttack = false;
                                        // If we still have cards that were going to attack
    									if(bestCombatPotential.cards.Count >= 1)
    									{
                                            // Re-evaluate the combat as the card being attacked no longer exists
    										reEvaluateCombat = true;
    										return;
    									}
								    }
                                    // If the enemy survives
     								else
    								{
    									enemyCardToAttack.healthPoints = enemyHealth;
    									if(heroA)
    									{
    										turnHistory.text += ("\n----------\n Hero A has attacked a card with " + enemyCardToAttack.manaCost + " mana cost and " + enemyCardToAttack.attackPower + " Attack");
    									}
    									else
    									{
    										turnHistory.text += ("\n----------\n Hero B has attacked a card with " + enemyCardToAttack.manaCost + " mana cost and " + enemyCardToAttack.attackPower + " Attack");
    									}
    									cardToAttack.canAttack = false;

    									if(bestCombatPotential.cards.Count >= 1)
    									{
    										reEvaluateCombat = true;
    										return;
    									}
    								}
							    }
                                // If we are attacking the hero
    							else
    							{
    							    // Update the heroes health and stop the card attacking again
    								enemyHealth -= cardToAttack.attackPower;
    								if(heroATurn)
    								{
    									heroBHealth = enemyHealth;
    									turnHistory.text += ("\n----------\n Hero A has attacked hero B with a " + cardToAttack.attackPower + " attack and " + cardToAttack.manaCost + " mana");
    								}
    								else
    								{
    									heroAHealth = enemyHealth;
    									turnHistory.text += ("\n----------\n Hero B has attacked hero A with a " + cardToAttack.attackPower + " attack and " + cardToAttack.manaCost + " mana");
    								}
    								cardToAttack.canAttack = false;
    								if(bestCombatPotential.cards.Count >= 1)
    								{
    									combatPotentialPlay.Clear ();
    									reEvaluateCombat = true;
    									return;
								    }
							    }
						    }
    					}
                        // If we have nothing to attack with, end the phase
                        else
    					{
    						turnEnded = true;
    					}

    					foreach(Card cardsLeft in inPlay)
    					{
                            // If no cards can attack, end the phase
    						if(!cardsLeft.canAttack)
    						{
    							turnEnded = true;
    						}
    						else
    						{
    							turnEnded = false;
    							return;
    						}
    					}
				    }
                }
                // If the enemy has no cards and we've attacked with everything or we have no cards left to use
                if((inPlayEnemy.Count == 0 && turnEnded) || turnEnded || inPlay.Count == 0)
    			{
    				currentPhaseText.text = "Current Phase: Discard";
                    // If we have more than 5 cards in hand
    				if(hand.Count > 5)
    				{
    					// Throw away the card that we have had the longest (Could be improved to throw away a 'less fit' one)
    					Card card = hand[0];
    					CardInfo cardInfo = card.obj.GetComponent<CardInfo>();
                        // Update the fitness to reflect how long we had the card without being able to use it
    					if(cardInfo.turnDrawn == currentTurn)
    					{
    						cardInfo.fitness -= 0.2f;
    					}
    					else
    					{
    						cardInfo.fitness -= (currentTurn - cardInfo.turnDrawn);
    					}

    					if(heroA)
    					{
    						turnHistory.text += ("\n----------\n Hero A has discarded a card with " + cardInfo.manaCost + " mana cost and " + cardInfo.attackDamage + "damage and " + cardInfo.healthPoints + " health");   
    					}
    					else
    					{
    						turnHistory.text += ("\n----------\n Hero B has discarded a card with " + cardInfo.manaCost + " mana cost and " + cardInfo.attackDamage + "damage and " + cardInfo.healthPoints + " health");
    					}

    					card.cardPosition = Card.POSITION.GRAVEYARD;
    					hand.Remove(card);
    					if(heroATurn)
    					{
    						card.obj.transform.position = new Vector3(13.75f, 1, -2.39f);
    					}
    					else
    					{
    						card.obj.transform.position = new Vector3(-13.75f, 1, 2.39f);
    					}
    				}
				    // Reorganise the hand
                    float handSortPos = -3.64f;
    				foreach (Card cardPos in hand)
    				{
    					if(heroATurn)
    					{
    						cardPos.obj.transform.position = new Vector3(handSortPos, 1, -6.78f);
    					}
    					else
    					{
    						cardPos.obj.transform.position = new Vector3(handSortPos - 2.66f, 1, 6.78f);
    					}
    					cardPos.cardPosition = Card.POSITION.HAND;
    					handSortPos += 2.46f;
    				}
        			turnState = TURNSTATE.ENDED;
        			if(!heroA)
        			{
        				currentTurn++;
        			}
                }

                waitForInvoke = false;
        		turnEnded = false;
                break;
        }
    }

    /// <summary>
    /// Find a space for the cards being played
    /// </summary>
    /// <param name="inPlayHero">In play hero.</param>
    /// <param name="playPositions">Play positions.</param>
    /// <param name="cardToMove">Card to move.</param>
    void FindCardSlot(List<Card> inPlayHero, List<GameObject> playPositions, Card cardToMove)
    {
        // Find the first empty slot on the board by checking the 5 positions
        bool slotFilled = true;
        foreach (GameObject obj in playPositions)
        {
            foreach (Card inPlayCard in inPlayHero)
            {
                // If at any point we find an empty slot, stop checking
                if (inPlayCard.obj.transform.position == obj.transform.position)
                {
                    slotFilled = true;
                    break;
                }
                else
                {
                    slotFilled = false;
                }
            }
            // Move the card into the open space
            if (!slotFilled)
            {
                cardToMove.obj.transform.position = obj.transform.position;
                cardToMove.cardPosition = Card.POSITION.INPLAY;
                if(!cardToMove.charge)
                {
                    // If the minion does not have the 'charge' trait, stop it from attacking this turn
                    cardToMove.canAttack = false;
                }
                waitForInvoke = false;
                break;
            }
        }
    }

    /// <summary>
    /// Checks if the card attacking can kill or survive and stores the outcome in a list of potential plays
    /// </summary>
    /// <param name="friendlyCard">Friendly card.</param>
    /// <param name="enemy">Enemy.</param>
    /// <param name="combinedAttack">Combined attack.</param>
    void CheckOutcome(Card friendlyCard, Card enemy, int combinedAttack)
    {
        int playPower = 0;
        // See whether the card can kill the enemy/survive attacking it, being able to do so makes the play 'stronger'
        if(enemy.attackPower >= friendlyCard.healthPoints)
        {
            friendlyCard.canSurvive = false;
        }
        else
        {
            friendlyCard.canSurvive = true;
            playPower += 1;
        }
        
        if(friendlyCard.attackPower >= enemy.healthPoints)
        {
            friendlyCard.canKill = true;
            playPower += 2;
        }
        else
        {
            friendlyCard.canKill = false;
        }
        
        // If the card cannot win and will simply just die, store a potential attack on the hero instead
        if(!friendlyCard.canSurvive && !friendlyCard.canKill)
        {
            // Store hero as a target
            int enemyHero = 0;
            if(heroA)
            {
                enemyHero = heroBHealth;
            }
            else
            {
                enemyHero = heroAHealth;
            }
            combatPotentialPlay.Add(new CombatPotentialPlays(combatCard, enemyHero, friendlyCard.attackPower, friendlyCard.attackPower, combatPlayID));
        }
        else
        {
            combatPotentialPlay.Add(new CombatPotentialPlays(combatCard, enemy, canSurvive, canKill, combinedAttack, playPower, combatPlayID));
        }
        
        // Give unique ID numbers to the plays to compare them later
        combatPlayID++;
    }

    /// <summary>
    /// Start the very first round once the cards have been generated
    /// </summary>
	public void StartRound()
	{
		if(m_cardManager.cardsGenerated)
		{
			gameActive = true;
			startRound.gameObject.SetActive(false);
		}
	}

    /// <summary>
    /// Start a new round, reset everything and update fitness values
    /// </summary>
	public void NewRound()
	{
		for(int i = 0; i < m_cardManager.cardTotal; i++)
		{
            // Get every card
			GameObject resetObj = GameObject.Find("" + (i + 1) + "(Clone)");
			if(resetObj != null)
			{
				CardInfo resetCard = resetObj.GetComponent<CardInfo>();
                // Reset the HP of the card and move it to the collection again
				resetCard.healthPoints = resetCard.healthPointsStatic;
				resetCard.hpText.text = "" + resetCard.healthPoints;
				resetCard.thisCard.cardPosition = Card.POSITION.UNUSED;
				if(!m_cardManager.cardCollection.Contains(resetCard.thisCard))
				{
					m_cardManager.cardCollection.Add (resetCard.thisCard);
				}
				resetCard.thisCard.obj.transform.position = new Vector3(20, 1, 0);
			}
		}
        // Clear the heroes decks and card lists
		deckSizeA = 0;
		deckSizeB = 0;
		deckA.Clear ();
		deckB.Clear ();
		handA.Clear ();
		handB.Clear ();
		inPlayA.Clear ();
		inPlayB.Clear ();
		graveyardA.Clear ();
		graveyardB.Clear ();
        bestDeckComposition.Clear();
		checkedBestCards = false;
        // Change how many 'best' cards will be picked next round (This adds more variance early on to give every card a chance)
		if (maxFitnessCount < 11) 
		{
			maxFitnessCount += 2;
		}
        PauseGame();

        // Get the best 15 cards as of this round
        while (bestDeckComposition.Count < 15)
        {
            float highestFitness = 0f;
            Card highestCard = null;
            // Check each card in the collection
            foreach (Card bestCard in m_cardManager.cardCollection)
            {
                // If it is 'better' than the one currently stored
                if (bestCard.fitness > highestFitness && !bestDeckComposition.Contains(bestCard))
                {
                    // Update it to reflect this
                    highestCard = bestCard;
                    highestFitness = highestCard.fitness;
                }
            }
            bestDeckComposition.Add(highestCard);
        }
        // Show the best deck on the left
        turnHistory.text = "";
        int bestCardNo = 1;
        foreach (Card bestDeckCard in bestDeckComposition)
        {
            turnHistory.text += "\n----------\n " + bestCardNo + " : " + bestDeckCard.manaCost + " mana, " + bestDeckCard.attackPower + " attack, " + bestDeckCard.healthPoints + " health ";
            if(bestDeckCard.charge)
            {
                turnHistory.text += ", with charge";
            }
            if(bestDeckCard.taunt)
            {
                turnHistory.text += ", with taunt";
            }
            bestCardNo+= 1;
        }

        turnHistory.text += "\n----------\n Turn History";

		heroA = true;
		turnState = TURNSTATE.DRAW;
        // Reset hero stats
		heroAHealth = 30;
		heroBHealth = 30;
		heroAMaxMana = 0;
		heroBMaxMana = 0;

		drawCount = 3;
		firstTurn = true;
        // Start a new game
		m_cardManager.cardsGenerated = true;
		gameActive = true;
		waitForInvoke = false;

		nextRound.gameObject.SetActive(false);
	}

    /// <summary>
    /// Pauses the game.
    /// </summary>
	public void PauseGame()
	{
        // If it is already paused, unpause it
        if(paused)
		{
            if(gameActive)
            {
    			Time.timeScale = 1;
    			pauseText.text = "Pause";
    			paused = false;
            }
		}
		else
		{
			Time.timeScale = 0;
			pauseText.text = "Unpause";
			paused = true;
		}
	}
}
