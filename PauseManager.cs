using Cysharp.Threading.Tasks;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public bool GameIsPaused = false;

    public UniTask StartManager()
    {
        return UniTask.CompletedTask;
    }

    public UniTask StopManager()
    {
        return UniTask.CompletedTask;
    }

    public void Resume()
    {
        Time.timeScale = 1.0f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
}