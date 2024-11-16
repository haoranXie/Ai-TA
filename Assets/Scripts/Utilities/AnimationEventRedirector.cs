using UnityEngine;
/// <summary>
/// Redirects animation events called by the animator
/// </summary>

public class AnimationEventRedirector : MonoBehaviour
{
    private AIModuleActionClickReaction _aiModuleActionClickReaction;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _aiModuleActionClickReaction = GetComponentInParent<AIModuleActionClickReaction>();
    }
    
    public void OnTouchBodyHappyEndComplete()
    {
        if(_aiModuleActionClickReaction != null) _aiModuleActionClickReaction.OnTouchBodyHappyEndComplete();
    }
}
