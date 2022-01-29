using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KillFeedController : MonoBehaviour
{
    public Text killer;
    public Text victim;

    private Animator animator;

    private Queue<KeyValuePair<string, string>> kills;
    private bool appear = false;

    private void Awake()
    {
        kills = new Queue<KeyValuePair<string, string>>();
        animator = GetComponent<Animator>();
    }

    public void OnAppear()
    {
        appear = false;
        animator.SetBool("Appear", false);
    }

    public void OnDissapear()
    {
        ShowNextKill();
    }

    public void ShowKill(string killer, string victim)
    {
        kills.Enqueue(new KeyValuePair<string, string>(killer, victim));
        ShowNextKill();
    }

    private void ShowNextKill()
    {
        if (kills.Count == 0)
        {
            return;
        }

        if (appear)
        {
            return;
        }

        KeyValuePair<string, string> kill = kills.Dequeue();

        killer.text = kill.Key;
        victim.text = kill.Value;

        appear = true;
        animator.SetBool("Appear", true);
    }
}
