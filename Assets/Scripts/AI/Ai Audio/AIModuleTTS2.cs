using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;


public class VoiceSettings
{
    public float stability;
    public float similarity_boost;
    public float style;
}

public class TTSData
{
    public string text;
    public string model_id;
    public VoiceSettings voice_settings;
}

// Laura find more voices here: https://api.elevenlabs.io/v1/voices
// https://github.com/sopermanspace/Elevenlabs/blob/main/elevenlabs.cs

public class AIModuleTTS2 : AIModule
{
    /// <summary>
    /// Options for voices selectable in settings Menu
    /// </summary>
    [SerializeField] private String[] _voiceIds;
    private AudioSource audioSource;
    private String _voiceId;
    
    [Range(0.0f, 1.0f)] public float stability;
    [Range(0.0f, 1.0f)] public float similarity_boost;
    [Range(0.0f, 1.0f)] public float style;

    private string ttsUrl = "https://api.elevenlabs.io/v1/text-to-speech/{0}/stream";

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        audioSource = _brain.audioSource;
        _voiceId = _brain.voiceId;
    }
    
    public override void ModuleUpdate() { }


    public IEnumerator CallTTS(string text)
    {
        string modelId = "eleven_turbo_v2_5";
        string url = string.Format(ttsUrl, _voiceId);

        TTSData ttsData = new TTSData
        {
            text = text.Trim(),
            model_id = modelId,
            voice_settings = new VoiceSettings
            {
                stability = stability,
                similarity_boost = similarity_boost,
                style = style
            }
        };

        string jsonData = JsonUtility.ToJson(ttsData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerAudioClip(new Uri(url), AudioType.MPEG);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", _brain.ElevenLabsAPIKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
                yield break;
            }

            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(request);

        if (audioClip != null)
            {
                audioSource.clip = audioClip; 
                audioSource.PlayOneShot(audioClip);
            yield return new WaitForSeconds(audioClip.length * 0.1f);
            }
        else
        {
            // the audio is null so download the audio again
            yield return StartCoroutine(CallTTS(text));
        }


            // Wait for the audio clip to finish playing
            yield return new WaitForSeconds(audioClip.length);
        }
    }
    
    public void HandleProfileInput(int val)
    {
        if (val == 0)
        {
            _voiceId = _brain.voiceId;
            return;
        }

        _voiceId = _voiceIds[val - 1];

    }
}
