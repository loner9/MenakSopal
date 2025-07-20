using UnityEngine;

public class NPCState
{
    protected NPC npc;
    protected NPCStateMachine npcStateMachine;

    public NPCState(NPC npc, NPCStateMachine npcStateMachine)
    {
        this.npc = npc;
        this.npcStateMachine = npcStateMachine;
    }
    
    public virtual void EnterState()
    {
        Debug.Log($"NPC {npc.npcName}: Entering {GetType().Name}");
    }

    public virtual void ExitState()
    {
        Debug.Log($"NPC {npc.npcName}: Exiting {GetType().Name}");
    }

    public virtual void FrameUpdate()
    {
    }

    public virtual void PhysicsUpdate()
    {
    }

    public virtual void AnimationTriggerEvent(NPC.AnimationTriggerType triggerType)
    {
    }
}