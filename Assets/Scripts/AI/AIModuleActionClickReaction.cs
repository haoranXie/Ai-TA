using UnityEngine;
using System;

public class AIModuleActionClickReaction : AIModule
{
    private SimpleStateMachine<AIStates.BehaviorStates> _behaviorState;
    private AIModuleActionTalk _actionTalk;
    private Animator _animator;
    private AudioSource _audioSource;
    private CapsuleCollider2D _collider;
    private Camera _camera;

    // Variables for detecting click vs. hold
    private float holdThreshold = 0.3f; // 300ms for an instant click
    private float holdDuration = 0f;    // Time for which the mouse button has been held down
    private bool isHolding = false;     // Is the mouse button being held?

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        _animator = _brain.animator;
        _behaviorState = _brain.BehaviorState;
        _audioSource = _brain.audioSource;
        _actionTalk = GetComponent<AIModuleActionTalk>();
        _collider = _brain.capsuleCollider;
        _camera = Camera.main;
    }

    public override void ModuleUpdate()
    {
        // If the mouse button is pressed, start tracking the duration
        if (Input.GetMouseButtonDown(0))
        {
            // Reset the hold duration and tracking flag
            holdDuration = 0f;
            isHolding = true;
        }

        // If the mouse button is being held down, increment the duration
        if (Input.GetMouseButton(0) && isHolding)
        {
            holdDuration += Time.deltaTime;

            // If the hold duration exceeds the threshold, it's a "hold" action
            if (holdDuration >= holdThreshold)
            {
                isHolding = false;  // No longer consider it an instant click
            }
        }

        // If the mouse button is released, handle the action
        if (Input.GetMouseButtonUp(0))
        {
            // Only process the click if it's an instant click (not a hold)
            if (isHolding && holdDuration < holdThreshold)
            {
                HandleClick();  // Process the click action
            }

            // Reset the hold tracking variables after release
            isHolding = false;
            holdDuration = 0f;
        }
    }

    private void HandleClick()
    {
        if (_collider == null) return; // Exit if no collider is assigned

        // Convert the click position to world coordinates
        Vector2 clickPosition = _camera.ScreenToWorldPoint(Input.mousePosition);

        // Check if the click position is within the character's collider bounds
        if (_collider.OverlapPoint(clickPosition))
        {
            //If they're talking, send them to straight Idle animation
            if (_behaviorState.CurrentState == AIStates.BehaviorStates.Talking)
            {
                _behaviorState.ChangeState(AIStates.BehaviorStates.Idle);
            }
            else
            {
                _behaviorState.ChangeState(AIStates.BehaviorStates.Touching);
                _brain.SetIsTouchingBody();
            }
            _audioSource.Stop();
            _actionTalk.StopTypingEffect();
        }
    }

    public void OnTouchBodyHappyEndComplete()
    {
        _behaviorState.ChangeState(AIStates.BehaviorStates.Idle);
    }
}
