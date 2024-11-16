using UnityEngine;
/// <summary>
/// Behavior State for when the AI is Idle
/// </summary>
public class AIModuleActionIdle : AIModule
{
    private SimpleStateMachine<AIStates.BehaviorStates> _behaviorState;

    private Animator _animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        _animator = _brain.animator;
        _behaviorState = _brain.BehaviorState;
    }

    public override void ModuleUpdate()
    {
        ExitState();
    }

    private void ExitState()
    {
        if (_behaviorState.CurrentState != AIStates.BehaviorStates.Idle)
        {
            _brain.SetIsIdle(false);
        }
        else if (_behaviorState.CurrentState == AIStates.BehaviorStates.Idle)
        {
            _brain.SetIsIdle(true);
        }
    }
}
