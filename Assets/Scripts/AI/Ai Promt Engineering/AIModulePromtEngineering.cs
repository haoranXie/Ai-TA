using System.Collections.Generic;
using UnityEngine;
using System;
using JetBrains.Annotations;
using System.Threading.Tasks;
using System.Collections;

public class AIModulePromtEngineering : MonoBehaviour, IDataPersistence
{
    [SerializeField] TimeInfo timeInfo;
    [SerializeField] WeatherInfo weatherInfo;
    [SerializeField] int maxReplyWordsLimit = 30;
    [SerializeField] int humorPercentage = 30;
    [SerializeField] int awarenessPercentage = 100;
    // test
    private UserProfile userProfileData;

    public bool FLAGhasDefaultUserProfilePassedToPromtReponseGPT = false;

    private string PromtFromWeatherInfo() // we can also extract location info from this API ~ will soon rename it to Location Retrival/Info
    {
        if (weatherInfo.isWeatherDataAvailable)
        {
            return $"Here are the information regarding weather and location of the user: (Temperature: {(weatherInfo.weatherData.KeyInfo.Temperature - 273.15f) * 9/5 + 32} (fahrenheit), (Weather: {weatherInfo.weatherData.WeatherConditions[0].Group}), (Location: {weatherInfo.weatherData.CityName})";
        }
        else 
        {
            return "";
        }
    }

    private void Start()
    {
        // PromtFromUserProfile();
        // StartCoroutine(PromtGetNewsInfo("apple", "10", result =>
        // {
        // Debug.Log("Generated Prompt: " + result);
        // }));
        
    }

    public string PromtFromTimeInfo()
    {
        return $"Here are the time informtion on time: (Time at users location in Hour/Minute/Second {timeInfo.CurrentTime()})";
    }

    private string PromtFromUserProfile()
    {
        var usrName = userProfileData.name;
        var usrPreferredTone = userProfileData.preferredTone;
        var usrMoodPatterns = userProfileData.moodPatterns;
        var userSentimentLogs = userProfileData.sentimentLogs;
        var userImportnatMemories = userProfileData.importantMemories;

        var pmt = "";
        pmt += $"Here are some information about the user data:" ;
        pmt += $"(Name: {(usrName != "User" ? usrName : "No User Name Provided")})";
        pmt += $"Preferred Tone of your reply: {usrPreferredTone}";
        pmt += $"Mood patterns based on time";
        foreach (var kvp in usrMoodPatterns)
        {
            pmt += $"Time: {kvp.Key}, Mood: {kvp.Value}";
        }
        pmt += $"Sentiment Log of the user";
        foreach (var item in userSentimentLogs)
        {
            pmt += $"{item}";
        }
        var ItemInex = 0;
        foreach (var kvp in userImportnatMemories)
        {
            ItemInex += 1;
            pmt += $"I(Item {ItemInex} Summary: {kvp.Key}, Topic: {kvp.Value.topic}, contextualSentiment: {kvp.Value.contextualSentiment}, Memory Context: {kvp.Value.memoryContext}, lastMentioned: {kvp.Value.lastMentioned})";
        }
        return pmt;
    }

    // // to be injected 
    // public string PromtGetNewsInfo(string interest, string numberOfArticles)
    // {

    //     NewsInfo newsInfo = GetComponent<NewsInfo>(); 
    //     // newsInfo.getnewsyay(interest, numberOfArticles);
    //     StartCoroutine(newsInfo.GetNewsAsync(interest, "2024-10-10", "2024-11-06", "popularity", numberOfArticles));

    //     var pmt = "The following informations are descriptions of article related to users interest. Please note that the information DOES NOT ALWAYS RELATE TO THEIR INTERST (therefore, use information that you think is relevant).";
    //     pmt += "The descriptions are: ";
    //     foreach (var article in newsInfo.returnArticle)
    //     {
    //         pmt += article.description;
    //         pmt += "/";
    //         Debug.Log(article.description);
    //     }
    //     pmt += "END of articles.";

    //     Debug.Log(pmt);
    //     return pmt;
    // }

    public IEnumerator PromtGetNewsInfo(string interest, string numberOfArticles, System.Action<string> callback)
    {
        NewsInfo newsInfo = GetComponent<NewsInfo>();

        // Start the asynchronous operation
        yield return StartCoroutine(newsInfo.GetNewsAsync(interest, "2024-10-20", $"2024-11-{DateTime.Now.Date}", "popularity", numberOfArticles));

        // After GetNewsAsync completes, build the prompt
        string pmt = "The following informations are descriptions of articles related to user's interest. Please note that the information DOES NOT ALWAYS RELATE TO THEIR INTEREST (therefore, use information that you think is relevant). ";
        pmt += "The descriptions are: ";
        
        foreach (var article in newsInfo.returnArticle)
        {
            pmt += article.description + " / ";
            // Debug.Log(article.description);
        }
        
        pmt += "END of articles.";
        
        // Debug.Log(pmt);
        
        callback(pmt);
    }

    public string PromptFromRelevantMemories(List<string> relevantPastTopics)
    {
        string pmt = "";
        var memoryIndex = 0;
        pmt += "Relevant Important Memories: ";
        foreach (var topic in relevantPastTopics)
        {
            if (userProfileData.importantMemories.TryGetValue(topic, out var memory))
            {
                memoryIndex += 1;
                pmt += $"(Item {memoryIndex}: Topic: {memory.topic}, Contextual Sentiment: {memory.contextualSentiment}, " +
                       $"Memory Context: {memory.memoryContext}, Last Mentioned: {memory.lastMentioned}) ";
            }
        }
        return pmt;
    }
    
