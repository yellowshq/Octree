using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OctreeObject
{
    public GameObject gameObject;
    public Bounds bounds;

    public OctreeObject(GameObject obj)
    {
        gameObject = obj;
        bounds = obj.GetComponent<Collider>().bounds;
    }
}

public class OctreeNode
{
    private static int id_number = 0;
    public int id;
    public Bounds nodeBounds;
    public Bounds[] childBounds;
    public OctreeNode[] children = null;
    public OctreeNode parent = null;
    private float minNodeSize;
    private List<OctreeObject> containObjcets = new List<OctreeObject>();

    public OctreeNode(Bounds bounds,float minNodeSize, OctreeNode parent)
    {
        id = id_number++;
        this.parent = parent;
        this.nodeBounds = bounds;
        this.minNodeSize = minNodeSize;

        float quarter = nodeBounds.size.x / 4f;
        float childLength = nodeBounds.size.x / 2f;
        Vector3 childSize = new Vector3(childLength, childLength, childLength);
        childBounds = new Bounds[8];
        childBounds[0] = new Bounds(nodeBounds.center + new Vector3(-quarter, quarter, -quarter), childSize);
        childBounds[1] = new Bounds(nodeBounds.center + new Vector3(quarter, quarter, -quarter), childSize);
        childBounds[2] = new Bounds(nodeBounds.center + new Vector3(-quarter, quarter, quarter), childSize);
        childBounds[3] = new Bounds(nodeBounds.center + new Vector3(quarter, quarter, quarter), childSize);

        childBounds[4] = new Bounds(nodeBounds.center + new Vector3(-quarter, -quarter, -quarter), childSize);
        childBounds[5] = new Bounds(nodeBounds.center + new Vector3(quarter, -quarter, -quarter), childSize);
        childBounds[6] = new Bounds(nodeBounds.center + new Vector3(-quarter, -quarter, quarter), childSize);
        childBounds[7] = new Bounds(nodeBounds.center + new Vector3(quarter, -quarter, quarter), childSize);
    }

    public void AddObject(GameObject go)
    {
        OctreeObject octreeObject = new OctreeObject(go);
        DivideAndAdd(octreeObject);
    }

    public void DivideAndAdd(OctreeObject octreeObject)
    {
        if (nodeBounds.size.x <= minNodeSize)
        {
            containObjcets.Add(octreeObject);
            return;
        }
        if (children == null)
        {
            children = new OctreeNode[8];
        }
        bool dividing = false;

        for (int i = 0; i < 8; i++)
        {
            if(children[i] == null)
            {
                children[i] = new OctreeNode(childBounds[i], minNodeSize, this);
            }
            //if (childBounds[i].Contains(octreeObject.bounds.min) && childBounds[i].Contains(octreeObject.bounds.max))
            if (childBounds[i].Intersects(octreeObject.bounds))
            {
                dividing = true;
                children[i].DivideAndAdd(octreeObject);
            }
        }
        if(!dividing)
        {
            containObjcets.Add(octreeObject);
            children = null;
        }
    }

    public bool CheckIsEmpty()
    {
        return containObjcets.Count == 0;
    }

    public void Draw()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(nodeBounds.center, nodeBounds.size);
        Gizmos.color = Color.red;
        foreach (var containObject in containObjcets)
        {
            Gizmos.DrawWireCube(containObject.bounds.center, containObject.bounds.size);
        }
        if (children != null)
        {
            foreach (var childeNode in children)
            {
                childeNode?.Draw();
            }
        }else if (containObjcets.Count != 0)
        {
            Gizmos.color = new Color(0, 0, 1, 0.25f);
            Gizmos.DrawCube(nodeBounds.center, nodeBounds.size);

        }
    }
}
