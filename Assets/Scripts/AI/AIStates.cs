using UnityEngine;

/// <summary>
/// The various states your AI can be in represented as enums
/// </summary>
public class AIStates
{
    /// Actions States that the AI can do but not overlap
    public enum BehaviorStates
    {
        Idle,
        Thinking,
        Touching,
        Talking,
        PickedUp
    }
}
