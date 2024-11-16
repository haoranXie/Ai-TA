using System;
using OpenAI; 
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

public class AIModuleChatgpt : AIModule, IDataPersistence
{
    private List<ChatMessage> messages = new List<ChatMessage>();
    private List<ChatMessage> messages2 = new List<ChatMessage>();
    private List<ChatMessage> messages3 = new List<ChatMessage>();

    private OpenAIApi openai;
    private AIModuleActionTalk _aiModuleTalk;
    private AIModuleTTS2 _aiTTS;
    // private AIModuleTTS _aiTTS;
    private AIModulePromtEngineering _promtEngineering;
    private UserProfile _userProfile;
    private DataPersistenceManager _dataPersistenceManager;

    private bool _supportLanguages = false;

    public override void ModuleUpdate()
    {
        base.ModuleUpdate();
    }


// Representing the full analysis response structure
    public class AnalysisResponse
    {
        public List<SentimentLog> sentimentLogs { get; set; }
        public Dictionary<string, ImportantMemory> importantMemories { get; set; }
    }

    
    protected override void Start()
    {
        base.Start();
        openai = _brain.openAIAPI;
        _aiModuleTalk = GetComponent<AIModuleActionTalk>();
        _aiTTS = GetComponentInChildren<AIModuleTTS2>();
        // _aiTTS = GetComponentInChildren<AIModuleTTS>();
        _promtEngineering = GetComponentInChildren<AIModulePromtEngineering>();
        _dataPersistenceManager = _brain.dataPersistenceManager;
    }
    
    /// <summary>
    /// Message sent by Ai by thesmelves
    /// </summary>
    /// <param name="reply"></param>
    /// <param name="inputField"></param>
    public async void AiMessage(string prompt, string responseTone = "Supportive")
    {
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = prompt
        };
        
        if (messages.Count == 0) newMessage.Content = _promtEngineering.GetPromtFromPromtEngineering() + "\n" + prompt;
        
        messages.Add(newMessage);
        
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = messages,
        });
        
        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();
                
            messages.Add(message);
            if(_aiModuleTalk!=null) _aiModuleTalk.Talk(message.Content, responseTone);
            StartCoroutine(_aiTTS.CallTTS(message.Content));
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }
    }
    
    public async void SendAnalysisRequest(string userMessage, TMP_InputField inputField = null)
    {
        // Generate analysis prompt with example structure
        var analysisPrompt = _promtEngineering.GetAnalysisPrompt(userMessage, _userProfile.pastTopics);

        var analysisMessage = new ChatMessage
        {
            Role = "user",
            Content = analysisPrompt
        };

        messages2.Add(analysisMessage);

        if (inputField != null)
        {
            inputField.text = "";
            inputField.enabled = false;
        }
        
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest
        {
            Model = "gpt-4o-mini",
            Messages = messages2
        });

        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            SendReply(userMessage, inputField, _userProfile.ParseAnalysisResponse(message.Content));
            // Parse and apply the JSON response to     user profile
            //ParseAnalysisResponse(message.Content);
        }
        else
        {
            Debug.LogWarning("No analysis data was generated from this prompt.");
        }
        
        messages2.Clear();
        if(inputField!=null) inputField.enabled = true;
    }
    

    /// <summary>
    /// Reply is the text sent to GPT. Optionally it can use inputField
    /// </summary>
    /// <param name="reply"></param>
    /// <param name="inputField"></param>
    public async void SendReply(string reply, TMP_InputField inputField = null, List<String> relevantPastTopics = null)
    {
        UpdateSaveFile();
        var newMessage = new ChatMessage()
        {
            Role = "user",
            Content = reply
        };
        
        // for each relevant memory, fetch the topic, contextualSentiment, memoryContext and feed that to 
        if (messages.Count == 0)
        {
            newMessage.Content = _promtEngineering.GetPromtFromPromtEngineering();
            if (_supportLanguages)
            {
                newMessage.Content += "Speak the language the user speaks in";
            }
            newMessage.Content += "\n" + reply;
        }
        else
        {
            newMessage.Content = _promtEngineering.PromtFromTimeInfo() +
                                 _promtEngineering.PromptFromRelevantMemories(relevantPastTopics);
            if (_supportLanguages)
            {
                newMessage.Content += "Speak the language the user speaks in";
            }
            newMessage.Content += "\n" + reply;
        }
        messages.Add(newMessage);

        if (inputField != null)
        {
            inputField.text = "";
            inputField.enabled = false;
        }
        
        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-4o-mini",
            Messages = messages,
        });
        
        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            var message = completionResponse.Choices[0].Message;
            message.Content = message.Content.Trim();
                
            messages.Add(message);
            if(_aiModuleTalk!=null) _aiModuleTalk.Talk(message.Content);
            StartCoroutine(_aiTTS.CallTTS(message.Content));
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
        }
        
        if(inputField!=null) inputField.enabled = true;
    }
    
    
    
    public async void HandleCalendarRequest(string userMessage, TMP_InputField inputField = null)
    {
        // Define keywords associated with Google Calendar actions
        var calendarKeywords = new List<string> { "schedule", "calendar", "meeting", "event", "appointment" };

        // Check if the message contains any of the keywords
        if (calendarKeywords.Any(keyword => userMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            // Generate a specific prompt for creating a Google Calendar link
            var calendarPrompt = _promtEngineering.GetCalendarPrompt(userMessage);
            
            var calendarMessage = new ChatMessage
            {
                Role = "user",
                Content = calendarPrompt
            };

            messages3.Add(calendarMessage);

            if (inputField != null)
            {
                inputField.text = "";
                inputField.enabled = false;
            }
            
            // Send the prompt to OpenAI API
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest
            {
                Model = "gpt-4o-mini",
                Messages = messages3
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                string responseContent = message.Content;

                // Extract Google Calendar link from the response
                var calendarLink = ExtractGoogleCalendarLink(responseContent);

                if (!string.IsNullOrEmpty(calendarLink))
                {
                    // Open the Google Calendar link in the default browser
                    Application.OpenURL(calendarLink);
                }
                else
                {
                    Debug.LogWarning("Could not find a Google Calendar link in the response.");
                }
            }
            else
            {
                Debug.LogWarning("No response received from OpenAI for the calendar request.");
            }

            // Clear messages2 list
            messages3.Clear();

            if (inputField != null) inputField.enabled = true;
        }
        else
        {
            SendAnalysisRequest(userMessage, inputField);
            //Debug.Log("No calendar-related keywords found in the message.");
        }
    }

    // Helper function to extract a Google Calendar link from the AI response
    private string ExtractGoogleCalendarLink(string content)
    {
        // Simple link extraction logic based on known URL patterns
        int linkStartIndex = content.IndexOf("https://calendar.google.com");
        
        if (linkStartIndex != -1)
        {
            int linkEndIndex = content.IndexOf(" ", linkStartIndex);
            if (linkEndIndex == -1) linkEndIndex = content.Length;
            return content.Substring(linkStartIndex, linkEndIndex - linkStartIndex);
        }
        
        return null;
    }

    private void UpdateSaveFile()
    {
        if(_dataPersistenceManager!=null) _dataPersistenceManager.SaveGame();
    }
    public void LoadData(UserProfile data, APIKeys apiKeys)
    {
        _userProfile = data;
        messages.Clear();
        messages2.Clear();
        messages3.Clear();
    }

    public void SaveData(ref UserProfile data, APIKeys apiKeys)
    {
        // Implement save logic here
    }

    public void SupportLanguages()
    {
        _supportLanguages = !_supportLanguages;
    }
}


