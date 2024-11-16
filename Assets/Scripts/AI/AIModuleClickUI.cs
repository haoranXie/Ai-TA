using UnityEngine;

/// <summary>
/// Shows Input field and Buttons when right clicking
/// </summary>
public class AIModuleClickUI : AIModule
{
    private GameObject inputFieldObject;
    private GameObject settingsObject;
    private GameObject settingsWindow;
    private GameObject apiKeysWindow;
    private Camera mainCamera;
    private CapsuleCollider2D _collider;

    public override void ModuleUpdate() { }

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main; // Cache the main camera for raycasting
    }

    protected override void Start()
    {
        base.Start();
        inputFieldObject = _brain.inputFieldObject;
        settingsObject = _brain.buttonsObject;
        settingsWindow = _brain.settingsWindow;
        apiKeysWindow = _brain.apiKeysWindow;
        _collider = _brain.capsuleCollider; // Use the collider directly from the brain
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1)) // Check if the right mouse button is pressed
        {
            DetectRightClick();
        }
    }

    private void DetectRightClick()
    {
        if (mainCamera == null || _collider == null) return; // Exit if no camera or collider is assigned

        // Convert mouse position to world position
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Check if the right-click position is within the character's collider bounds
        if (_collider.OverlapPoint(mousePosition))
        {
            ToggleInputField();
        }
    }

    // Function to toggle the input field on right-click
    private void ToggleInputField()
    {
        if (inputFieldObject != null)
        {
            // Toggle the active state of the input field
            inputFieldObject.SetActive(!inputFieldObject.activeSelf);
        }

        if (settingsObject != null)
        {
            settingsObject.SetActive(!settingsObject.activeSelf);
        }
    }

    public void ToggleSettingsMenu()
    {
        if (settingsWindow != null)
        {
            settingsWindow.SetActive(!settingsWindow.activeSelf);
        }
    }
    
    public void ToggleAPIKeysMenu()
    {
        if (apiKeysWindow != null)
        {
            apiKeysWindow.SetActive(!apiKeysWindow.activeSelf);
        }
    }
}