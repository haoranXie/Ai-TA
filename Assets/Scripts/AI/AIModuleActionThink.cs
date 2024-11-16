using UnityEngine;

public class AIModuleActionThink : AIModule
{
    private SimpleStateMachine<AIStates.BehaviorStates> _behaviorState;

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        _behaviorState = _brain.BehaviorState;
    }

    public override void ModuleUpdate()
    {
        if (_behaviorState.CurrentState != AIStates.BehaviorStates.Thinking)
        {
            _brain.SetIsThinking(false);
        }
    }
}
