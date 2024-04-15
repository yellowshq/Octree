using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class FlyTo : MonoBehaviour
{
    public float speed = 20.0f;
    public float accuracy = 1.0f;
    public float rotSpeed = 15.0f;

    private int currentWP = 0;

    public Transform goalTrans;
    public GameObject octreeGo;

    private Octree octree;
    private Graph graph;

    private OctreeNode currentNode;

    private List<Node> pathList = new List<Node>();
    void Start()
    {
        var co = octreeGo.GetComponent<CreateOctree>();
        graph = co.navGraph;
        octree = co.octree;

    }

    void NavigateTo(int destination, Node finalGoal)
    {
        Node destinationNode = graph.FindNode(destination);
        int start_i = octree.FindBindingNode(transform.position);
        Node startNode = graph.FindNode(start_i);

        graph.AStar(startNode.octreeNode, destinationNode.octreeNode, pathList);
        currentWP = 0;
        pathList.Add(finalGoal);

        foreach (var path in pathList)
        {
            Debug.Log(path.octreeNode.nodeBounds.center);
        }
        Debug.Log("=======================");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < pathList.Count - 1; i++)
        {
            var path = pathList[i];
            var path_to = pathList[i+1];

            Gizmos.DrawLine(path.octreeNode.nodeBounds.center, path_to.octreeNode.nodeBounds.center);
        }
    }

    void Update()
    {
        if(octree == null) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 pos = goalTrans.position;
            int i = octree.FindBindingNode(pos);
            if(i == -1)
            {
                Debug.Log("Destination not found in Octree");
                return;
            }
            if (Vector3.Distance(pos, this.transform.position) <= accuracy)
            {
                Debug.Log("Destination is run to target pos");
                return;
            }
            Node finalGoal = new Node(new OctreeNode(new Bounds(pos, new Vector3(1, 1, 1)), 1, null));
            NavigateTo(i, finalGoal);
        }

    }

    void LateUpdate()
    {
        if (graph == null) return;
        int length = pathList.Count;
        if (length == 0 || currentWP == length)
        {
            return;
        }

        if (Vector3.Distance(pathList[currentWP].octreeNode.nodeBounds.center, this.transform.position) <= accuracy)
        {
            currentWP++;
        }

        if (currentWP < length)
        {
            currentNode = pathList[currentWP].octreeNode;
            var goal = currentNode.nodeBounds.center;

            Vector3 lookAtGoal = new Vector3(goal.x, goal.y, goal.z);
            Vector3 derection = lookAtGoal - this.transform.position;

            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(derection), Time.deltaTime * rotSpeed);
            this.transform.Translate(0, 0, Time.deltaTime * speed);
        }
    }
}
