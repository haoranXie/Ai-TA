using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

public class FileDataHandler
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private bool useEncryption = false;
    private readonly string encryptionCodeWord = "Tractatus";

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    public T Load<T>() where T : class
    {
        string fullPath = Path.Combine(this.dataDirPath, this.dataFileName);
        T loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad;
                using (FileStream fileStream = new FileStream(fullPath, FileMode.Open))
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    dataToLoad = reader.ReadToEnd();
                }

                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                loadedData = JsonConvert.DeserializeObject<T>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading data: {e.Message}");
            }
        }
        return loadedData;
    }

    public void Save<T>(T data, String customName = "") where T : class
    {
        string fullPath = Path.Combine(this.dataDirPath, this.dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataToStore = JsonConvert.SerializeObject(data);

            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            using (FileStream fs = new FileStream(fullPath, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(dataToStore);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving data: {e.Message}");
        }
    }

    public UserProfile LoadFromTextAsset(TextAsset jsonFile)
    {
        UserProfile loadedData = null;
        if (jsonFile != null)
        {
            try
            {
                string dataToLoad = jsonFile.text;
                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }
                loadedData = JsonConvert.DeserializeObject<UserProfile>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading UserProfile from TextAsset: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("No TextAsset provided for loading UserProfile.");
        }
        return loadedData;
    }

    private string EncryptDecrypt(string data)
    {
        string modifiedData = "";
        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }
        return modifiedData;
    }
    
    public void SaveAPIKeys(APIKeys apiKeys, string fileName)
    {
        string fullPath = Path.Combine(this.dataDirPath, fileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataToStore = JsonConvert.SerializeObject(apiKeys);
            
            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            using (FileStream fs = new FileStream(fullPath, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.Write(dataToStore);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving data: {e.Message}");
        }
    }

    public APIKeys LoadAPIKeys(string fileName)
    {
        string fullPath = Path.Combine(dataDirPath, fileName);
        if (File.Exists(fullPath))
        {
            string dataToLoad = File.ReadAllText(fullPath);
            return JsonConvert.DeserializeObject<APIKeys>(dataToLoad);
        }
        return new APIKeys();
    }

}
