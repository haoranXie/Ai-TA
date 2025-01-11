using UnityEngine;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AIModuleAzureChatGPT : MonoBehaviour
{
    private List<ChatMessage> messages = new List<ChatMessage>();

    async void Start()
    {
        // Set up the initial system message
        messages.Add(new SystemChatMessage(
            ChatMessageContentPart.CreateTextPart("You are a helpful tutor. You will not disclose real answers. You will not use mathematical equations. You will reply within 30 words.")
        ));

        await StartAsync();
    }

    private void ScreenShot(string savePath)
    {
        string folderName = "AiTutor";
        string dataDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), folderName);

        if (!Directory.Exists(dataDirPath))
        {
            Directory.CreateDirectory(dataDirPath);
        }

        DesktopScreenshot.CaptureDesktop(savePath);
    }

    private async Task CaptureAndSendScreenshot()
    {
        // Take a screenshot and save it temporarily
        string folderName = "AiTutor";
        string dataDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), folderName);

        if (!Directory.Exists(dataDirPath))
        {
            Directory.CreateDirectory(dataDirPath);
        }

        string savePath = Path.Combine(dataDirPath, "DesktopScreenshot.png");
        DesktopScreenshot.CaptureDesktop(savePath);

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

    private async Task StartAsync()
    {
        // Call the CaptureAndSendScreenshot method to demonstrate usage
        await CaptureAndSendScreenshot();
    }

    void Update()
    {
        // Placeholder for Update functionality
    }
}
