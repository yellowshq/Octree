using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateOctree : MonoBehaviour
{
    public GameObject[] worldObjects;
    public float minNodeSize = 5f;
    public Octree octree;
    public Graph navGraph;
    void Awake()
    {
        navGraph = new Graph();
        octree = new Octree(worldObjects, minNodeSize, navGraph);
    }


    private void OnDrawGizmos()
    {
        if(Application.isPlaying)
        {
            octree.rootNode.Draw();
            navGraph.Draw();
        }
    }
}
