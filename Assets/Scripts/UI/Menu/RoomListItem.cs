using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class RoomListItem : MonoBehaviour
{
    public Text owner;
    public Text roomName;
    public Text mapName;
    public Text playerCount;

    public Color evenColor;
    public Color oddColor;

    private Image background;

    [HideInInspector]
    public RoomInfo info;

    private void Awake()
    {
        background = GetComponent<Image>();
    }

    public void setRoomInfo(RoomInfo info, bool even)
    {
        this.info = info;

        owner.text = info.CustomProperties["owner"].ToString();
        roomName.text = info.Name;
        mapName.text = info.CustomProperties["map"].ToString();
        playerCount.text = info.PlayerCount + "/" + info.MaxPlayers;

        background.color = even ? evenColor : oddColor;
    }
}
