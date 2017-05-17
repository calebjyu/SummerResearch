using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour {

    NavMeshAgent agent;
    bool colliding = false;

	// Use this for initialization
	void Start () {
        agent = GetComponent<NavMeshAgent>();
	}
	
	// Update is called once per frame
	void Update () {
        //colliding = false;
	}
    public void go(Vector3 v3)
    {
        agent.SetDestination(v3);
    }
    void OnTriggerEnter(Collider other)
    {
        //if (colliding) return;
        //Debug.Log("collide");
        if (other.gameObject.tag.Equals("AI"))
        {
           // colliding = true;
            agent.isStopped = true;
            agent.ResetPath();
            agent.isStopped = false;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("AI"))
        {
            agent.isStopped = false;
        }
    }
}
