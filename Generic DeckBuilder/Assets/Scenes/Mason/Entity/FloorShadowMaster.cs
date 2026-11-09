using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(UnityEngine.UI.Image))]
public class FloorShadowMaster : MonoBehaviour
{
    // The master active list of entities
    public static List<FloorEntity> ActiveEntities { get; private set; } = new List<FloorEntity>();

    private UnityEngine.UI.Image backgroundImage;
    private Material floorMaterial;
    private RectTransform backgroundRect;

    private int shadowPositionsID;
    private int shadowRadiiID;
    private int entityCountID;

    private const int MAX_ENTITIES = 8;
    private Vector4[] shadowPositionsArray = new Vector4[MAX_ENTITIES];
    private float[] shadowRadiiArray = new float[MAX_ENTITIES];

    void Start()
    {
        backgroundImage = GetComponent<UnityEngine.UI.Image>();
        floorMaterial = backgroundImage.materialForRendering;
        backgroundRect = GetComponent<RectTransform>();

        shadowPositionsID = Shader.PropertyToID("_ShadowPositions");
        shadowRadiiID = Shader.PropertyToID("_ShadowRadii");
        entityCountID = Shader.PropertyToID("_EntityCount");
    }

    void Update()
    {
        if (floorMaterial == null) return;

        // Clean lookup: Math is capped by MAX_ENTITIES or the exact list count
        int count = Mathf.Min(ActiveEntities.Count, MAX_ENTITIES);

        for (int i = 0; i < count; i++)
        {
            if (ActiveEntities[i] == null) continue;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                backgroundRect,
                ActiveEntities[i].transform.position,
                null,
                out localPoint
            );

            float uvX = (localPoint.x / backgroundRect.rect.width) + 0.5f;
            float uvY = (localPoint.y / backgroundRect.rect.height) + 0.5f;

            shadowPositionsArray[i] = new Vector4(uvX, uvY, 0, 0);
            shadowRadiiArray[i] = ActiveEntities[i].shadowRadius;
        }

        // Send the tracked calculations to the shader
        floorMaterial.SetVectorArray(shadowPositionsID, shadowPositionsArray);
        floorMaterial.SetFloatArray(shadowRadiiID, shadowRadiiArray);
        floorMaterial.SetInt(entityCountID, count);
    }
}
