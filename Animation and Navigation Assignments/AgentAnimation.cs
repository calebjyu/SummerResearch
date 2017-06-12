using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentAnimation : MonoBehaviour {

    GameObject robot;
    NavMeshAgent agent;
    Animator animator;
    private List<Vector3> path;
    private bool runningCoroutine;
    private float distance;
    private int x = 0;

    // Use this for initialization
    void Start () {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        robot = this.gameObject;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.LeftShift) && !agent.isOnOffMeshLink)
        {
            animator.SetFloat("y", 1.0f, 1f, Time.deltaTime * 10);
            //agent.speed = 7.0f;
        }
        else
        {
            animator.SetFloat("y", 0.0f, 1f, Time.deltaTime * 10);
            //agent.speed = 3.5f;
        }
        /*if (agent.remainingDistance<2f&&animator.GetBool("isMoving"))
        {
            animator.SetBool("isMoving", false);
        }*/
        if (path!=null && !runningCoroutine)
        {
            runningCoroutine = true;
            StartCoroutine(walkOnPath());
        }
        if (runningCoroutine) //update distance
        {
            distance = Vector3.Distance(path[x], robot.transform.position);
            Debug.Log("distance="+distance);
        }
        /*if (agent.hasPath)
        {
            animator.SetBool("isMoving", true);
        }

        if (agent.isOnOffMeshLink)
        {
            //agent.speed = 3.5f;
            animator.SetBool("isJumping",true);
            StartCoroutine(jump());
        }*/
	}
    IEnumerator walkOnPath()
    {
        Debug.Log("hi");
        for (int i=0;i<path.Count;i++)
        {
            Vector3 targetDir = path[i] - robot.transform.position;
            float step = 10 * Time.deltaTime;
            Vector3 newDir = Vector3.RotateTowards(transform.right, targetDir, step, 0.0f);
            transform.rotation = Quaternion.LookRotation(targetDir);
            x = i;
            Debug.Log(Vector3.Distance(path[i], robot.transform.position));
            yield return new WaitUntil(() => distance<2f);
            if (i==path.Count-1)
            {
                animator.SetBool("isMoving", false);
            }
        }
        x = 0;
        path = null;
        runningCoroutine = false;
    }
    IEnumerator jump()
    {
        yield return new WaitForSeconds(.85f);
        animator.SetBool("isJumping", false);
    }
    public void go(Vector3 v3)
    {
        //agent.SetDestination(v3);
        agent.enabled = true;
        NavMeshPath navMeshPath = new NavMeshPath();
        agent.CalculatePath(v3, navMeshPath);
        path = new List<Vector3>(navMeshPath.corners);
        animator.SetBool("isMoving", true);
        agent.enabled = false;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("AI"))
        {
            agent.isStopped = true;
            agent.ResetPath();
            animator.SetBool("isMoving", false);
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
