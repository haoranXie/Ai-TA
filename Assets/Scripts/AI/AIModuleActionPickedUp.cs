using UnityEngine;

public class AIModuleActionPickedUp : AIModule
{
    [SerializeField]
    private float requiredHoldTime = 0.5f; // Adjust for a longer/shorter hold time
    
    private SimpleStateMachine<AIStates.BehaviorStates> _behaviorState;
    private Animator _animator;
    private bool _isRaised = false;
    private float _holdDuration = 0f;
    private CapsuleCollider2D _collider;
    private Camera _camera;

    protected override void Start()
    {
        base.Start();
        _behaviorState = _brain.BehaviorState;
        _animator = _brain.animator;
        _collider = _brain.capsuleCollider; // Use the collider directly from the brain
        _camera = Camera.main;
    }
    
    public override void ModuleUpdate()
    {
        HandlePickup();
        ExitState();
    }

    private void HandlePickup()
    {
        
        if (_collider == null) return; // Exit if no collider is assigned

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonUp(0) && _isRaised)
        {
            ExitRaisedState();
        }
        // Check if the left mouse button is held down and the mouse position is within the collider
        else if (Input.GetMouseButton(0) && _collider.OverlapPoint(mousePosition) || _isRaised) 
        {
            _holdDuration += Time.deltaTime;

            // Check if hold duration has reached the required time to initiate pickup
            if (_holdDuration >= requiredHoldTime && !_isRaised)
            {
                EnterRaisedState();
            }

            // Move the character if it's in the raised state
            if (_isRaised)
            {
                Vector3 worldMousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
                worldMousePos.z = 0f; // Adjust to character's z-plane
                transform.position = worldMousePos;
            }
        }
        else
        {
            // Reset hold duration if the button is not held
            _holdDuration = 0f;
        }
    }

    private void ExitState()
    {
        if (_behaviorState.CurrentState != AIStates.BehaviorStates.PickedUp)
        {
            _brain.SetIsRaised(false);
        }
    }

    private void EnterRaisedState()
    {
        _isRaised = true;
        _behaviorState.ChangeState(AIStates.BehaviorStates.PickedUp);
        _brain.SetIsRaised(true);
    }

    private void ExitRaisedState()
    {
        _isRaised = false;
        _behaviorState.RestorePreviousState();
        _behaviorState.ChangeState(AIStates.BehaviorStates.Idle);
        _brain.SetIsRaised(false);
    }
}
