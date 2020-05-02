using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeContainer : MonoBehaviour
{
    public Color color = Color.blue;
    public bool drawOutline = true;
    void OnDrawGizmosSelected()
    {
        Debug.Log("Draw");
        if (drawOutline)
        {
            Gizmos.color = color;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}
