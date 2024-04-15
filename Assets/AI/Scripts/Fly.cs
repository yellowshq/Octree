using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : MonoBehaviour
{
    public float speed = 5.0f;
    public float accuracy = 1.0f;
    public float rotSpeed = 5.0f;

    private int currentWP = 1;
    private Vector3 goal;
    private OctreeNode currentNode;

    public GameObject octree;
    private Graph graph;

    private List<Node> pathList = new List<Node>();

    public bool canFly = true;

    void Start()
    {
        graph = octree.GetComponent<CreateOctree>().navGraph;
        if (canFly)
        {
            Navigate();
        }
    }

    void Navigate()
    {
        currentNode = graph.nodes[currentWP].octreeNode;
        GetRandomDestination();
    }

    void GetRandomDestination()
    {
        int randnode = Random.Range(0, graph.nodes.Count);
        graph.AStar(currentNode, graph.nodes[randnode].octreeNode, pathList);
        currentWP = 0;
        if (pathList.Count == 0)
        {
            Debug.Log("No Path");
        }
    }

    private void LateUpdate()
    {
        if(graph == null) return;
        if (!canFly) return;
        int length = pathList.Count;
        if (length == 0|| currentWP == length)
        {
            GetRandomDestination();
            return;
        }
        if (Vector3.Distance(pathList[currentWP].octreeNode.nodeBounds.center, this.transform.position) <= accuracy)
        {
            currentWP++;
        }

        if(currentWP < length)
        {
            currentNode = pathList[currentWP].octreeNode;
            goal = currentNode.nodeBounds.center;

            Vector3 lookAtGoal = new Vector3(goal.x, goal.y, goal.z);
            Vector3 derection = lookAtGoal - this.transform.position;

            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(derection), Time.deltaTime * rotSpeed);
            this.transform.Translate(0, 0, Time.deltaTime * speed);
        }
        else
        {
            GetRandomDestination();
        }
    }
}
