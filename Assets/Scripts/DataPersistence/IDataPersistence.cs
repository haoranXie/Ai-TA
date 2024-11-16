using UnityEngine;

public interface IDataPersistence
{
    void LoadData(UserProfile data, APIKeys keys = null);

    void SaveData(ref UserProfile data, APIKeys keys = null);
    
}