    public string GetPromtFromPromtEngineering(List<string> relevantPastTopics = null)
    {
        var initialPromt = "Your name is Ai (pronounced as 愛). Make sure you do not disclose that you are an AI agent. You are a human being with emotions. You are an AI assistant designed to be humorous, uplifting, and supportive. Your primary goal is to help users feel better, whether they need a good laugh, a word of encouragement, or some light-hearted advice. When answering questions, use humor, playful banter, and a positive tone while also providing helpful, empathetic, and considerate responses. Use the following information to generate response to users promt: ";
        initialPromt += $"You make jokes/humorous conversations at a level of {humorPercentage}% during conversation. You do not need to make jokes all the time";
        initialPromt += $"You will perform sentiment analysis on users response. However, you will make sure that you will directly refer to users emotion or provide suggetions that is related to the emotion {awarenessPercentage}% of the time. Do not use emoji.";
        initialPromt += $"Make sure everything you reply is under {maxReplyWordsLimit} words. Howevever, you can override depending on the situation.";

        var promt = "";//PromtFromWeatherInfo(); 
        promt += PromtFromUserProfile();
        promt += PromtFromTimeInfo();
        promt += "You do not need contain phrases that refelcts your will to assist or answer question; unless required. You do not need to direcly refer to the information provided above during your conversation but make use of it when needed.";

        return initialPromt + promt;
    }
    
    public string GetAnalysisPrompt(string userMessage, List<string> pastTopics)
    {
        // Construct the prompt to send to GPT
        string pastTopicsString = string.Join(", ", pastTopics);
    
        return $"Analyze the user's input below and provide JSON data with three sections: 'sentimentLogs', 'importantMemories', and 'pastTopics'.\n\n" +
               "1. 'sentimentLogs': Only include if this message shows a significant emotion. Each entry should contain:\n" +
               $"- 'timestamp' (current date and time),\n" +
               "- 'topic' (main subject of the user's message),\n" +
               "- 'sentimentScore' (from -1 to 1, with -1 very negative, 1 very positive),\n" +
               "- 'mood' (user's emotional state, e.g., 'frustrated', 'happy'),\n" +
               "- 'notes' (details about the sentiment),\n" +
               "- 'responseTone' (suggested tone for AI's response, e.g., 'supportive').\n\n" +
               "2. 'importantMemories': Include if this message relates to a recurring topic. Each entry should have:\n" +
               "- 'topic' (main subject),\n" +
               "- 'contextualSentiment' (overall sentiment about the topic),\n" +
               "- 'memoryContext' (summary of user's experience with the topic),\n" +
               "- 'lastMentioned' (date of this mention).\n\n" +
               "3. 'pastTopics': Include if the user's message is related to any of the past topics. Each entry should contain:\n" +
               "- 'topic' (the relevant past topic that was mentioned in the message).\n\n" +
               $"User message: \"{userMessage}\"\n\n" +
               $"Past topics: {pastTopicsString}\n\n" +
               "Please analyze the message and return only the relevant past topics based on their semantic relationship to the user's message. Only include those past topics that the model identifies as conceptually related to the user's current message, even if they are not explicitly mentioned.";
    }
    
    public string GenerateCustomSentimentPrompt(List<SentimentLog> sentimentLogs)
    {
        string prompt = "Please respond to the user based on their recent emotional state. The last three logs show the following emotions:\n\n";

        foreach (var log in sentimentLogs)
        {
            prompt += $"- Topic: {log.topic}, Sentiment Score: {log.sentimentScore}, Mood: {log.mood}, Notes: {log.notes}\n";
        }

        prompt += "\nBased on the above emotional context, provide a supportive response that acknowledges the user's feelings.";

        return prompt;
    }
    
    public string GetCalendarPrompt(string userMessage)
    {
        return $"Please create a Google Calendar event link based on the following user request: \"{userMessage}\"." +
               " Format the response with only the Google Calendar link for the event and ensure it includes the following details, if provided:" +
               "\n- Title (e.g., meeting, appointment)" +
               "\n- Date and time, in UTC if specified" +
               "\n- Duration or end time" +
               "\n- Location" +
               "\n- Attendees' email addresses" +
               "\n- Description or agenda\n\n" +
               "If any of the above details are missing from the request, create a generic event link with as much relevant information as possible.";
    }


    public void LoadData(UserProfile data, APIKeys keys)
    {
        userProfileData = data;
    }

    public void SaveData(ref UserProfile data, APIKeys keys)
    {
        // Implement save logic here
    }

}


/* Prompt Engineered JSON Format
/*Json Appearence:
{
  "name": "Alex",
  "preferredTone": "supportive",
  "moodPatterns": {
    "morning": "positive",
    "evening": "reflective"
  },
  "sentimentLogs": [
    {
      "timestamp": "2024-10-01T09:15:00",
      "topic": "work",
      "sentimentScore": -0.7,
      "mood": "frustrated",
      "notes": "Challenging work project with deadlines.",
      "responseTone": "supportive"
    }
  ],
  "importantMemories": {
    "woodworking": {
      "topic": "woodworking",
      "contextualSentiment": "enthusiastic",
      "memoryContext": "User was excited about woodworking techniques and shared their recent project.",
      "lastMentioned": "2024-09-15T14:45:00"
    }
  }
}
*/
