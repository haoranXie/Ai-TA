using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Azure;
using System;
using Azure.AI.OpenAI;
using Azure.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

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
    private AIModuleAzureChatGPT _aiModuleAzureChatgpt;


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
        _aiModuleAzureChatgpt = this.GetComponentInParent<AIModuleAzureChatGPT>();
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
         if(_brain.UseAzure){_aiModuleAzureChatgpt.SendAnalysisRequest(res.Text, inputField);}
         else{aiModuleChatgpt?.HandleCalendarRequest(res.Text, inputField);}
     }
    /*
    private async void ProcessRecording()
    {
        var endpoint = new Uri("https://haora-m5sknlh0-northcentralus.cognitiveservices.azure.com/openai/deployments/whisper/audio/translations?api-version=2024-06-01");
        var credentials = new AzureKeyCredential("4FH7FkDY0eahNUblg7GVY5EJjBDT9OK1ypYfQ1yMQgE5j6VQ6KgVJQQJ99BAACHrzpqXJ3w3AAAAACOGjVso");
        byte[] data = SaveWav.Save(filename, clip);
        // Debug.Log(data);
        var deploymentName = "whisper";

        try
        {
            var openAIClient = new AzureOpenAIClient(endpoint, credentials);

            var audioClient = openAIClient.GetAudioClient(deploymentName);

            string filePath = Path.Combine(Application.persistentDataPath, filename);
        
            Debug.Log(filePath);

            var result = await audioClient.TranslateAudioAsync(filePath);
            // var result = await audioClient.TranscribeAudioAsync(filePath);
            // var result = await audioClient.TranscribeAudioAsync("/Users/bagsangjun/Desktop/TalkForAFewSeconds16.wav");
            foreach (var item in result.Value.Text)
            {
                Debug.Log(item);
            }
        }
        catch(RequestFailedException ex)
        {
            Debug.LogError($"Azure OpenAI Error: {ex.Message}");
            Debug.LogError($"Status Code: {ex.Status}");
            Debug.LogError($"Error Details: {ex.ErrorCode}");
            Debug.LogException(ex);
        }
    }
    */

    // private async void ProcessRecording()
    // {
    //     if (clip == null)
    //     {
    //         Debug.LogWarning("No audio clip to process.");
    //         return;
    //     }

    //     byte[] wavFile = ConvertAudioClipToWav(clip);

    //     string filePath = System.IO.Path.Combine(Application.persistentDataPath, "recordedAudio.wav");

    //     System.IO.File.WriteAllBytes(filePath, wavFile);

    //     Debug.Log($"Audio saved to: {filePath}");
        
    //     var endpoint = new Uri("https://haora-m5sj74yw-eastus2.cognitiveservices.azure.com/openai/deployments/whisper/audio/translations?api-version=2024-06-01");
    //     var credentials = new AzureKeyCredential("3RaOZhYcc7nq6ncRdPZCZX3ekREH131aWWierSvropi8KFp3I87gJQQJ99BAACHYHv6XJ3w3AAAAACOGKbJr");
    //     var deploymentName = "whisper";

    //     var openAIClient = new AzureOpenAIClient(endpoint, credentials);

    //     var audioClient = openAIClient.GetAudioClient(deploymentName);

    //     var result = await audioClient.TranscribeAudioAsync(filePath);

    //     foreach (var item in result.Value.Text)
    //     {
    //     Debug.Log(item);
    //     }

    // }

    // private byte[] ConvertAudioClipToWav(AudioClip clip)
    // {
    //     float[] samples = new float[clip.samples * clip.channels];
    //     clip.GetData(samples, 0);

    //     byte[] wavData = new byte[samples.Length * 2];
    //     for (int i = 0; i < samples.Length; i++)
    //     {
    //         // Normalize the float samples to 16-bit PCM values
    //         short sample = (short)Mathf.Clamp(samples[i] * 32767, short.MinValue, short.MaxValue);
    //         wavData[i * 2] = (byte)(sample & 0xFF);        // Lower byte
    //         wavData[i * 2 + 1] = (byte)((sample >> 8) & 0xFF); // Upper byte
    //     }

    //     // Create WAV header
    //     byte[] header = WriteWavHeader(clip, wavData.Length);
    //     byte[] wavFile = new byte[header.Length + wavData.Length];
    //     System.Buffer.BlockCopy(header, 0, wavFile, 0, header.Length);
    //     System.Buffer.BlockCopy(wavData, 0, wavFile, header.Length, wavData.Length);

    //     return wavFile;
    // }

    // private byte[] WriteWavHeader(AudioClip clip, int dataLength)
    // {
    //     int sampleRate = clip.frequency;
    //     short channels = (short)clip.channels;
    //     int byteRate = sampleRate * channels * 2; // 16-bit audio

    //     using (var memStream = new System.IO.MemoryStream(44))
    //     using (var writer = new System.IO.BinaryWriter(memStream))
    //     {
    //         // RIFF chunk
    //         writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
    //         writer.Write(36 + dataLength); // File size minus 8 bytes
    //         writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

    //         // fmt subchunk
    //         writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
    //         writer.Write(16); // Subchunk1 size (PCM)
    //         writer.Write((short)1); // Audio format (1 = PCM)
    //         writer.Write(channels); // Number of channels
    //         writer.Write(sampleRate); // Sample rate
    //         writer.Write(byteRate); // Byte rate
    //         writer.Write((short)(channels * 2)); // Block align
    //         writer.Write((short)16); // Bits per sample

    //         // data subchunk
    //         writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
    //         writer.Write(dataLength); // Data chunk size

    //         return memStream.ToArray();
    //     }
    // }
}
