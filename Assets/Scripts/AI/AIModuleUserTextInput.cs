using TMPro;
using UnityEngine;

public class AIModuleUserTextInput : AIModule
{
    private TMP_InputField _inputField;
    private GameObject _inputFieldObject;
    private SimpleStateMachine<AIStates.BehaviorStates> _behaviorState;

    private AIModuleChatgpt _aiModuleChatgpt;

    protected override void Start()
    {
        base.Start();
        _inputField = _brain.dialogueInput;
        _inputFieldObject = _brain.inputFieldObject;
        _behaviorState = _brain.BehaviorState;
        _aiModuleChatgpt = GetComponent<AIModuleChatgpt>();
    }
    
    public void GrabFromInputField(string input)
    {
        //The user's input doesn't work if the AI is not Idle
        //No Interruptions
        //if (_behaviorState.CurrentState != AIStates.BehaviorStates.Idle) return;
        
        if (_aiModuleChatgpt != null)
        {
            //_aiModuleChatgpt.SendReply(input, _inputField);
            //_aiModuleChatgpt.SendAnalysisRequest(input, _inputField);
            _aiModuleChatgpt.HandleCalendarRequest(input, _inputField);
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }
}
