using UnityEngine;


public class FloorEntity : MonoBehaviour
{

    [Tooltip("How wid this specific entities shadow distortion should be")]
    public float shadowRadius = 0.2f;

    void OnEnable()
    {
        if (!FloorShadowMaster.ActiveEntities.Contains(this))
        {

            FloorShadowMaster.ActiveEntities.Add(this);
        }
       

    }

    void OnDisable()
    {
        if (FloorShadowMaster.ActiveEntities.Contains(this))
        {

            FloorShadowMaster.ActiveEntities.Remove(this);
        }


    }


}
