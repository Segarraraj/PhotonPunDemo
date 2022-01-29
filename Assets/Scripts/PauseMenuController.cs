using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public bool Pause()
    {
        bool active = gameObject.activeInHierarchy;
        gameObject.SetActive(!active);
        return !active;
    }

    public void ResumeButton()
    {
        GameManager.instance.ShowPause();
    }

    public void BackToMenu()
    {
        GameManager.instance.LeaveRoom();
    }

    public void Exit()
    {
        Application.Quit();
    }
}
