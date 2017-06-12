using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TreeSharpPlus;

using NPC;
using System;

/// <summary>
/// We will use this module as the connector of NPC with
/// TreeSharpPlus. The main purpose of this module is to
/// be a shell for the already implemented affordances of
/// the agents. Nodes will tick here and the module on each
/// tick will return the current status of each node.
/// 
/// Note that for each affordance, we will need two functions:
///     1. Node return to create the actual Behavior Tree component
///     2. A wrapper controller on the affordance which will be ticked on
///        and need to return, on tick, the status of the node.
/// Although this will add an extra layer of complexity, I will concentrate
/// all the implementation on this module rather than mixing TreeSharpPlus
/// with the NPC affordances. That will allow, in the long run, for more
/// freedom, scalable, maitainable and clear code.
/// 
/// </summary>

public class NPCBehavior : MonoBehaviour, INPCModule, IHasBehaviorObject {

    #region Members
    
    private NPCController g_NPCController;
    public bool Enabled = true;
    public bool hasRead = false;
    private bool g_GestureRunning = false;
    BehaviorObject g_BehaviorObject;
    private bool g_Initialized = false;
    private Transform[] g_RoamingPoints;

    public BehaviorObject Behavior {
        get {
            return g_BehaviorObject;
        }
    }

    #endregion

    #region Unity_Methods

    public void Awake() { }

    #endregion

    #region Public_Functions
    /// <summary>
    /// Void methods are self contained behaviors of an agent
    /// </summary>
    /// <param name="code"></param>
    public void Behavior_DoTimedGesture(GESTURE_CODE code) {
        BehaviorEvent be = new BehaviorEvent(doEvent => NPCBehavior_DoTimedGesture(code), new IHasBehaviorObject[] { this });
        be.StartEvent(1f);
    }

    public Node ApproachAndWait(Transform target, bool run) {
        return new Sequence(NPCBehavior_GoTo(target, run), new LeafWait(1000));
    }

    public Node NPCBehavior_TakeSit(NPCObject chair) {
        Transform t = chair.MainInteractionPoint;
        return NPCBehavior_TakeSit(t);
    }

    public Node NPCBehavior_TakeSit(Transform t) {
        return new Sequence(
                NPCBehavior_GoTo(t, false),
                NPCBehavior_OrientTowards(t.position + t.forward),
                NPCBehavior_DoTimedGesture(GESTURE_CODE.SIT),
                NPCBehavior_DoGesture(GESTURE_CODE.SITTING,true)
            );
    }

    public Node NPCBehavior_PatrolRandomPoints(Transform[] points) {
        g_RoamingPoints = g_RoamingPoints == null ? points : g_RoamingPoints;
        return new DecoratorLoop (
            new SequenceShuffle(
                new LeafInvoke(() => Behavior_DoGesture(GESTURE_CODE.LOOK_AROUND)),
                new LeafWait(3500),
                new LeafInvoke(() => Behavior_Wander(false)),
                new LeafWait(2500),
                new LeafInvoke(() => Behavior_DoGesture(GESTURE_CODE.THINK))
            )
        );
    }

    public Node NPCBehavior_OrientTowards(Transform t) {
        Debug.Log("OrientTowards");
        return new LeafInvoke(() => Behavior_OrientTowards(t));
    }

    public Node NPCBehavior_OrientTowards(Vector3 t) {
        return new LeafInvoke(() => Behavior_OrientTowards(t));
    }

    public Node NPCBehavior_LookAt(Transform t, bool start) {
        if(start)
            return new LeafInvoke(() => Behavior_LookAt(t));
        else
            return new LeafInvoke(() => Behavior_StopLookAt());
    }

    public Node NPCBehavior_GoTo(Transform t, bool run) {
        g_NPCController.Debug(g_NPCController + " GoTo: " + t);
        return new LeafInvoke(
            () => Behavior_GoTo(t, run)                           
        );
    }

    public Node NPCBehavior_DoTimedGesture(GESTURE_CODE gesture, System.Object o = null) {
        return NPCBehavior_DoGesture(gesture, o, true);
    }

    public Node NPCBehavior_DoGesture(GESTURE_CODE gesture, System.Object o = null, bool timed = false) {
        return new LeafInvoke(
            () => Behavior_DoGesture(gesture,o, timed)
        );
    }
    //---------------------------Caleb's affordances
    public Node NPCBehavior_Talk(String speech, GameObject bubble)
    {
        return new Sequence(
                new LeafInvoke(() => Behavior_Talk(speech, bubble))
                //new LeafWait(2000)
            ); 
    }
    //---------------------------
    public override string ToString() {
        return g_NPCController.name;
    }

