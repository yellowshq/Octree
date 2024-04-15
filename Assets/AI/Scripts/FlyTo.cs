using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
        graph.AStar(graph.nodes[currentWP].octreeNode, destinationNode.octreeNode, pathList);
        currentWP = 0;
        pathList.Add(finalGoal);
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
            var currentNode = pathList[currentWP].octreeNode;
            var goal = currentNode.nodeBounds.center;

            Vector3 lookAtGoal = new Vector3(goal.x, goal.y, goal.z);
            Vector3 derection = lookAtGoal - this.transform.position;

            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(derection), Time.deltaTime * rotSpeed);
            this.transform.Translate(0, 0, Time.deltaTime * speed);
        }
    }
}
