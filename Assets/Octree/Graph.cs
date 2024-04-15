using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class Graph
{
    public List<Edge> edges = new List<Edge>();
    public List<Node> nodes = new List<Node>();

    public Graph() 
    { 
        
    }

    public void AddNode(OctreeNode otn)
    {
        if(FindNode(otn.id) != null)
        {
            return;
        }
        nodes.Add(new Node(otn));
    }

    public void AddEdge(OctreeNode fromNode, OctreeNode toNode)
    {
        Node from = FindNode(fromNode.id);
        Node to = FindNode(toNode.id);
        if(from != null && to != null)
        {
            Edge e = new Edge(from, to);
            edges.Add(e);
            from.edgeList.Add(e);
            Edge f = new Edge(to, from);
            edges.Add(f);
            to.edgeList.Add(f);
        }
    }

    public Node FindNode(int otn_id)
    {
        foreach (Node node in nodes)
        {
            if(node.GetNode().id == otn_id)
            { 
                return node; 
            }
        }
        return null;
    }

    public bool AStar(OctreeNode startNode, OctreeNode endNode, in List<Node> pathList)
    {
        Node start = FindNode(startNode.id);
        Node end = FindNode(endNode.id);
        pathList.Clear();

        if (start == null || endNode == null) { return false; }
        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();

        start.g = 0;
        start.h = Vector3.SqrMagnitude(startNode.nodeBounds.center - endNode.nodeBounds.center);
        start.f = start.g + start.h;

        float tentative_g_score = 0;
        float tentative_h_score = 0;

        open.Add(start);

        while(open.Count > 0)
        {
            int i = lowestF(open);
            Node currentNode = open[i];

            if (currentNode.octreeNode.id == endNode.id)
            {
                ReconstructPath(start, end, pathList);
                return true;
            }
            closed.Add(currentNode);
            open.RemoveAt(i);

            List<Node> neighborhoodNode = currentNode.GetNeighBoorHoodNodes();
            foreach (Node n_node in neighborhoodNode)
            {
                if (!closed.Contains(n_node))
                {
                    bool tentative_is_better = false;
                    tentative_g_score = currentNode.g + Vector3.SqrMagnitude(currentNode.octreeNode.nodeBounds.center - n_node.octreeNode.nodeBounds.center);
                    if (open.Contains(n_node))
                    {
                        if(tentative_g_score < n_node.g)
                        {
                            tentative_is_better = true;
                        }
                    }
                    else
                    {
                        tentative_is_better = true;
                        open.Add(n_node);
                    }

                    if(tentative_is_better)
                    {
                        n_node.comeFrom = currentNode;
                        n_node.g = tentative_g_score;
                        n_node.h = Vector3.SqrMagnitude(currentNode.octreeNode.nodeBounds.center - endNode.nodeBounds.center);
                        n_node.f = n_node.g + n_node.h;
                    }
                }
            }
        }

        return false;
    }

    public void ReconstructPath(Node start, Node end, List<Node> pathList)
    {
        pathList.Clear();
        pathList.Add(end);
        var p = end.comeFrom;
        while(p != null && p != start)
        {
            pathList.Insert(0, p);
            p = p.comeFrom;
        }
        pathList.Insert(0, start);
    }

    private int lowestF(List<Node> l)
    {
        float lowestf = 0;
        int count = 0;
        int iteratorCount = 0;
        for (int i = 0; i < l.Count; i++)
        {
            if (i == 0 || l[i].f < lowestf)
            {
                lowestf = l[i].f;
                iteratorCount = count;
            }
            count++;
        }
        return iteratorCount;
    }

    public void Draw()
    {
        foreach (var edge in edges)
        {
            Debug.DrawLine(edge.startNode.GetNode().nodeBounds.center, edge.endNode.GetNode().nodeBounds.center, Color.red);
        }
        foreach (var node in nodes)
        {
            Gizmos.color = new Color(1,1,0);
            Gizmos.DrawWireSphere(node.GetNode().nodeBounds.center, 0.25f);
        }
    }
}
