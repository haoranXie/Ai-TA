using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generic state change event structure for passing state change data.
/// </summary>
public struct StateChangeEvent<T> where T : struct, IComparable, IConvertible, IFormattable
{
    public GameObject Target;
    public SimpleStateMachine<T> StateMachine;
    public T NewState;
    public T PreviousState;

    public StateChangeEvent(GameObject target, SimpleStateMachine<T> stateMachine, T newState, T previousState)
    {
        Target = target;
        StateMachine = stateMachine;
        NewState = newState;
        PreviousState = previousState;
    }
}

/// <summary>
/// A basic state machine for Unity, using an enum type as the state.
/// </summary>
public class SimpleStateMachine<T> where T : struct, IComparable, IConvertible, IFormattable
{
    public GameObject Target { get; private set; }
    public T CurrentState { get; private set; }
    public T PreviousState { get; private set; }
    public bool TriggerEvents { get; set; }

    public event Action<StateChangeEvent<T>> OnStateChanged;

    /// <summary>
    /// Constructor for the state machine.
    /// </summary>
    /// <param name="target">The GameObject associated with this state machine.</param>
    /// <param name="triggerEvents">Whether the state machine should trigger state change events.</param>
    public SimpleStateMachine(GameObject target, bool triggerEvents = false)
    {
        Target = target;
        TriggerEvents = triggerEvents;
    }

    /// <summary>
    /// Changes the current state to a new state and triggers events if necessary.
    /// </summary>
    /// <param name="newState">The new state to transition to.</param>
    public void ChangeState(T newState)
    {
        if (EqualityComparer<T>.Default.Equals(newState, CurrentState))
        {
            return; // Exit if the new state is the same as the current state
        }

        PreviousState = CurrentState;
        CurrentState = newState;

        OnStateChanged?.Invoke(new StateChangeEvent<T>(Target, this, newState, PreviousState));
        /*
        if (TriggerEvents)
        {
            Debug.Log($"State changed from {PreviousState} to {CurrentState} on {Target.name}");
        }
        */
    }

    /// <summary>
    /// Restores the previous state and triggers the state change event if necessary.
    /// </summary>
    public void RestorePreviousState()
    {
        T tempState = CurrentState;
        CurrentState = PreviousState;
        PreviousState = tempState;

        OnStateChanged?.Invoke(new StateChangeEvent<T>(Target, this, CurrentState, PreviousState));
        
        /*
        if (TriggerEvents)
        {
            Debug.Log($"State restored to {CurrentState} on {Target.name}");
        }
        */
    }
}
