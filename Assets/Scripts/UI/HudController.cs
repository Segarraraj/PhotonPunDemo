using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    public ScoresMenuController scoresMenu;
    public GameplayHudController gameplayHudController;
    public KillFeedController killFeed;
    public PauseMenuController pauseController;
    public CalvoSelectionController calvoSelector;
    private GameObject fadeImage;

    private void Awake()
    {
        fadeImage = transform.parent.Find("Image").gameObject;
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        gameplayHudController.UpdateHealth(currentHealth, maxHealth);
    }

    public void UpdateAmmo(int currentAmmo, int maxAmmo)
    {
        gameplayHudController.UpdateAmmo(currentAmmo, maxAmmo);
    }

    public void UpdatePlayerScore(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        scoresMenu.UpdateScoreItem(targetPlayer, changedProps);   
    }

    public void ShowScores()
    {
        bool active = scoresMenu.gameObject.activeInHierarchy;
        scoresMenu.transform.parent.gameObject.SetActive(!active);
        scoresMenu.gameObject.SetActive(!active);
        gameplayHudController.gameObject.SetActive(active);
    }

    public bool Pause()
    {
        bool pause = pauseController.Pause();
        pauseController.transform.parent.gameObject.SetActive(pause);
        gameplayHudController.gameObject.SetActive(!pause);
        return pause;
    }

    public void ShowKill(string killer, string victim)
    {
        killFeed.ShowKill(killer, victim);
    }

    public void SelectCalvo(int calvo)
    {
        fadeImage.SetActive(false);

        calvoSelector.gameObject.SetActive(false);
        calvoSelector.transform.parent.gameObject.SetActive(false);
        gameplayHudController.gameObject.SetActive(true);

        GameManager.instance.SelectCalvo(calvo);
    }
}
