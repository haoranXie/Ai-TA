using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SliderAudioControl : MonoBehaviour
{
    public Slider volumeSlider;  // Drag your slider UI component here in the inspector
    private AudioSource audioSource;

    void Start()
    {
        // Get the AudioSource component attached to the same GameObject
        audioSource = GetComponent<AudioSource>();

        // Set initial volume from the slider's value
        if (volumeSlider != null)
            audioSource.volume = volumeSlider.value;

        // Add listener to the slider to update volume when it changes
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(UpdateVolume);
    }

    // Method to update the AudioSource volume
    private void UpdateVolume(float value)
    {
        audioSource.volume = value;
    }

    // Remove listener to avoid memory leaks
    void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(UpdateVolume);
    }
}