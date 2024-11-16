using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIModuleSST : AIModule
{
    [SerializeField] private Button button;
    [SerializeField] private int microphoneIndex = 0;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private KeyCode pushToTalkKey = KeyCode.BackQuote; // Default push-to-talk key is "`"
    
    private TMP_InputField inputField;
    private TMP_InputField textMeshprofield;
    private AIModuleChatgpt aiModuleChatgpt;
    private AIModuleActionTalk aiModuleActionTalk;
    private AudioClip clip;
    private OpenAIApi openai;

    private bool isRecording = false; // Tracks recording status
    private readonly string filename = "Output.wav";
    
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        openai = _brain.openAIAPI;
        aiModuleChatgpt = this.GetComponentInParent<AIModuleChatgpt>();
        button.onClick.AddListener(ToggleRecording);
        textMeshprofield = _brain.dialogueInput;
        inputField = _brain.dialogueInput;
        aiModuleActionTalk = GetComponent<AIModuleActionTalk>();
    }

    private void Update()
    {
        // Check for push-to-talk key press and release
        if (Input.GetKeyDown(pushToTalkKey) && !isRecording)
        {
            StartRecording();
            buttonImage.sprite = selectedSprite; // Change button sprite to "selected"
        }
        else if (Input.GetKeyUp(pushToTalkKey) && isRecording)
        {
            StopRecording();
            buttonImage.sprite = defaultSprite; // Change button sprite to default
        }
    }

    private void ToggleRecording()
    {
        isRecording = !isRecording;

        if (isRecording)
        {
            StartRecording();
            buttonImage.sprite = selectedSprite; // Change button sprite to "selected"
        }
        else
        {
            StopRecording();
            buttonImage.sprite = defaultSprite; // Change button sprite to default
        }
    }

    private void StartRecording()
    {
        isRecording = true;
        // Start recording with the specified microphone index
        clip = Microphone.Start(Microphone.devices[microphoneIndex], false, 30, 44100);
    }

    private void StopRecording()
    {
        isRecording = false;
        if (Microphone.IsRecording(Microphone.devices[microphoneIndex]))
        {
            Microphone.End(Microphone.devices[microphoneIndex]);
            ProcessRecording();
        }
    }

    private async void ProcessRecording()
    {
        byte[] data = SaveWav.Save(filename, clip);
        var req = new CreateAudioTranscriptionsRequest
        {
            FileData = new FileData() { Data = data, Name = "audio.wav" },
            Model = "whisper-1"
        };
        
        var res = await openai.CreateAudioTranscription(req);
        textMeshprofield.text = res.Text;
        aiModuleChatgpt?.HandleCalendarRequest(res.Text, inputField);
    }
}
