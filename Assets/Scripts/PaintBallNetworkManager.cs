using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PaintBallNetworkManager : NetworkManager
{
    public PaintBallUI ui;
    public PaintBallManager manager;

    public int matchDuration = 120;

    [Header("Timing")]
    public float playerJoiningWaitTime;

    [Header("AmmoPacks")]
    public GameObject ammoPrefab;
    public Transform ammoLocations;

    int players;
    Queue<Color> availableColors;
    Dictionary<Color, int> scores;

    private void Awake()
    {
        players = 0;
        availableColors = new Queue<Color>();
        scores = new Dictionary<Color, int>();

        availableColors.Enqueue(Color.red);
        scores.Add(Color.red, 0);

        availableColors.Enqueue(Color.green);
        scores.Add(Color.green, 0);

        availableColors.Enqueue(Color.blue);
        scores.Add(Color.blue, 0);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        if(players < 3)
        {
            Transform startPos = GetStartPosition();
            GameObject player = Instantiate<GameObject>(playerPrefab, startPos.position, startPos.rotation);
            player.GetComponent<PaintballPlayer>().playerColor = availableColors.Dequeue();
            ++players;
            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

            if(players == 3)
            {
                manager.OnConnectionAchieved();
                manager.SetBlockStart(true);
                StartCoroutine(AllPlayersJoinedCo());
            }
        }
        else
        {
            manager.OnGameStarted();
        }
    }

    IEnumerator AllPlayersJoinedCo()
    {
        yield return new WaitForSeconds(playerJoiningWaitTime);
        ui.AllPlayersJoined();
        while (ui.secondsToStart > 0)
        {
            yield return new WaitForSeconds(1f);
            ui.secondsToStart--;
            ui.UpdateText("All players joined. Starting in " + ui.secondsToStart);
        }
        ui.UIEnabled(false);
        manager.SetBlockStart(false);
        manager.OnGameStarted();

        foreach(Transform t in ammoLocations)
        {
            GameObject ammo = Instantiate<GameObject>(ammoPrefab,t.position, t.rotation);
            NetworkServer.Spawn(ammo);
        }
        StartCoroutine(MatchTimer());
    }

    IEnumerator MatchTimer()
    {
        ui.TimerEnabled(true);
        while(matchDuration > 0)
        {
            ui.UpdateTimerText(SecondsToTimerString(matchDuration-1));
            matchDuration--;
            yield return new WaitForSeconds(1f);
        }
        manager.OnGameEnded();
        ui.TimerEnabled(false);
        ui.UIEnabled(true);

        string resultsText = "";
        resultsText += "Red's Score: " + scores[Color.red] + "\n";
        resultsText += "Green's Score: " + scores[Color.green] + "\n";
        resultsText += "Blue's Score: " + scores[Color.blue] + "\n";
        ui.UpdateText(resultsText);
    }

    string SecondsToTimerString(int seconds)
    {
        string mins = "0";

        int msec = seconds % 60;
        string secs = (msec > 9 ? "" : "0") + (msec).ToString();

        if(seconds > 60)
        {
            mins = (seconds / 60).ToString();
        }

        return mins + ":" + secs;
    }

    public void AddScore(Color c)
    {
        scores[c]++;
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        availableColors.Enqueue(conn.playerControllers[0].gameObject.GetComponent<PaintballPlayer>().playerColor);
        base.OnServerDisconnect(conn);
    }
}
