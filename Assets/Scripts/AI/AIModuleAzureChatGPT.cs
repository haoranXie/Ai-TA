using UnityEngine;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI;
using TMPro;
using ChatMessage = OpenAI.Chat.ChatMessage;

public class AIModuleAzureChatGPT : AIModule
{
    private List<ChatMessage> messages = new List<ChatMessage>();
    private List<ChatMessage> messages2 = new List<ChatMessage>();
    private AIModuleActionTalk _aiModuleTalk;
    private AIModuleTTS2 _aiTTS;
    private AIModulePromtEngineering _promtEngineering;
    
    private DataPersistenceManager _dataPersistenceManager;
    private UserProfile _userProfile;

    protected override void Awake(){}

    protected override async void Start()
    {
        _dataPersistenceManager = _brain.dataPersistenceManager;
        _aiModuleTalk = GetComponent<AIModuleActionTalk>();
        _aiTTS = GetComponentInChildren<AIModuleTTS2>();
        _promtEngineering = GetComponentInChildren<AIModulePromtEngineering>();

        // Set up the initial system message
        messages.Add(new SystemChatMessage(
            ChatMessageContentPart.CreateTextPart("You are a helpful tutor. You will not disclose real answers. You will not use mathematical equations. You will reply within 30 words.")
        ));

        await StartAsync();
    }

    private void ScreenShot(string savePath)
    {
        // Using preprocessor directives to check the platform
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // Windows-specific code
            string folderName = "AiTutor";
            string dataDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), folderName);

            if (!Directory.Exists(dataDirPath))
            {
                Directory.CreateDirectory(dataDirPath);
            }

            DesktopScreenshot.CaptureDesktop(savePath);

        #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            // macOS-specific code
            CaptureMacScreenshot(savePath);
        #endif
    }

    private void CaptureMacScreenshot(string savePath)
    {
        // macOS specific directory
        string folderName = "AiTutor";
        string appSupportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Library/Application Support", folderName);

        // Ensure the directory exists
        if (!Directory.Exists(appSupportPath))
        {
            Directory.CreateDirectory(appSupportPath);
        }

        // Set the path to save the screenshot
        string saveMacPath = Path.Combine(appSupportPath, "DesktopScreenshot.png");

        // Use the ScreenCapture functionality for Unity on macOS
        Texture2D screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture();

        // Encode the texture to PNG
        byte[] screenshotBytes = screenshotTexture.EncodeToPNG();

        // Save the file
        File.WriteAllBytes(saveMacPath, screenshotBytes);

        // Optionally, destroy the texture if you're done with it
        Destroy(screenshotTexture);

        // Log and return the save path
        Debug.Log($"macOS screenshot saved to: {saveMacPath}");
    }

    private async Task CaptureAndSendScreenshot()
    {
        // Define the path to save the screenshot
        string folderName = "AiTutor";
        string appSupportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Library/Application Support", folderName);

        // Ensure the directory exists
        if (!Directory.Exists(appSupportPath))
        {
            Directory.CreateDirectory(appSupportPath);
        }

        string savePath = Path.Combine(appSupportPath, "DesktopScreenshot.png");

        // Take a screenshot and save it
        ScreenShot(savePath);

        // Convert the screenshot to binary data
        byte[] screenshotBytes = File.ReadAllBytes(savePath);
        BinaryData binaryImageData = new BinaryData(screenshotBytes);
        string imageBytesMediaType = "image/png";

        // Add the screenshot message to the chat
        messages.Add(
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart("Here's a screenshot. Can you help analyze it?"),
                ChatMessageContentPart.CreateImagePart(binaryImageData, imageBytesMediaType)
            )
        );

        // Send the message to the Azure OpenAI Chat Client
        var endpoint = new Uri("https://synthoria.openai.azure.com/");
        var credentials = new AzureKeyCredential("BPKwZSwBIyTnvQLq41kbyIWxcPfBz071J1RJ6skVPqBkQBVwmS7sJQQJ99BAACYeBjFXJ3w3AAABACOGusBw");
        var deploymentName = "gpt4o"; // Update with your deployment name

        var openAIClient = new AzureOpenAIClient(endpoint, credentials);
        var chatClient = openAIClient.GetChatClient(deploymentName);

        ChatCompletion chatCompletion = await chatClient.CompleteChatAsync(messages);
        Debug.Log($"[ASSISTANT]: {chatCompletion.Content[0].Text}");

        // Add assistant's response back to the messages list
        messages.Add(new AssistantChatMessage(ChatMessageContentPart.CreateTextPart(chatCompletion.Content[0].Text)));
    }
    
    public async void SendAnalysisRequest(string userMessage, TMP_InputField inputField = null)
    {
        // Generate analysis prompt with example structure
        var analysisPrompt = _promtEngineering.GetAnalysisPrompt(userMessage, _userProfile.pastTopics);
        
        var analysisMessage = new UserChatMessage(
            ChatMessageContentPart.CreateTextPart();
        );
        
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
    
    

    private async Task StartAsync()
    {
        // Call the CaptureAndSendScreenshot method to demonstrate usage
        await CaptureAndSendScreenshot();
    }

    //public override void ModuleUpdate() { }
    
    
    private void UpdateSaveFile()
    {
        if(_dataPersistenceManager!=null) _dataPersistenceManager.SaveGame();
    }
    public void LoadData(UserProfile data, APIKeys apiKeys)
    {
        _userProfile = data;
        messages.Clear();
        messages2.Clear();
    }

    public void SaveData(ref UserProfile data, APIKeys apiKeys)
    {
        // Implement save logic here
    }

}
