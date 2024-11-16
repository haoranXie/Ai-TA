using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIModuleAwareness : AIModule, IDataPersistence
{
    private UserProfile _userProfile;
    private AIModuleChatgpt _aiModuleChatgpt;
    private AIModulePromtEngineering _aiModulePromtEngineering;

    private float _lastTriggeredAverageSentiment = 0f; // Track the last triggered sentiment

    protected override void Awake()
    {
        base.Awake();
    }
    
    protected override void Start()
    {
        base.Start();
        _aiModuleChatgpt = _brain.GetComponent<AIModuleChatgpt>();
        _aiModulePromtEngineering = _brain.GetComponentInChildren<AIModulePromtEngineering>();
    }

    public override void ModuleUpdate()
    {
        CheckSentimentThreshold();
    }

    private void CheckSentimentThreshold()
    {
        if (_userProfile.sentimentLogs.Count >= 3)
        {
            // Take the last three sentiment logs
            var lastThreeLogs = _userProfile.sentimentLogs.Take(3).ToList();
            float averageScore = lastThreeLogs.Average(log => log.sentimentScore);
            // Check if the average score crosses the threshold and has shifted significantly
            if ((averageScore < -0.75 || averageScore > 0.75) && 
                Mathf.Abs(averageScore - _lastTriggeredAverageSentiment) > 0.75)
            {
                // Get the response tone from the most recent sentiment log
                string responseTone = lastThreeLogs[0].responseTone;
            
                // Generate a custom prompt
                List<SentimentLog> lastThreeLogsList = lastThreeLogs;
                string customPrompt = _aiModulePromtEngineering.GenerateCustomSentimentPrompt(lastThreeLogsList);
            
                // Send the custom prompt with the response tone to ChatGPT
                _aiModuleChatgpt.AiMessage(customPrompt, responseTone);

                // Update the last triggered average sentiment
                _lastTriggeredAverageSentiment = averageScore;
            }
        }
    }

    public void EnableAIModuleAwareness()
    {
        this.enabled = true;
    }

    public void DisableAiModuleAwareness()
    {
        this.enabled = false;
    }
    
    public void LoadData(UserProfile data, APIKeys apiKeys)
    {
        _userProfile = data;
    }

    public void SaveData(ref UserProfile data, APIKeys apiKeys)
    {
        // Implement save logic here
    }
}
