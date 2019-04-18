using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class PaintBallUI : NetworkBehaviour
{
    public GameObject uiParent;
    public Text statusText;

    public GameObject timerParent;
    public Text timerText;

    public int secondsToStart = 5;

    // Start is called before the first frame update
    void Start()
    {
        statusText.text = "Waiting for more players to join";
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
        }
    }

    public void AllPlayersJoined()
    {
        if (isServer)
        {
            RpcUpdateClientText("All players joined. Starting in " + secondsToStart);
        }
    }

    public void UpdateText(string text)
    {
        if (isServer)
        {
            RpcUpdateClientText(text);
        }
    }

    public void UpdateTimerText(string text)
    {
        if (isServer)
        {
            RpcUpdateTimerText(text);
        }
    }

    public void UIEnabled(bool e)
    {
        uiParent.SetActive(e);
        RpcUIEnabled(e);
    }

    public void TimerEnabled(bool e)
    {
        timerParent.SetActive(e);
        RpcTimerEnabled(e);
    }

    [ClientRpc]
    public void RpcUpdateClientText(string text)
    {
        statusText.text = text;
    }

    [ClientRpc]
    public void RpcUpdateTimerText(string text)
    {
        timerText.text = text;
    }

    [ClientRpc]
    public void RpcUIEnabled(bool e)
    {
        uiParent.SetActive(e);
    }

    [ClientRpc]
    public void RpcTimerEnabled(bool e)
    {
        timerParent.SetActive(e);
    }
}