    #endregion

    #region Private_Functions
    //---------------------------Caleb's affordances
    private RunStatus Behavior_Talk(String speech, GameObject bubble)
    {
            bubble.SetActive(true);
            bubble.GetComponentInChildren<Text>().text = speech;
            if (hasRead)
            {
                Debug.Log("hasRead");
                bubble.SetActive(false);
                hasRead = false;
                return RunStatus.Success;
            }else
            {
                Debug.Log("reading");
                return RunStatus.Running;
            }
    }
    //---------------------------
    private RunStatus Behavior_OrientTowards(Vector3 t) {
        if (g_NPCController.Body.TargetOrientation != t) {
            g_NPCController.OrientTowards(t);
        }
        return g_NPCController.Body.Oriented ? RunStatus.Success : RunStatus.Running;
    }

    private RunStatus Behavior_OrientTowards(Transform t) {
        if(g_NPCController.Body.TargetOrientation != t.position ) {
            g_NPCController.OrientTowards(t.position);
        }
        return g_NPCController.Body.Oriented ? RunStatus.Success : RunStatus.Running;
    }

    private RunStatus Behavior_DoGesture(GESTURE_CODE gest, System.Object o = null, bool timed = false) {
        if (g_NPCController.Body.IsGesturePlaying(gest)) {
            return RunStatus.Running;
        } else if (g_GestureRunning) {
            g_GestureRunning = false;
            return RunStatus.Success;
        }  else {
            try {
                g_NPCController.Body.DoGesture(gest, o, timed);
                g_GestureRunning = true;
                return RunStatus.Running;
            } catch (System.Exception e ) {
                g_NPCController.Debug("Could not initialize gesture with error: " + e.Message);
                return RunStatus.Failure;
            }
        }
        
    }

    private RunStatus Behavior_Wander(bool run) {
        if (!g_NPCController.Body.Navigating) {
            try {
                Transform t = g_RoamingPoints[(int)(UnityEngine.Random.value * (g_RoamingPoints.Length - 1))];
                if (run)
                    g_NPCController.RunTo(t.position);
                else g_NPCController.GoTo(t.position);
                return RunStatus.Running;
            } catch (System.Exception e) {
                g_NPCController.Debug(e.Message);
                return RunStatus.Failure;
            }
        } else { return RunStatus.Success; }
    }

    private RunStatus Behavior_GoTo(Transform t, bool run) {
        if (g_NPCController.Body.IsAtTargetLocation(t.position)) {
            g_NPCController.Debug("Finished go to");
            return RunStatus.Success;
        }
        else {
            try {
                Debug.Log("going");
                if (run)
                    g_NPCController.RunTo(t.position);
                else g_NPCController.GoTo(t.position);
                return RunStatus.Running;
            } catch(System.Exception e) {
                // this will occur if the target is unreacheable
                Debug.Log("failure");
                return RunStatus.Failure;
            }
        }
    }

    private RunStatus Behavior_StopLookAt() {
        g_NPCController.Body.StopLookAt();
        return RunStatus.Success;
    }

    private RunStatus Behavior_LookAt(Transform t) {
        g_NPCController.Body.StartLookAt(t);
        g_NPCController.Debug("Finished look at");
        return RunStatus.Success;
    }

    #endregion

    #region INPCModule

    public void InitializeModule() {
        g_NPCController = GetComponent<NPCController>();
        g_BehaviorObject = new BehaviorAgent(
                                new DecoratorLoop(
                                    new LeafAssert(() => true)));
        BehaviorManager.Instance.Register((IBehaviorUpdate)g_BehaviorObject);
        g_NPCController.Debug("NPCBehavior - Initialized: " + name);
        g_Initialized = true;
    }

    public bool IsEnabled() {
        return Enabled;
    }

    public string NPCModuleName() {
        return "NPC TreeSP/Connector";
    }

    public NPC_MODULE_TARGET NPCModuleTarget() {
        return NPC_MODULE_TARGET.AI;
    }

    public NPC_MODULE_TYPE NPCModuleType() {
        return NPC_MODULE_TYPE.BEHAVIOR;
    }

    public void RemoveNPCModule() {
        throw new NotImplementedException();
    }

    public void SetEnable(bool e) {
        Enabled = e;
    }

    public bool IsUpdateable() {
        return false;
    }

    public void TickModule() {
        throw new NotImplementedException();
    }
    
    #endregion
}
