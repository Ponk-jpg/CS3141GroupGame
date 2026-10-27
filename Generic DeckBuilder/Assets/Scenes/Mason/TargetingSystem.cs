using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetingSystem2D : MonoBehaviour
{
    [Header("References")]
    public GameObject dotPrefab;
    [Tooltip("How many points/dots make up the arrow chain.")]
    public int dotCount = 15;

    [Header("Curve Tuning")]
    [Tooltip("How much the curve bows outward horizontally.")]
    public float curveOffsetPower = 150f;

    private List<RectTransform> spawnedDots = new List<RectTransform>();
    private Canvas mainCanvas;
    private bool isActive = false;
    private Vector2 startPoint;

    void Awake()
    {
        mainCanvas = GetComponent<Canvas>();

        // Pre-spawn the pooling dots so we don't lag the engine while dragging
        for (int i = 0; i < dotCount; i++)
        {
            GameObject dot = Instantiate(dotPrefab, transform);
            RectTransform rt = dot.GetComponent<RectTransform>();
            dot.SetActive(false);
            spawnedDots.Add(rt);
        }
    }

    public void StartTargeting(Vector2 handPosition)
    {
        isActive = true;
        startPoint = handPosition;

        foreach (var dot in spawnedDots)
        {
            dot.gameObject.SetActive(true);
        }
    }

    public void StopTargeting()
    {
        isActive = false;
        foreach (var dot in spawnedDots)
        {
            dot.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isActive) return;

        Vector2 mouseLocalPoint;

        Vector2 currentMousePos = Mouse.current.position.ReadValue();

        // Convert the current screen mouse position accurately into UI Canvas space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            currentMousePos,
            mainCanvas.worldCamera,
            out mouseLocalPoint
        );

        // Calculate a control point to generate a Slay the Spire dynamic curve arc
        Vector2 controlPoint = Vector2.Lerp(startPoint, mouseLocalPoint, 0.5f);
        controlPoint.y += curveOffsetPower; // Push the curve upward in the middle
        controlPoint.x -= (mouseLocalPoint.x - startPoint.x) * 0.2f; // Slight horizontal bow

        for (int i = 0; i < dotCount; i++)
        {
            float t = (float)i / (dotCount - 1);

            // Quadratic Bezier Curve formula: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
            Vector2 dotPos = Mathf.Pow(1 - t, 2) * startPoint +
                             2 * (1 - t) * t * controlPoint +
                             Mathf.Pow(t, 2) * mouseLocalPoint;

            spawnedDots[i].anchoredPosition = dotPos;

            //Make the dots progressively scale up larger near the tip arrow cursor
            spawnedDots[i].localScale = Vector3.one * Mathf.Lerp(0.5f, 1.3f, t);

            // Rotate the dots to face the next dot in the chain path
            if (i > 0)
            {
                Vector2 direction = dotPos - spawnedDots[i - 1].anchoredPosition;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                spawnedDots[i].localRotation = Quaternion.Euler(0, 0, angle - 90f);
            }
        }
    }
}