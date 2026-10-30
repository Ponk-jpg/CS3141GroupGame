using System.Collections.Generic;
using UnityEngine;


// UI manager for status effects on an entity.
public class StatusDisplayRowUI : MonoBehaviour
{
    [Header("References")]
    public StatusController targetStatusController;

    [Header("Prefabs")]
    [Tooltip("Drop a small UI prefab containing an Image and TMPro text component here")]
    public GameObject statusIconPrefab;

    private List<GameObject> spawnedIcons = new List<GameObject>();

    private void OnEnable()
    {
        if (targetStatusController != null)
        {
            targetStatusController.OnStatusesChanged += RefreshStatusIconsDisplay;
        }
    }

    private void OnDisable()
    {
        if (targetStatusController != null)
        {
            targetStatusController.OnStatusesChanged -= RefreshStatusIconsDisplay;
        }
    }

    // Call this if swapping targets manually at runtime
    public void SetNewTarget(StatusController newTarget)
    {
        if (targetStatusController != null) targetStatusController.OnStatusesChanged -= RefreshStatusIconsDisplay;
        targetStatusController = newTarget;
        if (targetStatusController != null)
        {
            targetStatusController.OnStatusesChanged += RefreshStatusIconsDisplay;
            RefreshStatusIconsDisplay(targetStatusController.GetActiveStatusesList());
        }
    }


    private void RefreshStatusIconsDisplay(List<StatusController.ActiveStatus> currentStatuses)
    {
        // Clear out old UI icons structures
        foreach (GameObject icon in spawnedIcons) if (icon != null) Destroy(icon);
        spawnedIcons.Clear();

        foreach (var status in currentStatuses)
        {
            if (status.stacks <= 0 && status.data.hideAtZeroStacks) continue;

            GameObject newIcon = Instantiate(statusIconPrefab, transform);

            // Fix Scale drift anomalies across dynamic canvases layouts
            // NOTE: i dont think this fixed it.
            newIcon.transform.localScale = Vector3.one;
            newIcon.transform.localPosition = Vector3.zero;

            StatusIconUI iconScript = newIcon.GetComponent<StatusIconUI>();
            if (iconScript != null)
            {
                iconScript.SetupIcon(status);
            }

            spawnedIcons.Add(newIcon);
        }
    }



}
