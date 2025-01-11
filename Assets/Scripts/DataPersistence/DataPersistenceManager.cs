using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Configuration")] 
    [SerializeField] private string fileName;
    [SerializeField] private string apiKeysFileName; // Added for APIKeys

    [SerializeField] private bool useEncryption;

    [Header("Optional Profiles")] 
    [SerializeField] private TextAsset[] files;

    private UserProfile _gameData;
    public APIKeys _apiKeys;
    private List<IDataPersistence> _dataPersistenceObjects;
    private FileDataHandler _fileDataHandler;
    private FileDataHandler _apiKeysDataHandler; // Separate handler for APIKeys

    public static DataPersistenceManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
        }
        instance = this;

        string folderName = "AiTutor";
        string securePath;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        securePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), folderName);
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        securePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), folderName);
#else
        securePath = Path.Combine(Application.persistentDataPath, folderName); // Fallback for other platforms
#endif

        _fileDataHandler = new FileDataHandler(securePath, fileName, useEncryption);
        _apiKeysDataHandler = new FileDataHandler(securePath, apiKeysFileName, useEncryption); // Initialize for APIKeys

        _dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void NewGame()
    {
        _gameData = new UserProfile();
        _apiKeys = new APIKeys(); // Initialize APIKeys with defaults or empty values
    }

    public void LoadGame()
    {
        _gameData = _fileDataHandler.Load<UserProfile>();
        if (_gameData == null)
        {
            Debug.Log("No user profile data was found. Initializing to defaults.");
            NewGame();
        }

        _apiKeys = _apiKeysDataHandler.LoadAPIKeys(apiKeysFileName);
        if (_apiKeys == null)
        {
            Debug.Log("No API keys data was found. Initializing to defaults.");
            _apiKeys = new APIKeys();
        }

        foreach (IDataPersistence dataPersistenceObj in _dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(_gameData, _apiKeys);
        }
    }

    public void SaveGame()
    {
        foreach (IDataPersistence dataPersistenceObj in _dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref _gameData);
        }
        
        _fileDataHandler.Save(_gameData);
        _apiKeysDataHandler.SaveAPIKeys(_apiKeys, apiKeysFileName); // Save APIKeys separately
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {  
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    /// <summary>
    /// Loads the profile data from a dropdown relative to the serialized files
    /// </summary>
    public void HandleProfileInput(int val)
    {
        if (val == 0)
        {
            LoadGame();
            return;
        }

        UserProfile gameData = _fileDataHandler.LoadFromTextAsset(files[val-1]);
        if (gameData == null) return;
        foreach (IDataPersistence dataPersistenceObj in _dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(gameData);
        }
    }
    
    public void SaveAPIKeys(APIKeys apiKeys)
    {
        _fileDataHandler.SaveAPIKeys(apiKeys, apiKeysFileName);
    }

    public APIKeys LoadAPIKeys()
    {
        return _fileDataHandler.LoadAPIKeys(apiKeysFileName);
    }

}
