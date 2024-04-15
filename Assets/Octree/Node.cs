using System;
using System.Collections.Generic;

public class Node
{
    public List<Edge> edgeList = new List<Edge>();
    public Node path = null;
    public OctreeNode octreeNode;

    public Node comeFrom;
    public float f, g, h;
    public Node(OctreeNode octreeNode)
    {
        this.octreeNode = octreeNode;
        path = null;
    }

    public OctreeNode GetNode()
    {
        return octreeNode;
    }

    public List<Node> GetNeighBoorHoodNodes()
    {
        List<Node> nodes = new List<Node>();
        foreach (var edge in edgeList)
        {
            nodes.Add(edge.endNode);
        }
        return nodes;
    }
}
