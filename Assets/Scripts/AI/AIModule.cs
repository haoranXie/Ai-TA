using UnityEngine;


/// <summary>
/// A class meant to be overridden that can performs various behaviors of the AI
/// </summary>
//[RequireComponent(typeof(AIBrain))]
public class AIModule : MonoBehaviour
{
    protected AIBrain _brain;
    protected bool _abilityInitialized = false;
    public virtual bool AbilityInitialized { get { return _abilityInitialized; } }

    // This method is meant to be overridden by subclasses to implement custom behavior
    public virtual void ModuleUpdate() { }

    protected virtual void Awake()
    {
        _brain = GetComponentInParent<AIBrain>();
    }
    protected virtual void Start()
    {
        _abilityInitialized = true;
    }
}

/* Example inherited implemtation
public class AIModuleName : AIModule
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
    }
    
    public override void ModuleUpdate() { }

}
*/