using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayHudController : MonoBehaviour
{
    public Slider leftSlider;
    public Slider rightSlider;
    public Text ammoText;

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        leftSlider.value = currentHealth / maxHealth;
        rightSlider.value = currentHealth / maxHealth;
    }

    public void UpdateAmmo(int currentAmmo, int maxAmmo)
    {
        ammoText.text = currentAmmo + "/" + maxAmmo;
    }
}
