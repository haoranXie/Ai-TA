using UnityEngine;
using OpenAI;
using TMPro;
using UnityEngine.UI;

public class AIBrain : MonoBehaviour, IDataPersistence
{
    [Header("AI")] [SerializeField] public bool UseAzure = true;
    public string openAPIKey;
    public string OpenWeatherAPIKey;
    public string ElevenLabsAPIKey;
    public string NewsAPIKey;

    [Header("TTS Settings")]
    [SerializeField]
    public string voiceId;
    
    [Header("Defaults")]
    public AIStates.BehaviorStates defaultBehaviorState = AIStates.BehaviorStates.Idle;
    
    [Header("Bindings")]
    public Animator animator;
    public AudioSource audioSource;
    public SpriteRenderer sprite;
    
    [Header("Buttons")]
    public Button speakerButton;
    public Button powerButton;
    public Button settingsButton;
    
    [Header("Text")]
    public TextMeshPro dialogueText;
    public TMP_InputField dialogueInput;
    public GameObject dialogueBubbleObject;
    public GameObject inputFieldObject;
    public GameObject buttonsObject;
    
    [Header("Windows")]
    public GameObject settingsWindow;

    public GameObject apiKeysWindow;
    
    [Header("Data Management")]
    public DataPersistenceManager dataPersistenceManager;
    
    [Header("Events")]
    [Tooltip("If true, the AI state machine will fire events when the AI state is changed")]
    public bool StateChangeEvent = true;
    
    public CapsuleCollider2D capsuleCollider;
    public SimpleStateMachine<AIStates.BehaviorStates> BehaviorState;
    public OpenAIApi openAIAPI;

    protected AIModule[] _aiModules;
    public UserProfile userProfile;
    private APIKeys _apiKeys;

    // Define Animator parameter hashes for efficiency
    private static readonly int IsIdleHash = Animator.StringToHash("IsIdle");
    private static readonly int IsTalkingHash = Animator.StringToHash("IsTalking");
    private static readonly int IsRaisedHash = Animator.StringToHash("IsRaised");
    private static readonly int IsTouchingUserHash = Animator.StringToHash("IsTouchingUser");
    private static readonly int IsTouchingBodyHash = Animator.StringToHash("IsTouchingBody");
    private static readonly int IsThinkingHash = Animator.StringToHash("IsThinking");
    private static readonly int IsStudyingHash = Animator.StringToHash("IsStudying");
    private static readonly int IsShuttingDownHash = Animator.StringToHash("IsShuttingDown");
    private static readonly int EmotionNeutral = Animator.StringToHash("EmotionNeutral");
    private static readonly int EmotionSupportive = Animator.StringToHash("EmotionSupportive");
    private static readonly int EmotionEnthusiastic = Animator.StringToHash("EmotionEnthusiastic");
    protected virtual void Awake()
    {
        openAIAPI = new OpenAIApi(openAPIKey);
        BehaviorState = new SimpleStateMachine<AIStates.BehaviorStates>(gameObject, StateChangeEvent);
        capsuleCollider = this.GetComponent<CapsuleCollider2D>();
        CacheModules();
    }

    protected virtual void Start()
    {
        inputFieldObject.SetActive(false);
        dialogueBubbleObject.SetActive(false); 
        buttonsObject.SetActive(false);
        BehaviorState.ChangeState(defaultBehaviorState);
        
        openAPIKey = _apiKeys.openAPIKey;
        OpenWeatherAPIKey = _apiKeys.OpenWeatherAPIKey;
        ElevenLabsAPIKey = _apiKeys.ElevenLabsAPIKey;
        NewsAPIKey = _apiKeys.NewsAPIKey;
        voiceId = _apiKeys.voiceId;
        openAIAPI = new OpenAIApi(openAPIKey);
    }

    private void LoadAPIKeyData()
    {
        if (_apiKeys == null) return;
        openAPIKey = _apiKeys.openAPIKey;
        OpenWeatherAPIKey = _apiKeys.OpenWeatherAPIKey;
        ElevenLabsAPIKey = _apiKeys.ElevenLabsAPIKey;
        NewsAPIKey = _apiKeys.NewsAPIKey;
        voiceId = _apiKeys.voiceId;
        openAIAPI = new OpenAIApi(openAPIKey);
    }

    protected virtual void Update()
    {
        UpdateModules();
    }
    
    public virtual void CacheModules()
    {
        _aiModules = GetComponents<AIModule>();
    }
    
    protected virtual void UpdateModules()
    {
        foreach (AIModule module in _aiModules)
        {
            if (module.enabled && module.AbilityInitialized)
            {
                module.ModuleUpdate();
            }
        }
    }
    
    // Methods to set each animation parameter
    public void SetIsIdle(bool value) => animator.SetBool(IsIdleHash, value);
    public void SetIsTalking(bool value) => animator.SetBool(IsTalkingHash, value);
    public void SetIsRaised(bool value) => animator.SetBool(IsRaisedHash, value);
    public void SetIsTouchingUser(bool value) => animator.SetBool(IsTouchingUserHash, value);
    public void SetIsThinking(bool value) => animator.SetBool(IsThinkingHash, value);
    public void SetEmotionNeutral(bool value) => animator.SetBool(EmotionNeutral, value);
    public void SetEmotionSupportive(bool value) => animator.SetBool(EmotionSupportive, value);
    public void SetEmotionEnthusiastic(bool value) => animator.SetBool(EmotionEnthusiastic, value);
    public void SetIsStudying(bool value) => animator.SetBool(IsStudyingHash, value);

    public void SetIsTouchingBody() => animator.SetTrigger(IsTouchingBodyHash);

    public void HandlePowerOff()
    {
        animator.SetTrigger(IsShuttingDownHash);
        Invoke(nameof(ExitApplication), 2.0f); // Delay exit to allow shutdown animation to play
    }

    private void ExitApplication()
    {
        Application.Quit();
    }
    public void LoadData(UserProfile data, APIKeys keys)
    {
        this.userProfile = data;
        _apiKeys = keys;
        LoadAPIKeyData();
    }

    public void SaveData(ref UserProfile data, APIKeys keys)
    {
        
        // Implement save logic here
    }
}
