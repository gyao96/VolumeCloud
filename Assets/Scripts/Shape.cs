using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{

    public enum ShapeType { Sphere, Cube, Torus, RCube };
    public enum Operation { None, Blend, Cut, Mask }

    public ShapeType shapeType;
    public Operation operation;
    public Color colour = Color.white;
    public float radius;
    [Range(0, 1)]
    public float blendStrength;
    public float movespeed = 1f;
    public Vector3 movedir;
    public float moveradius = 0.5f;
    [HideInInspector]
    public int numChildren;

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

    private Vector3 startpos;
    private Vector3 goalpos;

    public void Start()
    {
        startpos = transform.position;
        goalpos = transform.position + movedir * moveradius;
    }
    public void FixedUpdate()
    {
        if ((transform.position - goalpos).magnitude < 0.01f)
        {
            movedir = -movedir;
            goalpos = startpos;
            startpos = transform.position;
        }
        Vector3 delta = movedir * movespeed * Time.deltaTime;
        transform.position += delta;
    }
}
