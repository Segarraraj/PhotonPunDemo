using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ScoresMenuController : MonoBehaviour
{

    public GameObject scoreItemPrefab;
    public Transform content;

    private List<ScoreItemController> controllers = new List<ScoreItemController>();

    public void UpdateScoreItem(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        ScoreItemController controller = controllers.Find(x => x.player.NickName == targetPlayer.NickName);
        if (controller)
        {
            if (changedProps != null)
            {
                controller.UpdateScore(changedProps);
            }
            else
            {
                controllers.Remove(controller);
                Destroy(controller.gameObject);
            }
        }
        else if (changedProps != null)
        {
            GameObject instance = Instantiate(scoreItemPrefab, content);
            controller = instance.GetComponent<ScoreItemController>();
            controller.player = targetPlayer;
            controller.UpdateScore(changedProps);
            controllers.Add(controller);
        }
    }
}
