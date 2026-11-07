using System.Collections;
using System.Collections.Specialized;
using UnityEngine;

public class TransformJuice : MonoBehaviour
{
    [Header("Lunge Animation Settings")]
    [Tooltip("How far to the left (towards the player) the enemy should dash.")]
    public float dashDistanceX = 1.5f;
    public float forwardDashDuration = 0.08f;
    public float returnSnapDuration = 0.15f;

    public bool dashRight = false;

    private Vector3 baselineRestPosition;
    private Coroutine activeMotionRoutine;

    void Start()
    {
        // Record where the enemy procedurally spawned so we always know where to snap back to
        baselineRestPosition = transform.position;
    }

    // --- MAIN API INTERFACE ---
    public void TriggerLungeAnimation()
    {
        if (activeMotionRoutine != null)
        {
            StopCoroutine(activeMotionRoutine);
        }
        activeMotionRoutine = StartCoroutine(ExecuteLungeAndReturn());
    }

    private IEnumerator ExecuteLungeAndReturn()
    {
        Vector3 dashDirection = dashRight ? Vector3.right : Vector3.left;
        Vector3 peakLungeTarget = baselineRestPosition + (dashDirection * dashDistanceX);

        // DASH FORWARD (Fast)
        float elapsed = 0f;
        while (elapsed < forwardDashDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(baselineRestPosition, peakLungeTarget, elapsed / forwardDashDuration);
            yield return null;
        }
        transform.position = peakLungeTarget;

        //SNAP BACK HOME (Smooth damping)
        elapsed = 0f;
        while (elapsed < returnSnapDuration)
        {
            elapsed += Time.deltaTime;
            float smoothT = Mathf.SmoothStep(0f, 1f, elapsed / returnSnapDuration);
            transform.position = Vector3.Lerp(peakLungeTarget, baselineRestPosition, smoothT);
            yield return null;
        }
        transform.position = baselineRestPosition;
        activeMotionRoutine = null;
    }

    private void OnDisable()
    {
        // Safety insurance: Snap back to baseline positioning strings if combat abruptly ends mid-motion
        transform.position = baselineRestPosition;
    }
}
