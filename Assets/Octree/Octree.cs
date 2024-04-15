using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class Octree
{
    public OctreeNode rootNode;
    public List<OctreeNode> emptyLeaves = new List<OctreeNode>();
    public Graph navgationGraph;
    public Octree(GameObject[] gameObjects, float minNodeSize, Graph navGraph)
    {
        navgationGraph = navGraph;
        Bounds bounds = new Bounds();
        foreach (var go in gameObjects)
        {
            bounds.Encapsulate(go.GetComponent<Collider>().bounds);
        }

        //设置为等边立方体
        float maxSize = Mathf.Max(new float[] { bounds.size.x, bounds.size.y, bounds.size.z });
        Vector3 sizeVector = new Vector3(maxSize, maxSize, maxSize) * 1f;
        bounds.SetMinMax(bounds.center - sizeVector, bounds.center + sizeVector);

        rootNode = new OctreeNode(bounds, minNodeSize, null);
        AddObjects(gameObjects);

        GetEmptyLeaves(rootNode);
        ConnectLeafNodeNeighbours();
        //ProcessExtraConnections();
        //Debug.Log($"navgationGraph edge count {navgationGraph.edges.Count}");
    }

    public void AddObjects(GameObject[] gameObjects)
    {
        foreach (var go in gameObjects)
        {
            if (go.activeSelf)
            {
                rootNode.AddObject(go);
            }
        }
    }

    public int FindBindingNode(Vector3 position)
    {
        return FindBindingNode(rootNode, position);
    }

    public int FindBindingNode(OctreeNode node, Vector3 position)
    {
        int found = -1;
        if(node == null) return found;
        if(node.children == null)
        {
            if(node.CheckIsEmpty() && node.nodeBounds.Contains(position))
            {
                return node.id;
            }
        }
        else
        {
            foreach (var child in node.children)
            {
                found = FindBindingNode(child, position);
                if (found != -1)break;
            }
        }
        return found;
    }

    public void GetEmptyLeaves(OctreeNode node)
    {
        if(node == null)
        {
            return;
        }
        if (node.children == null)
        {
            if (node.CheckIsEmpty())
            {
                emptyLeaves.Add(node);
                navgationGraph.AddNode(node);
            }
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                GetEmptyLeaves(node.children[i]);

                //for (int s = 0; s < 8; s++)
                //{
                //    if(s != i)
                //    {
                //        navgationGraph.AddEdge(node.children[i], node.children[s]);
                //    }
                //}
            }
        }
    }

    private void ProcessExtraConnections()
    {
        int eCount = 0;
        int length = emptyLeaves.Count;
        Dictionary<int, int> subGraphConnections = new Dictionary<int, int>();
        for (int i = 0; i < length; i++)
        {
            var node_i = emptyLeaves[i];
            for (int j = 0; j < length; j++)
            {
                var node_j = emptyLeaves[j];
                if(node_i.parent.id != node_j.parent.id)
                {
                    RaycastHit raycastHit;
                    Vector3 direction = node_j.nodeBounds.center - node_i.nodeBounds.center;
                    float accuracy = 1f;
                    if (!subGraphConnections.ContainsKey(node_i.parent.id) && !Physics.SphereCast(node_i.nodeBounds.center, accuracy, direction, out raycastHit))
                    {
                        if (subGraphConnections.TryAdd(node_i.parent.id, node_j.parent.id))
                        {
                            navgationGraph.AddEdge(node_i, node_j);
                            eCount = eCount + 1;
                        }
                    }
                }
            }
        }
    }

    private void ConnectLeafNodeNeighbours()
    {
        List<Vector3> rays = new List<Vector3>()
        {
            new Vector3(1,0,0),
            new Vector3(-1,0,0),
            new Vector3(0,1,0),
            new Vector3(0,-1,0),
            new Vector3(0,0,1),
            new Vector3(0,0,-1)
        };
        int length = emptyLeaves.Count;
        int rayCount = rays.Count;
        List<OctreeNode> neighbours = new List<OctreeNode>();
        for (int i = 0; i < length; i++)
        {
            var currentNode = emptyLeaves[i];
            neighbours.Clear();
            for (int j = 0; j < length; j++)
            {
                for (int r = 0; r < rayCount; r++)
                {
                    Ray ray = new Ray(currentNode.nodeBounds.center, rays[r]);
                    float maxLength = currentNode.nodeBounds.size.y / 2.0f + 0.01f;
                    float hitLength;
                    if (emptyLeaves[j].nodeBounds.IntersectRay(ray, out hitLength))
                    {
                        if(hitLength < maxLength)
                        {
                            neighbours.Add(emptyLeaves[j]);
                        }
                    }
                }
            }
            foreach (var nb in neighbours)
            {
                navgationGraph.AddEdge(currentNode, nb);
            }
        }
    }
}
