using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HostGameMenuController : MonoBehaviour
{
    public InputField serverName;
    public Text playersLabel;
    public Slider playersSlider;
    public Dropdown map;

    public Launcher launcher;
    public GameObject connectionStatus;

    // Start is called before the first frame update
    void Start()
    {
        updateLabel(playersSlider.value);
    }

    private void OnEnable()
    {
        playersSlider.onValueChanged.AddListener(updateLabel);
    }

    private void OnDisable()
    {
        playersSlider.onValueChanged.RemoveListener(updateLabel);
    }

    // Update is called once per frame
    private void updateLabel(float value)
    {
        playersLabel.text = "Players: " + value;
    }

    public void hostGame()
    {
        if (serverName.text.Equals(""))
        {
            return;
        }

        gameObject.SetActive(false);
        connectionStatus.SetActive(true);

        launcher.host(playersSlider.value, serverName.text, map.options[map.value].text);
    }
}
