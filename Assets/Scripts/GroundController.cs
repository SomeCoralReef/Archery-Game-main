using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundController : MonoBehaviour
{
    private List<Transform> attachedArrows = new List<Transform>();

    public void AttachArrow(Transform arrowTransform)
    {
        attachedArrows.Add(arrowTransform);
    }

    void Update()
    {
        foreach (Transform arrow in attachedArrows)
        {
            // Update arrow position if necessary or apply transformations
            // This part needs to be implemented based on specific requirements
        }
    }
}