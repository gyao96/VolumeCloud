using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudBox : MonoBehaviour
{
    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public Vector3 Scale
    {
        get
        {
            Vector3 parentScale = Vector3.one;
            if (transform.parent != null && transform.parent.GetComponent<Shape>() != null)
            {
                parentScale = transform.parent.GetComponent<Shape>().Scale;
            }
            return Vector3.Scale(transform.localScale, parentScale);
        }
    }
}
