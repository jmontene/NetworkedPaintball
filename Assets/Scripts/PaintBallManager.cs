using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PaintBallManager : NetworkBehaviour
{
    public GameObject blockingUI;

    PaintballPlayer player;
    bool inGame = false;
    bool blockStart = false;

    public void OnConnectionAchieved()
    {
        RpcOnConnectionAchieved();
    }

    public void OnGameStarted()
    {
        RpcOnGameStarted();
    }

    public void OnGameEnded()
    {
        RpcOnGameEnded();
    }

    public void SetBlockStart(bool block)
    {
        RpcSetBlockStart(block);
    }

    [ClientRpc]
    public void RpcOnConnectionAchieved()
    {
        inGame = true;
    }

    [ClientRpc]
    public void RpcOnGameEnded()
    {
        if (!inGame) return;
        inGame = false;
        blockStart = false;
        player.OnGameEnded();
    }

    [ClientRpc]
    public void RpcOnGameStarted()
    {
        if (!inGame)
        {
            blockingUI.SetActive(true);
            return;
        }
        if (blockStart) return;
        foreach (PaintballPlayer p in GameObject.FindObjectsOfType<PaintballPlayer>())
        {
            if (p.hasAuthority)
            {
                player = p;
                break;
            }
        }
        player.OnGameStarted();
    }

    [ClientRpc]
    public void RpcSetBlockStart(bool b)
    {
        blockStart = b;
    }
}
