using System.Collections.Generic;
using UnityEngine;

// Extra implementation for the actual spawning and setup of the cards in the hand of the player during combat.
public class PlayerHand2D : MonoBehaviour
{

    [Header("References")]
    public GameObject cardPrefab;

    [Header("2D Arc Layout Configuration")]
    public float maxCardSpacing = 140f;
    public float arcHeight = 45f;
    public float maxRotationAngle = 14f;
    public float maxHandWidth = 750f;

    private List<CardController> activeCards = new List<CardController>();

    public List<CardController> GetCardsInHand()
    {
        return activeCards;
    }


    public void AddCardToHand(CardData data)
    {
        GameObject spawnedCard = Instantiate(cardPrefab, transform);
        CardController controller = spawnedCard.GetComponent<CardController>();

        if (controller != null)
        {
            activeCards.Add(controller);
            controller.Initialize(data, this);
            ArrangeHand();
        }
    }

    public void ArrangeHand()
    {
        int cardCount = activeCards.Count;
        if (cardCount == 0) return;

        // Calculate condensed spacing dynamically if the user holds too many cards
        float totalWidth = (cardCount - 1) * maxCardSpacing;
        float currentSpacing = maxCardSpacing;

        if (totalWidth > maxHandWidth)
        {
            currentSpacing = maxHandWidth / (cardCount - 1);
        }

        for (int i = 0; i < cardCount; i++)
        {
            // Normalize layout index evaluation between -0.5 and 0.5 to keep hand centered
            float t = cardCount > 1 ? (float)i / (cardCount - 1) : 0.5f;
            float normalizedHorizontal = t - 0.5f;

            //Calculate X Position
            float xPos = normalizedHorizontal * (cardCount - 1) * currentSpacing;

            //Calculate Y Position using a parabolic curve offset
            float yPos = (1f - 4f * normalizedHorizontal * normalizedHorizontal) * arcHeight;

            //Calculate Z Rotation (Tilt)
            float zRot = -normalizedHorizontal * maxRotationAngle;

            // Pass targets downstream to individual card smooth lerp loop engines
            Vector2 targetPos = new Vector2(xPos, yPos);
            Quaternion targetRot = Quaternion.Euler(0, 0, zRot);

            activeCards[i].SetLayoutTarget(targetPos, targetRot, i);
        }
    }

    // Called when a card is unhovered to reset structural layered occlusion back to default
    public void RefreshSiblingOrders()
    {
        for (int i = 0; i < activeCards.Count; i++)
        {
            activeCards[i].ResetHierarchyDepth();
        }
    }

    public void RemoveCardFromHand(CardController card)
    {
        if (activeCards.Contains(card))
        {
            activeCards.Remove(card);
            ArrangeHand(); // reorganize cards 

        }


    }

}
