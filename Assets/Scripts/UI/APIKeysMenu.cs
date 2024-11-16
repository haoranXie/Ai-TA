using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class APIKeysMenu : MonoBehaviour, IDataPersistence
{
    // Reference to the APIKeys instance that stores user keys
    public APIKeys apiKeys;

    // Input fields for each API key
    
    public TMP_InputField openAIKeyField;
    public TMP_InputField openWeatherAPIKeyField;
    public TMP_InputField elevenLabsAPIKeyField;
    public TMP_InputField newsAPIKeyField;
    public TMP_InputField voiceIdField;

    private void Start()
    {
        // Load the existing API keys if available
        LoadAPIKeys();
    }

    public void SaveAPIKeys()
    {
        // Set the APIKeys fields to the input values
        apiKeys.openAPIKey = openAIKeyField.text;
        apiKeys.OpenWeatherAPIKey = openWeatherAPIKeyField.text;
        apiKeys.ElevenLabsAPIKey = elevenLabsAPIKeyField.text;
        apiKeys.NewsAPIKey = newsAPIKeyField.text;
        apiKeys.voiceId = voiceIdField.text;
        
        // Use DataPersistenceManager to save the API keys
        DataPersistenceManager.instance.SaveAPIKeys(apiKeys);
    }

    public void LoadAPIKeys()
    {
        // Load the API keys if available
        DataPersistenceManager.instance.LoadGame();

        apiKeys = DataPersistenceManager.instance._apiKeys;

        // Populate input fields with loaded values
        openAIKeyField.text = apiKeys.openAPIKey;
        openWeatherAPIKeyField.text = apiKeys.OpenWeatherAPIKey;
        elevenLabsAPIKeyField.text = apiKeys.ElevenLabsAPIKey;
        newsAPIKeyField.text = apiKeys.NewsAPIKey;
        voiceIdField.text = apiKeys.voiceId;
    }
    
    public void LoadData(UserProfile data, APIKeys keys)
    {
        apiKeys = keys;
        openAIKeyField.text = apiKeys.openAPIKey;
        openWeatherAPIKeyField.text = apiKeys.OpenWeatherAPIKey;
        elevenLabsAPIKeyField.text = apiKeys.ElevenLabsAPIKey;
        newsAPIKeyField.text = apiKeys.NewsAPIKey;
        voiceIdField.text = apiKeys.voiceId;
    }

    public void SaveData(ref UserProfile data, APIKeys keys)
    {
        
    }
    
}

