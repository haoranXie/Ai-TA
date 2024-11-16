using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AiModuleNews : AIModule, IDataPersistence
{
    private UserProfile _userProfile;
    private AIModuleChatgpt _aiModuleChatgpt;
    private AIModulePromtEngineering _aiModulePromtEngineering;

    private bool flag = true;


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
        if (flag)
        {
            FetchNewsDatalol();
        }
    }

    private void FetchNewsDatalol()
    {   
        if (_userProfile.pastTopics.Count > 0) // Ensure there's at least one topic
        {
            int randomIndex = UnityEngine.Random.Range(0, _userProfile.pastTopics.Count);
            string randomTopic = _userProfile.pastTopics[randomIndex];

            StartCoroutine(_aiModulePromtEngineering.PromtGetNewsInfo(randomTopic, "20", result =>
            {
                //Debug.Log(result);
                _aiModuleChatgpt.AiMessage(result, "Enthusiastic");
            }));
        }

        // var topicInIntrest = _userProfile.sentimentLogs.topic
        // how do we get the intereted topic
        flag = false;
    }

    public void EnableAIModuleNews()
    {
        this.enabled = true;
        flag = true;
    }

    public void DisableAiModuleNews()
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
