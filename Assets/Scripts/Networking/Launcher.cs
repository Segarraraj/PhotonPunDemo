using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";
    
    // Start is called before the first frame update
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Update()
    {
        Debug.Log(PhotonNetwork.NetworkClientState);
    }

    public void host(float players, string name, string mapName)
    {
        ExitGames.Client.Photon.Hashtable settings = new ExitGames.Client.Photon.Hashtable() {
            { "map", mapName }, { "owner", PhotonNetwork.NickName }
        };

        string[] properties = { "map", "owner" };

        RoomOptions options = new RoomOptions();
        options.IsOpen = true;
        options.IsVisible = true;
        options.MaxPlayers = (byte) players;
        options.CustomRoomPropertiesForLobby = properties;
        options.CustomRoomProperties = settings;

        PhotonNetwork.CreateRoom(name, options); 
    }

    public void joinGame(string name)
    {
        if (PhotonNetwork.NetworkClientState == ClientState.ConnectingToGameServer || 
            PhotonNetwork.NetworkClientState == ClientState.Authenticating || 
            PhotonNetwork.NetworkClientState == ClientState.Joining)
        {
            return;
        }

        PhotonNetwork.JoinRoom(name);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log(message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log(cause);
    }

    public override void OnCreatedRoom()
    {        
        PhotonNetwork.LoadLevel(PhotonNetwork.CurrentRoom.CustomProperties["map"].ToString().Replace(" ", ""));
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
    }
}
