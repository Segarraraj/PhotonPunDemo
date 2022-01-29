using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class JoinGameMenuController : MonoBehaviourPunCallbacks
{
    public Transform content;
    public RoomListItem item;
    public Launcher launcher;
    public GameObject connectingMenu;

    private List<RoomListItem> items = new List<RoomListItem>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            int index = items.FindIndex(x => x.info.Name == info.Name);

            if (info.RemovedFromList)
            {
                RoomListItem item = items[index];
                items.RemoveAt(index);
                Destroy(item.gameObject);
                break;
            }

            if (index != -1)
            {
                break;
            }

            RoomListItem instance = Instantiate(item, content);
            items.Add(instance);
            instance.setRoomInfo(info, items.Count % 2 == 0);

            instance.GetComponent<Button>().onClick.AddListener(delegate { 
                launcher.joinGame(info.Name); 
                connectingMenu.SetActive(true); 
            });
        }
    }
}
