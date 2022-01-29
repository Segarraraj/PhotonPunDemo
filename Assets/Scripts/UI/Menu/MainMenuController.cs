using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MainMenuController : MonoBehaviour
{
    public InputField playerName;

    private const string PLAYERNAMEKEY = "PLAYERNAME";

    // Start is called before the first frame update
    private void Start()
    {
        string name = PlayerPrefs.GetString(PLAYERNAMEKEY, "DefaultName");
        playerName.text = name;
        setNickname(name);
    }

    private void OnEnable()
    {
        playerName.onEndEdit.AddListener(inputfieldChanged);
    }

    private void OnDisable()
    {
        playerName.onEndEdit.RemoveListener(inputfieldChanged);
    }

    private void setNickname(string nickname)
    {
        PhotonNetwork.NickName = nickname;
    }

    public void inputfieldChanged(string value)
    {
        PlayerPrefs.SetString(PLAYERNAMEKEY, value);
        setNickname(value);
    }

    public void exit()
    {
        Application.Quit();
    }
}
