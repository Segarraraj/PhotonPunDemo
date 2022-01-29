using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    public Text playerName;
    public Slider health;

    public void setInitalData(string name, float health)
    {
        playerName.text = name;
        this.health.value = health;
    }

    public void updateHealth(float health)
    {
        this.health.value = health;
    }
}
