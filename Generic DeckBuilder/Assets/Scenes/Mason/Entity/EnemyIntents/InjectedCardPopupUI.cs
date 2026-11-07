using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InjectedCardPopupUI : MonoBehaviour
{
    [Header("Visual Sub-Prefab Templates")]
    public GameObject singleCardVisualPrefabTemplate;

    [Header("Physical UI Pile Screen Targets Links")]
    public RectTransform drawPileTargetButtonRect;
    public RectTransform discardPileTargetButtonRect;
    public RectTransform playerHandContainerRect;

    [Header("Scatter Layout Coordinates Tuning")]
    public float cardSpacingGapX = 160f;
    public float randomScatterJitterMax = 25f;

    [Header("Timing and Curve Animation Sliders")]
    public float centerDisplayDuration = 1.2f;
    public float flyToPileDuration = 0.6f;
    public Vector3 finalFlyScaleTarget = new Vector3(0.15f, 0.15f, 1f);

    public bool IsAnimating { get; private set; } = false;



    //TRACKER REWORK: Explicitly retain the active GameObjects to clean them on overwrite frames
    private List<GameObject> activeSpawnedCardObjects = new List<GameObject>();

    private void OnEnable()
    {
        EnemyAI.OnEnemyCardInjectedAlert += StartMultiCardScatterFlightSequence;
    }

    private void OnDisable()
    {
        EnemyAI.OnEnemyCardInjectedAlert -= StartMultiCardScatterFlightSequence;
        ClearActiveVisuals();
    }

    public void StartMultiCardScatterFlightSequence(CardData cardInjected, EnemyCardGenerationPile destination, int batchQuantity)
    {
        if (singleCardVisualPrefabTemplate == null || cardInjected == null || batchQuantity <= 0) return;

        // Forcibly clear and destroy lingering visual clones before spawning the new batch
        ClearActiveVisuals();

        IsAnimating = true;

        RectTransform finalTargetRect = drawPileTargetButtonRect;
        switch (destination)
        {
            case EnemyCardGenerationPile.DrawPile: finalTargetRect = drawPileTargetButtonRect; break;
            case EnemyCardGenerationPile.DiscardPile: finalTargetRect = discardPileTargetButtonRect; break;
            case EnemyCardGenerationPile.Hand: finalTargetRect = playerHandContainerRect; break;
        }

        float startOffsetPointX = -((batchQuantity - 1) * cardSpacingGapX) / 2f;

        for (int i = 0; i < batchQuantity; i++)
        {
            GameObject cardCloneObj = Instantiate(singleCardVisualPrefabTemplate, transform);

            // Register it inside our tracking list so we can destroy it if another enemy interrupts!
            activeSpawnedCardObjects.Add(cardCloneObj);

            RectTransform cardCloneRect = cardCloneObj.GetComponent<RectTransform>();
            if (cardCloneRect == null)
            {
                Destroy(cardCloneObj);
                continue;
            }

            PopulateVisualCardFields(cardCloneObj, cardInjected);

            float targetPosX = startOffsetPointX + (i * cardSpacingGapX);
            float targetPosY = UnityEngine.Random.Range(-randomScatterJitterMax, randomScatterJitterMax);

            Vector2 scatteredCenterCoordinates = new Vector2(targetPosX, targetPosY);

            StartCoroutine(AnimateSingleCardScatterAndFlight(cardCloneObj, cardCloneRect, scatteredCenterCoordinates, finalTargetRect));
        }
    }

    private IEnumerator AnimateSingleCardScatterAndFlight(GameObject parentObj, RectTransform cardRect, Vector2 scatteredCenterPosition, RectTransform destinationTarget)
    {
        cardRect.anchoredPosition = scatteredCenterPosition;
        cardRect.localScale = Vector3.zero;
        cardRect.gameObject.SetActive(true);

        float popTime = 0f;
        while (popTime < 0.2f)
        {
            popTime += Time.deltaTime;
            cardRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, popTime / 0.2f);
            yield return null;
        }
        cardRect.localScale = Vector3.one;

        yield return new WaitForSeconds(centerDisplayDuration);

        if (destinationTarget == null)
        {
            RemoveObjectFromTracking(parentObj);
            Destroy(parentObj);
            yield break;
        }

        Vector2 startPosition = cardRect.anchoredPosition;
        Vector2 targetScreenPosition = destinationTarget.position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.GetComponent<RectTransform>(),
            targetScreenPosition,
            null,
            out Vector2 resolvedLocalTargetPos
        );

        float elapsedTime = 0f;

        while (elapsedTime < flyToPileDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flyToPileDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            cardRect.anchoredPosition = Vector2.Lerp(startPosition, resolvedLocalTargetPos, smoothT);
            cardRect.localScale = Vector3.Lerp(Vector3.one, finalFlyScaleTarget, smoothT);

            yield return null;
        }

        // Clean out data references right as the flight path terminates
        RemoveObjectFromTracking(parentObj);
        Destroy(parentObj);

        if (activeSpawnedCardObjects.Count == 0)
        {
            IsAnimating = false;
        }

    }

    // NEW SAFETY METHOD: Safely stops operations and vaporizes every existing visual clone in the scene hierarchy
    private void ClearActiveVisuals()
    {
        StopAllCoroutines();

        IsAnimating = false;

        for (int i = activeSpawnedCardObjects.Count - 1; i >= 0; i--)
        {
            if (activeSpawnedCardObjects[i] != null)
            {
                Destroy(activeSpawnedCardObjects[i]);
            }
        }
        activeSpawnedCardObjects.Clear();
    }

    private void RemoveObjectFromTracking(GameObject targetObj)
    {
        if (activeSpawnedCardObjects.Contains(targetObj))
        {
            activeSpawnedCardObjects.Remove(targetObj);
        }
    }

    private void PopulateVisualCardFields(GameObject cardObj, CardData data)
    {
        CardController ctrl = cardObj.GetComponent<CardController>();
        if (ctrl != null)
        {
            ctrl.Initialize(data, null);
            ctrl.enabled = false;
        }
    }

}
