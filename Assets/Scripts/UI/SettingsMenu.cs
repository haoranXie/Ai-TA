using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public void resetAI()
    {
        DataPersistenceManager.instance.NewGame();
    }
    
}
