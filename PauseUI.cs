using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Cysharp.Threading.Tasks;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private ButtonToggleComponent sound;
    [SerializeField] private ButtonToggleComponent music;
    [SerializeField] private ButtonToggleComponent vibration;
    [SerializeField] private Button back;
    [SerializeField] private Button otherButton1;
    [SerializeField] private Button otherButton2;

    

    private void Awake()
    {
        sound.onClick.AddListener(SoundOnClick);
        music.onClick.AddListener(MusicOnClick);
        vibration.onClick.AddListener(VibrationOnClick);
        back.onClick.AddListener(BackOnClick);
        otherButton1.onClick.AddListener(otherButton1OnClick);
        otherButton2.onClick.AddListener(otherButton2OnClick);
    }

    public void OnEnable()
    {
        sound.SetToggle(DataProvider.SettingsConfigData.SFXEnabled);
        music.SetToggle(DataProvider.SettingsConfigData.MusicEnabled);
        vibration.SetToggle(DataProvider.SettingsConfigData.HapticsEnabled);

    }

    public void SoundOnClick()
    {
        DataProvider.SettingsConfigData.ToggleSFX(DataProvider.SettingsConfigData.SFXEnabled == false);
    }
    public void MusicOnClick()
    {
        DataProvider.SettingsConfigData.ToggleMusic(DataProvider.SettingsConfigData.MusicEnabled == false);
    }

    public void VibrationOnClick()
    {
        DataProvider.SettingsConfigData.ToggleHaptics(DataProvider.SettingsConfigData.HapticsEnabled == false);
    }

    public void BackOnClick()
    {
        if (ManagerProvider.StateManager.PreviousState is GameplayState)
        {
            ManagerProvider.StateManager.SetCurrentState(new GameplayState(false)).Forget();
        }
        else
        {
            ManagerProvider.StateManager.SetCurrentState(ManagerProvider.StateManager.PreviousState).Forget();
        }
        Debug.Log("[Back Button] clicked");
    }

    // Restore Purchases
    public void otherButton1OnClick()
    {
        Debug.Log("[other Button 1] clicked");
    }

    // Privacy Policy
    public void otherButton2OnClick()
    {
        Debug.Log("[other Button 2] clicked");
    }



}
