using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalvoSelectionController : MonoBehaviour
{
    public Text description;
    public Text stats;

    public void SetCalvoDescription(int calvo)
    {
        switch (calvo)
        {
            case 0:
                description.text = "John lost his hair while he was working for a programming company. Now, has taken up in arms and is willing to kill anybody to recover his hair.";
                stats.text = "Is well known for the usage of an assault rifle that shoots fast and deals low quantities of damage.";
                break;
            case 1:
                description.text = "Stacy, who has born bald is bold by nature. Tired of her bald life is pursuing a new hair, she is willing to kill anybody for doing so.";
                stats.text = "Stacy wears a big assault rifle which shoots slow and deals bigger quantities of damage.";
                break;
        }
    }
}
