using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NPC;
using TreeSharpPlus;

public class CalebBehavior : MonoBehaviour {

    //public NPCController agent;
    public NPCBehavior AgentA;
    public NPCBehavior AgentB;
    public NPCBehavior AgentC;
    public Transform target;
    public Transform target2;
    public Transform target3;
    public Transform gorgonfront;
    public Transform gorgon;
    public GameObject bubbleA;
    public GameObject bubbleB;
    public GameObject bubbleC;

    private BehaviorAgent behaviorAgent;
    private IEnumerable<IHasBehaviorObject> NPCarray;
    private bool AgentAUpset = false;
    private bool AgentBUpset = false;
    private bool AgentCUpset = false;

    // Use this for initialization
    void Start () {
        behaviorAgent = new BehaviorAgent(this.BuildTreeRoot());
        BehaviorManager.Instance.Register(behaviorAgent);
        behaviorAgent.StartBehavior();
    }
	
	// Update is called once per frame
	void Update () {
        if (bubbleA.activeSelf)
        {
            if (Input.GetKey(KeyCode.Return))
            {
                AgentA.hasRead = true;
                Debug.Log(AgentA.hasRead);
            }
        }
        if (bubbleB.activeSelf)
        {
            if (Input.GetKey(KeyCode.Return))
            {
                AgentB.hasRead = true;
            }
        }
        if (bubbleC.activeSelf)
        {
            if (Input.GetKey(KeyCode.Return))
            {
                AgentC.hasRead = true;
            }
        }
    }
    protected Node ApproachAndWait(Transform target)
    {
        return new Sequence(AgentA.NPCBehavior_GoTo(target, true), new LeafWait(1000));
    }
    protected Node ChoosePartner()
    {
        if (UnityEngine.Random.Range(-1f, 1f) > 0)
        {
            AgentCUpset = true;
            return new Sequence(
                AgentA.NPCBehavior_OrientTowards(AgentB.transform),
                AgentA.NPCBehavior_Talk("Hi Danny Bones! I like you better than Steve!", bubbleA),
                AgentB.NPCBehavior_Talk("Thanks Manny Bones!", bubbleB),
                AgentC.NPCBehavior_DoGesture(GESTURE_CODE.DISSAPOINTMENT)
            );
        }else {
            AgentBUpset = true;
            return new Sequence(
                AgentA.NPCBehavior_OrientTowards(AgentC.transform),
                AgentA.NPCBehavior_Talk("Hi Steve! I like you better than Danny Bones!", bubbleA),
                AgentC.NPCBehavior_Talk("Thanks Manny Bones!",bubbleC),
                AgentB.NPCBehavior_DoGesture(GESTURE_CODE.DISSAPOINTMENT)
            );
        }
                            
    }
    protected Node WalkAway()
    {
        if (AgentBUpset)
        {
            return new Sequence(
                new SequenceParallel(
                    AgentA.NPCBehavior_GoTo(target, false),
                    AgentC.NPCBehavior_GoTo(target, false)
                ),
                new SequenceParallel(
                    AgentA.NPCBehavior_OrientTowards(AgentC.transform),
                    AgentC.NPCBehavior_OrientTowards(AgentA.transform)
                ),
                AgentB.NPCBehavior_GoTo(gorgonfront, true),
                AgentB.NPCBehavior_OrientTowards(gorgon),
                AgentB.NPCBehavior_Talk("I am going to kill them!", bubbleB)
            );
        }else if (AgentCUpset)
        {
            return new Sequence(
                new SequenceParallel(
                    AgentA.NPCBehavior_GoTo(target3, false),
                    AgentB.NPCBehavior_GoTo(target3, false)
                ),
                new SequenceParallel(
                    AgentA.NPCBehavior_OrientTowards(AgentB.transform),
                    AgentB.NPCBehavior_OrientTowards(AgentA.transform)
                ),
                AgentC.NPCBehavior_GoTo(gorgonfront, true),
                AgentC.NPCBehavior_OrientTowards(gorgon),
                AgentC.NPCBehavior_Talk("I am going to kill them!", bubbleC)
            );
        }
        return null;
    }
    protected Node Revenge()
    {
        if (AgentBUpset)
        {
            return new Sequence(
                AgentB.NPCBehavior_GoTo(AgentA.transform, true),
                AgentB.NPCBehavior_DoGesture(GESTURE_CODE.GRAB_FRONT),
                die("a"),
                AgentB.NPCBehavior_GoTo(AgentC.transform, true),
                AgentB.NPCBehavior_DoGesture(GESTURE_CODE.GRAB_FRONT),
                die("c")
            );
        }
        else if (AgentCUpset)
        {
            return new Sequence(
                AgentC.NPCBehavior_GoTo(AgentA.transform, true),
                AgentC.NPCBehavior_DoGesture(GESTURE_CODE.GRAB_FRONT),
                die("a"),
                AgentC.NPCBehavior_GoTo(AgentB.transform, true),
                AgentC.NPCBehavior_DoGesture(GESTURE_CODE.GRAB_FRONT),
                die("b")
            );
        }
        return null;
    }
    protected Node Pursuit()
    {
        if (AgentBUpset)
        {
            return new DecoratorLoop(
                updatePursuit("c")
            );
        }
        else if (AgentCUpset)
        {
            return new DecoratorLoop(
                updatePursuit("b")
            );
        }
        return null;
    }
    private Node updatePursuit(String agent)
    {
        target2.position.Set(UnityEngine.Random.Range(-50, 50), 0, UnityEngine.Random.Range(-50, 50));
        if (agent.Equals("c"))
            return new SequenceParallel(AgentC.NPCBehavior_GoTo(target2, true), new Sequence(new LeafWait(10000),AgentB.NPCBehavior_GoTo(AgentC.transform, true),die("c"),AgentB.NPCBehavior_DoGesture(GESTURE_CODE.HURRAY)));
        else
            return new SequenceParallel(AgentB.NPCBehavior_GoTo(target2, true), new Sequence(new LeafWait(10000), AgentC.NPCBehavior_GoTo(AgentB.transform, true),die("b"),AgentC.NPCBehavior_DoGesture(GESTURE_CODE.HURRAY)));
    }
    private Node die(string agent)
    {
        if (agent.Equals("c"))
        {
            //AgentC.StopAllCoroutines(); is there a way to stop all behaviors for this agent?
            return AgentC.NPCBehavior_DoGesture(GESTURE_CODE.DIE);
        }
        else if(agent.Equals("b"))
        {
            //AgentB.StopAllCoroutines();
            return AgentB.NPCBehavior_DoGesture(GESTURE_CODE.DIE);
        }else
        {
            //AgentA.StopAllCoroutines();
            return AgentA.NPCBehavior_DoGesture(GESTURE_CODE.DIE);
        }
    }
    protected Node BuildTreeRoot()
    {
        Node root = new Sequence(
                        AgentB.NPCBehavior_OrientTowards(AgentA.transform),
                        AgentC.NPCBehavior_OrientTowards(AgentA.transform),
                        ChoosePartner(),
                        WalkAway(),
                        new LeafWait(2000),
                        Revenge()
                        //Pursuit()
                    );
        return root;
    }
}
