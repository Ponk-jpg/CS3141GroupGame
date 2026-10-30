using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

// Enemy intent UI, used to display what an enemy plans to do for a given turn.
// doesnt actually calculate what it plans to do, it just updates and displays it on the screen.
public class EnemyIntentUI : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Create a small sub-prefab containing an Image component and a child TextMeshPro component")]
    public GameObject singleIntentIconPrefab;

    private List<GameObject> activeSpawnedIcons = new List<GameObject>();
    private List<EnemyMoveData> trackingMoves = new List<EnemyMoveData>();
    private CombatEnemy parentEnemy;



    void Awake()
    {

        parentEnemy = GetComponentInParent<CombatEnemy>();

    }

    private void OnEnable()
    {
        StatusController.OnAnyStatusChanged += RefreshIntentValues;
    }

    private void OnDisable()
    {

        StatusController.OnAnyStatusChanged -= RefreshIntentValues;

    }

    // refreshes the values displayed in the UI based on status effects and buffs.
    private void RefreshIntentValues()
    {

        if (parentEnemy == null)
        {
            parentEnemy = GetComponentInParent<CombatEnemy>();
        }

        ClearIcons();

        if (trackingMoves == null || trackingMoves.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        PlayerStats playerTarget = UnityEngine.Object.FindAnyObjectByType<PlayerStats>();

        foreach (EnemyMoveData move in trackingMoves)
        {
            if (move == null) continue;

            GameObject newIconObj = Instantiate(singleIntentIconPrefab, transform);
            activeSpawnedIcons.Add(newIconObj);

            RectTransform iconRect = newIconObj.GetComponent<RectTransform>();
            if (iconRect != null)
            {
                iconRect.localPosition = Vector3.zero;
                iconRect.localScale = Vector3.one;
                iconRect.localRotation = Quaternion.identity;
            }

            UnityEngine.UI.Image img = newIconObj.GetComponentInChildren<UnityEngine.UI.Image>();
            TextMeshProUGUI txt = newIconObj.GetComponentInChildren<TextMeshProUGUI>();

            if (img != null) img.sprite = move.intentIcon;

            if (txt != null)
            {
                if (move.intentType == IntentType.Attack)
                {
                    // Calculate incoming modified damage based on Weak/Strength/Vulnerable status states!
                    int modDmg = DamageCalculator.CalculateModifiedDamage(move.baseValue, parentEnemy, playerTarget);

                    txt.text = move.hitCount > 1 ? $"{modDmg}x{move.hitCount}" : modDmg.ToString();

                    // Color shift feedback tracking
                    if (modDmg > move.baseValue) txt.color = Color.green;
                    else if (modDmg < move.baseValue) txt.color = Color.red;
                    else txt.color = Color.white;
                }
                else if (move.intentType == IntentType.Defend)
                {
                    // Calculate enemy block based on Frazier/Frail status metrics
                    int modBlk = DamageCalculator.CalculateModifiedBlock(move.baseValue, parentEnemy);
                    txt.text = modBlk.ToString();

                    if (modBlk > move.baseValue) txt.color = Color.green;
                    else if (modBlk < move.baseValue) txt.color = Color.red;
                    else txt.color = Color.cyan;
                }
                else
                {
                    txt.text = "";
                }
            }
        }

        RectTransform myRect = GetComponent<RectTransform>();
        if (myRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(myRect);


    }

    // used to update to display multiple intents if we are using multiple.
    public void DisplayMultipleIntents(List<EnemyMoveData> plannedMoves)
    {

        trackingMoves = new List<EnemyMoveData>(plannedMoves);
        RefreshIntentValues();
    }

    public void HideIntent()
    {
        trackingMoves.Clear();
        ClearIcons();
        gameObject.SetActive(false);
    }

    private void ClearIcons()
    {
        foreach (GameObject icon in activeSpawnedIcons)
        {
            if (icon != null) Destroy(icon);
        }
        activeSpawnedIcons.Clear();
    }
}
