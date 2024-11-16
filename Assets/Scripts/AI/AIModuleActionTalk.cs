using System.Collections;
using TMPro;
using UnityEngine;

public class AIModuleActionTalk : AIModule
{
    [SerializeField]
    private float typingSpeed = 0.05f;

    private SimpleStateMachine<AIStates.BehaviorStates> _behaviorState;
    private TextMeshPro _dialogueText;
    private GameObject _dialogueBubbleObject;
    private Animator _animator;
    private Coroutine _typingCoroutine;
    private TMP_InputField _inputField;


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        _behaviorState = _brain.BehaviorState;
        _dialogueText = _brain.dialogueText;
        _dialogueBubbleObject = _brain.dialogueBubbleObject;
        _inputField = _brain.dialogueInput;
        _animator = _brain.animator;
    }

    public override void ModuleUpdate()
    {
        HandleExitState();
    }
    
    
    private void HandleExitState()
    {
        
        if (_behaviorState.CurrentState != AIStates.BehaviorStates.Talking)
        {
            _brain.SetIsTalking(false);
            _dialogueBubbleObject.SetActive(false);
            StopTypingEffect();
        }
    }

    public void Talk(string messageContent, string responseTone = "Supportive")
    {
        if (_behaviorState.CurrentState == AIStates.BehaviorStates.PickedUp) return;

        _behaviorState.ChangeState(AIStates.BehaviorStates.Talking);
        
        StopTypingEffect();
        SetTalkingAnimation(responseTone);
        _typingCoroutine = StartCoroutine(AnimateText(messageContent));
    }

    private void SetTalkingAnimation(string responseTone)
    {
        _brain.SetIsTalking(true);
        switch (responseTone)
        {
            case "Neutral":
                _brain.SetEmotionNeutral(true);
                _brain.SetEmotionEnthusiastic(false);
                _brain.SetEmotionSupportive(false);
                break;
            
            case "Supportive":
                _brain.SetEmotionNeutral(false);
                _brain.SetEmotionEnthusiastic(false);
                _brain.SetEmotionSupportive(true);
                break;
            
            case "Enthusiastic":
                _brain.SetEmotionNeutral(false);
                _brain.SetEmotionEnthusiastic(true);
                _brain.SetEmotionSupportive(false);
                break;
        }
    }

    public void StopTypingEffect()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        _dialogueBubbleObject.SetActive(false);
    }

    private IEnumerator AnimateText(string messageContent)
    {
        _dialogueBubbleObject.SetActive(true);
        _dialogueText.text = "";

        foreach (char letter in messageContent)
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}