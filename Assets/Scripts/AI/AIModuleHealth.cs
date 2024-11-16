using System;
using UnityEngine;


public class AIModuleHealth : AIModule, IDataPersistence
{
    private UserProfile _userProfile;
    private AIModuleChatgpt _aiModuleChatgpt;
    private AIModulePromtEngineering _aiModulePromtEngineering;
    private DateTime t0 = DateTime.Now;

    private string CalculateSpan()
    {
        var t1 = DateTime.Now;
        TimeSpan span = t1.Subtract(t0);
        return $"{span.Hours} : {span.Minutes} : {span.Seconds}";
    }

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
            FetchHealthData();
        }
    }


    private void FetchHealthData()
    {
        var timeSpentWithAgent = CalculateSpan();
        // {timeSpentWithAgent}

        var pmt = $"The following time is given in Hour:Minute:Second format: 3:0:0. This is how long the user has spent time with you (or spent time on their screens). Please note that more than one hour of screen time is bad. You can also use the amount of time (in numbers or in words) they spent on their computer.";
        pmt += "Please make approperiate comments utilziing the weather, location, time, and interest infomation. Make sure to rememebr that your top priority is the users mental and physical well-being";
        
        //Debug.Log(pmt);
        _aiModuleChatgpt.AiMessage(pmt, "Enthusiastic");

        flag = false;
    }

    public void EnableAIModuleHealth()
    {
        this.enabled = true;
        flag = true;
    }

    public void DisableAiModuleHealth()
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
