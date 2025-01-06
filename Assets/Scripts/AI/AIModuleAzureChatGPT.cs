using UnityEngine;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public class AIModuleAzureChatGPT : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async Task StartAsync()
    {
        var endpoint = new Uri("https://synthoria.openai.azure.com/");
        var credentials = new AzureKeyCredential("BPKwZSwBIyTnvQLq41kbyIWxcPfBz071J1RJ6skVPqBkQBVwmS7sJQQJ99BAACYeBjFXJ3w3AAABACOGusBw");
        var deploymentName = "gpt4o"; // Default name, update with your own if needed


        var openAIClient = new AzureOpenAIClient(endpoint, credentials);
        var chatClient = openAIClient.GetChatClient(deploymentName);
        
        // var imageUri = "YOUR_IMAGE_URL";

        List<ChatMessage> messages = new List<ChatMessage>
        {
            new UserChatMessage(
            ChatMessageContentPart.CreateTextPart("Hello! Nice to meet you :)"))
        };

        ChatCompletion chatCompletion = await chatClient.CompleteChatAsync(messages);
        Debug.Log($"[ASSISTANT]:");
        Debug.Log($"{chatCompletion.Content[0].Text}");
    }

    async void Start()
    {
        await StartAsync();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
