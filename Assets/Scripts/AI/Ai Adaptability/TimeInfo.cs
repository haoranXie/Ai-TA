using UnityEngine;
using System;

public class TimeInfo : AIModule
{
    private DateTime t0 = DateTime.Now;

    public override void ModuleUpdate() { }

    protected override void Awake()
    {
        base.Awake();
    }
    protected override void Start()
    {
        base.Start();
        // Debug.Log(CurrentTime());
    }
    
    public string CalculateSpan()
    {
        var t1 = DateTime.Now;
        TimeSpan span = t1.Subtract(t0);
        return $"{span.Hours} : {span.Minutes} : {span.Seconds}";
    }

    public string CurrentTime()
    {
        var now = DateTime.Now;
        return $"{now.Hour} : {now.Minute} : {now.Second} and in year date month format {now.Year} : {now.Month} : {now.Date}"; 
    }
}