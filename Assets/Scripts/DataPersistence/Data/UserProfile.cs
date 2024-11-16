using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

[System.Serializable]
public class UserProfile
{
    public string name;
    public int age;
    public string profession;
    public string preferredTone;
    
    public List<string> pastTopics = new List<string>();

    public Dictionary<string, string> moodPatterns = new Dictionary<string, string>
    {
    };

    public Queue<SentimentLog> sentimentLogs = new Queue<SentimentLog>(10);

    public Dictionary<string, ImportantMemory> importantMemories = new Dictionary<string, ImportantMemory>();

    public UserProfile()
    {
        this.name = "User";
        this.preferredTone = "Helpful";
    }

    public void FillPastTopics()
    {
        pastTopics.Clear(); // Clear past topics before filling
        foreach (var kvp in importantMemories)
        {
            if (!pastTopics.Contains(kvp.Key))
            {
                pastTopics.Add(kvp.Key);
            }
        }
    }
    
    public void AddSentimentLog(SentimentLog log)
    {
        if (sentimentLogs.Count >= 10)
        {
            sentimentLogs.Dequeue();
        }
        sentimentLogs.Enqueue(log);
    }

    public void AddOrUpdateMemory(string topic, string contextualSentiment, string memoryContext, DateTime lastMentioned)
    {
        // Capitalize the first letter of the topic
        if (!string.IsNullOrEmpty(topic))
        {
            topic = char.ToUpper(topic[0]) + topic.Substring(1);
        }

        if (importantMemories.ContainsKey(topic))
        {
            importantMemories[topic].UpdateMemory(contextualSentiment, memoryContext, lastMentioned);
        }
        else
        {
            importantMemories[topic] = new ImportantMemory(topic, contextualSentiment, memoryContext, lastMentioned);
        }
        FillPastTopics();
    }

    /// <summary>
    /// Parses a Json Response
    /// Returns a list of relevant past topics from the Response
    /// </summary>
    /// <param name="jsonResponse"></param>
    /// <returns></returns>
    public List<string> ParseAnalysisResponse(string jsonResponse)
    {
        try
        {
            jsonResponse = jsonResponse.Replace("```", "").Trim();
            if (jsonResponse.StartsWith("json"))
            {
                jsonResponse = jsonResponse.Substring(jsonResponse.IndexOf('{'));
            }

            var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonResponse);
            List<string> relevantPastTopics = new List<string>();
            
            // Handle sentimentLogs as either JObject or JArray
            if (jsonObject["sentimentLogs"] is JObject singleSentimentLog)
            {
                var log = singleSentimentLog.ToObject<SentimentLog>();
                if (log != null)
                {
                    AddSentimentLog(log);
                }
            }
            else if (jsonObject["sentimentLogs"] is JArray sentimentArray)
            {
                foreach (var sentimentEntry in sentimentArray)
                {
                    if (sentimentEntry is JObject logEntry)
                    {
                        var log = logEntry.ToObject<SentimentLog>();
                        if (log != null)
                        {
                            AddSentimentLog(log);
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("No sentiment logs found in parsed response.");
            }

            // Handle importantMemories as either JObject or JArray
            if (jsonObject["importantMemories"] is JObject singleMemoryData)
            {
                var importantMemory = singleMemoryData.ToObject<ImportantMemory>();
                if (importantMemory != null)
                {
                    AddOrUpdateMemory(
                        importantMemory.topic,
                        importantMemory.contextualSentiment,
                        importantMemory.memoryContext,
                        importantMemory.lastMentioned
                    );
                }
            }
            else if (jsonObject["importantMemories"] is JArray memoriesArray)
            {
                foreach (var memoryEntry in memoriesArray)
                {
                    if (memoryEntry is JObject memoryData)
                    {
                        var importantMemory = memoryData.ToObject<ImportantMemory>();
                        if (importantMemory != null)
                        {
                            AddOrUpdateMemory(
                                importantMemory.topic,
                                importantMemory.contextualSentiment,
                                importantMemory.memoryContext,
                                importantMemory.lastMentioned
                            );
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("No important memories found in parsed response.");
            }

            // Handle pastTopics as JArray of objects
            if (jsonObject["pastTopics"] is JArray pastTopicsArray)
            {
                foreach (var topicEntry in pastTopicsArray)
                {
                    if (topicEntry is JObject topicData)
                    {
                        var topic = topicData["topic"]?.ToString();
                        if (topic != null && pastTopics.Contains(topic))
                        {
                            relevantPastTopics.Add(topic);
                        }
                    }
                }
            }
            
            /*
            if (relevantPastTopics.Count > 0)
            {
                foreach (var topic in relevantPastTopics)
                {
                    Debug.Log($"Relevant past topic found: {topic}");
                }
            }
            */
            
            return relevantPastTopics;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse analysis response: {e.Message}");
            return null;
        }
    }


}

[System.Serializable]
public class SentimentLog
{
    public DateTime timestamp;
    public string topic;
    public float sentimentScore;
    public string mood;
    public string notes;
    public string responseTone;

    public SentimentLog(DateTime timestamp, string topic, float sentimentScore, string mood, string notes, string responseTone)
    {
        this.timestamp = timestamp;
        this.topic = topic;
        this.sentimentScore = sentimentScore;
        this.mood = mood;
        this.notes = notes;
        this.responseTone = responseTone;
    }
}

[System.Serializable]
public class ImportantMemory
{
    public string topic;
    public string contextualSentiment;
    public string memoryContext;
    public DateTime lastMentioned;

    public ImportantMemory(string topic, string contextualSentiment, string memoryContext, DateTime lastMentioned)
    {
        this.topic = topic;
        this.contextualSentiment = contextualSentiment;
        this.memoryContext = memoryContext;
        this.lastMentioned = lastMentioned;
    }

    public void UpdateMemory(string contextualSentiment, string memoryContext, DateTime lastMentioned)
    {
        this.contextualSentiment = contextualSentiment;
        this.memoryContext = memoryContext;
        this.lastMentioned = lastMentioned;
    }
}
