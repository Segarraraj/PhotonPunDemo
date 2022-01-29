using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;

public class GameManager : MonoBehaviourPunCallbacks
{

    public static GameManager instance;
    
    [Header("Prefabs")]
    public GameObject[] playerPrefabs;
    public GameObject playerHud;

    [Header("Level")]
    public CinemachineVirtualCamera normalVirtualCamera;
    public CinemachineVirtualCamera aimVirtualCamera;
    public GameObject introTimeline;

    private GameObject[] spawnPoints;
    
    [HideInInspector]
    public ThirdPersonController localPlayer;
    private HudController localHud;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        Canvas rootCanvas = GameObject.FindObjectOfType<Canvas>();
        GameObject hud = Instantiate(playerHud, rootCanvas.transform);
        localHud = hud.GetComponent<HudController>();

        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        
        if (PhotonNetwork.IsConnected)
        {
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable() {
                { "kills", 0 }, { "deaths", 0 }
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                localHud.UpdatePlayerScore(PhotonNetwork.PlayerList[i], PhotonNetwork.PlayerList[i].CustomProperties.Count != 0 ? PhotonNetwork.PlayerList[i].CustomProperties : properties);
            }
        }            
    }

    public override void OnLeftRoom()   
    {
        SceneManager.LoadScene(0);
    }
    
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        localHud.UpdatePlayerScore(targetPlayer, changedProps);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        localHud.UpdatePlayerScore(otherPlayer, null);
    }

    public void LeaveRoom()
    {
        Cursor.lockState = CursorLockMode.None;

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Leaving)
            {
                return;
            }

            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    public void ShowScores()
    {
        localHud.ShowScores();
    }

    public void ShowKill(string killer, string victim)
    {
        localHud.ShowKill(killer, victim);
    }

    public void ShowPause()
    {
        bool pause = localHud.Pause();
        if (pause)
        {
            Cursor.lockState = CursorLockMode.None;
            localPlayer.playerInput.Player.Disable();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            localPlayer.playerInput.Player.Enable();
        }
    }

    public void Respawn()
    {
        localPlayer.Default();

        GameObject spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        localPlayer.transform.position = spawnPoint.transform.position;

        localHud.UpdateHealth(localPlayer.currentHealth, localPlayer.maxHealth);
        localHud.UpdateAmmo(localPlayer.currentAmmo, localPlayer.maxAmmo);
    }

    public void SelectCalvo(int calvo)
    {
        Cursor.lockState = CursorLockMode.Locked;

        GameObject instance;
        GameObject spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (PhotonNetwork.IsConnected)
        {
            instance = PhotonNetwork.Instantiate(playerPrefabs[calvo].name, spawnPoint.transform.position, spawnPoint.transform.rotation);
        }
        else
        {
            instance = Instantiate(playerPrefabs[calvo], spawnPoint.transform.position, spawnPoint.transform.rotation);
        }

        localPlayer = instance.GetComponent<ThirdPersonController>();

        localPlayer.aimVirtualCamera = aimVirtualCamera;
        localPlayer.normalVirtualCamera = normalVirtualCamera;
        aimVirtualCamera.Follow = localPlayer.followCameraTarget;
        normalVirtualCamera.Follow = localPlayer.followCameraTarget;

        localPlayer.OnHit += localHud.UpdateHealth;
        localPlayer.OnShoot += localHud.UpdateAmmo;

        localHud.UpdateHealth(localPlayer.currentHealth, localPlayer.maxHealth);
        localHud.UpdateAmmo(localPlayer.currentAmmo, localPlayer.maxAmmo);

        introTimeline.SetActive(false);
    }
}
