using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class ScoreItemController : MonoBehaviour
{
    public Player player;
    public Text nickname;
    public Text stats;

    public void UpdateScore(ExitGames.Client.Photon.Hashtable changedProps)
    {
        nickname.text = player.NickName;
        stats.text = changedProps["kills"].ToString() + "/" + changedProps["deaths"].ToString();
    }
}
