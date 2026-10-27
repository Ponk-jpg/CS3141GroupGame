using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class CardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Targeting Settings")]
    [Tooltip("How many pixels to drop the arros starting point to better fit")]
    public float arrowBaseYOffset = 150f;

    [Header("UI Visual References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public UnityEngine.UI.Image artworkImage;

    [Header("Hover Animation Settings")]
    public float hoverScale = 1.25f;
    public float hoverYOffset = 80f;
    public float animationSpeed = 15f;

    [Header("Play Settings")]
    [Tooltip("The Y position threshold in pixels. Moving above this triggers a play attempt.")]
    public float playYThreshold = 250f;

    private PlayerHand2D handManager;
    private TargetingSystem2D targetingSystem;
    private CardTargetType cardTargetType;
    private CardData myCardData;

    private Vector2 layoutPosition;
    private Quaternion layoutRotation;
    private int originalSiblingIndex;
    private bool isHovered = false;
    private bool isDragging = false;

    private RectTransform rectTransform;
    private Canvas mainCanvas;
    private bool isTargetingMode = false;


    public CardData GetCardData()
    {
        return myCardData;
    }


    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Automatically find the parent UI canvas for accurate drag math
        mainCanvas = GetComponentInParent<Canvas>();
        targetingSystem = GetComponentInParent<TargetingSystem2D>();
    }

    public void Initialize(CardData data, PlayerHand2D manager)
    {
        handManager = manager;
        myCardData = data;

        nameText.text = data.Name;
        descriptionText.text = data.Description;
        costText.text = data.energyCost.ToString();
        artworkImage.sprite = data.artwork;
        cardTargetType = data.targetType;
    }

    public void SetLayoutTarget(Vector2 position, Quaternion rotation, int siblingIndex)
    {
        layoutPosition = position;
        layoutRotation = rotation;
        originalSiblingIndex = siblingIndex;

        if (!isHovered && !isDragging)
        {
            rectTransform.SetSiblingIndex(siblingIndex);
        }
    }

    void Update()
    {
        // 1. STATE: Actively dragging an UNTARGETED card (or a targeted card below the line)
        if (isDragging && !isTargetingMode)
        {
            // Do nothing here—the OnDrag function is directly overriding position to match the mouse cursor!
            return;
        }

        // 2. STATE: Actively targeting an enemy (Card sits frozen in the Preview Slot)
        if (isTargetingMode)
        {
            Vector2 previewPosition = new Vector2(0f, 100f); // Centers the card right above the hand
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, previewPosition, Time.deltaTime * animationSpeed);
            rectTransform.localRotation = Quaternion.Slerp(rectTransform.localRotation, Quaternion.identity, Time.deltaTime * animationSpeed);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one * hoverScale, Time.deltaTime * animationSpeed);
        }
        // 3. STATE: Mouse is hovering over the card normally
        else if (isHovered)
        {
            Vector2 targetPos = new Vector2(layoutPosition.x, layoutPosition.y + hoverYOffset);
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPos, Time.deltaTime * animationSpeed);
            rectTransform.localRotation = Quaternion.Slerp(rectTransform.localRotation, Quaternion.identity, Time.deltaTime * animationSpeed);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one * hoverScale, Time.deltaTime * animationSpeed);
        }
        // 4. STATE: Resting passively inside the hand layout arc
        else
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, layoutPosition, Time.deltaTime * animationSpeed);
            rectTransform.localRotation = Quaternion.Slerp(rectTransform.localRotation, layoutRotation, Time.deltaTime * animationSpeed);
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one, Time.deltaTime * animationSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDragging) return;
        isHovered = true;
        rectTransform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isDragging) return;
        isHovered = false;
        ResetHierarchyDepth();
        handManager.RefreshSiblingOrders();
    }

    public void ResetHierarchyDepth()
    {
        if (!isHovered && !isDragging)
        {
            rectTransform.SetSiblingIndex(originalSiblingIndex);
        }
    }

    // --- DRAG INTERFACE IMPLEMENTATIONS ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        isHovered = false;
        rectTransform.SetAsLastSibling(); // Render on top of all other cards
        rectTransform.localRotation = Quaternion.identity; // Flatten card out
        rectTransform.localScale = Vector3.one * hoverScale; // Keep scaled up

        // Force an immediate snap to the mouse position on frame one
        MoveCardToMouse(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // TARGETED CARD BEHAVIOR: Check thresholds based on MOUSE position, not card position
        if (cardTargetType == CardTargetType.Targeted)
        {
            // Convert the screen mouse coordinate accurately into the local coordinate system of the hand anchor
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                handManager.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localMousePoint
            );

            // Check if the MOUSE is above the line, completely independent of where the card slides
            if (localMousePoint.y >= playYThreshold && !isTargetingMode)
            {
                isTargetingMode = true;

                if (targetingSystem != null)
                {
                    Vector2 lowerStartPoint = new Vector2(layoutPosition.x, layoutPosition.y - arrowBaseYOffset);
                    targetingSystem.StartTargeting(lowerStartPoint);
                }
            }
            else if (localMousePoint.y < playYThreshold && isTargetingMode)
            {
                isTargetingMode = false;
                if (targetingSystem != null) targetingSystem.StopTargeting();
            }
        }

        // Keep updating positions normally if we are still holding the card below the line
        if (!isTargetingMode)
        {
            MoveCardToMouse(eventData);
        }
    }

    private void MoveCardToMouse(PointerEventData eventData)
    {
        // Safely translates screen pixel mouse coordinates into the correct 3D Camera view plane
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint))
        {
            rectTransform.position = worldPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        isHovered = false;
        GetComponent<CanvasGroup>().alpha = 1f; // Force reset visibility flag

        if (targetingSystem != null) targetingSystem.StopTargeting();

        // SCENARIO A: Targeted Play Processing
        if (cardTargetType == CardTargetType.Targeted)
        {
            if (isTargetingMode)
            {
                isTargetingMode = false;

                // Attempt to find what enemy the pointer cursor dropped onto
                CombatEnemy enemy = GetEnemyUnderMouse(eventData);

                if (enemy != null)
                {
                    PlayCardWithTarget(enemy);
                }
                else
                {
                    // No enemy found, drop card backward into hand
                    handManager.RefreshSiblingOrders();
                }
            }
            else
            {
                handManager.RefreshSiblingOrders();
            }
        }
        // SCENARIO B: Untargeted Play Processing
        else
        {
            if (rectTransform.anchoredPosition.y >= playYThreshold)
            {
                PlayCardNoTarget();
            }
            else
            {
                handManager.RefreshSiblingOrders();
            }
        }
    }

    // --- RAYCAST ENEMY RESOLUTION MATH ---
    private CombatEnemy GetEnemyUnderMouse(PointerEventData eventData)
    {
        // 1. Check UI Canvas elements first (if your enemy is a UI Button or Panel)
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            CombatEnemy enemyComponent = result.gameObject.GetComponentInParent<CombatEnemy>();
            if (enemyComponent != null) return enemyComponent;
        }

        // 2. Check 2D Game World elements (if your enemy uses a SpriteRenderer + BoxCollider2D)
        Vector2 mouseWorldPos = mainCanvas.worldCamera.ScreenToWorldPoint(eventData.position);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            return hit.collider.GetComponentInParent<CombatEnemy>();
        }

        return null; // Empty area dropped
    }

    // --- CARD RESOLUTION ACTIVATIONS ---

    private void PlayCardWithTarget(CombatEnemy target)
    {
        UnityEngine.Debug.Log($"[PLAY] Active targeted spell: {nameText.text} successfully triggered against target: {target.enemyName}!");

        // This gives you the direct script hook access to execute damage or statuses!
        target.TakeDamage(6);

        FinalizeCardPlay();
    }

    private void PlayCardNoTarget()
    {
        UnityEngine.Debug.Log($"[PLAY] Active area effect/buff: {nameText.text} played globally!");

        // Handle standalone player state shifts here (e.g., gain Armor)

        FinalizeCardPlay();
    }

    private void FinalizeCardPlay()
    {
        DeckManager deckEngine = handManager.GetComponent<DeckManager>();
        if (deckEngine != null)
        {

            deckEngine.DiscardCard(myCardData);
        }

        handManager.RemoveCardFromHand(this);
        Destroy(gameObject);
    }


    private void PlayCard()
    {
        UnityEngine.Debug.Log($"Successfully Played: {nameText.text}!");

        // Remove this card from the layout management array
        handManager.RemoveCardFromHand(this);

        // Destroy the card UI object
        Destroy(gameObject);
    }



}
