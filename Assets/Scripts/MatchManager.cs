using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager instance;

    private void Awake()
    {
        instance = this;
    }

    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        ChangeStat
    }

    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    // used for referencing player in list, faster than searching list
    private int index;

    private List<LeaderboardPlayer> leaderboardPlayers = new List<LeaderboardPlayer>();

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            // loads up main menu scene
            SceneManager.LoadScene(0);
        } else
        {
            NewPlayerSend(PhotonNetwork.NickName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // show leaderboard when tab key is pressed
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // deactivate if already active
            if (UIController.instance.leaderboard.activeInHierarchy)
            {
                UIController.instance.leaderboard.SetActive(false);
            }
            else
            {
                ShowLeaderboard();
            }
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        // make sure event is not more than photon's reserved events
        if (photonEvent.Code < 200)
        {
            // cast event code to EventCodes enum
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            // switch statement to handle different events
            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayerReceive(data);
                    break;
                case EventCodes.ChangeStat:
                    ChangeStatReceive(data);
                    break;
            }
        }
    }

    public override void OnEnable()
    {
        // add to list of callback targets, listen for events
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        // remove from list of callback targets, stop listening for events
        PhotonNetwork.RemoveCallbackTarget(this);
    }


    public void NewPlayerSend(string username)
    {
        // package contains username, actor number, kills, deaths
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;

        // set kills and deaths 0 for new player
        package[2] = 0;
        package[3] = 0;

        // send event to master client only
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }
        );
    }

    public void NewPlayerReceive(object[] dataReceived)
    {
        // create new player object with data received, converted to correct types
        PlayerInfo player = new PlayerInfo(
            (string)dataReceived[0],
            (int)dataReceived[1],
            (int)dataReceived[2],
            (int)dataReceived[3]
        );

        allPlayers.Add(player);

        // update list of players for all clients
        ListPlayerSend();
    }

    public void ListPlayerSend()
    {
        object[] package = new object[allPlayers.Count];

        // package contains player name, actor number, kills, deaths
        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];
            piece[0] = allPlayers[i].playerName;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i] = piece;
        }

        // send event to all clients
        PhotonNetwork.RaiseEvent(
           (byte)EventCodes.ListPlayers,
           package,
           new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true }
       );
    }

    public void ListPlayerReceive(object[] dataReceived)
    {
        // clear all players list to avoid duplicates
        allPlayers.Clear();

        // loop through data received and create new player objects
        for (int i = 0; i < dataReceived.Length; i++)
        {
            object[] piece = (object[])dataReceived[i];

            // create new player object with data received, converted to correct types
            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
            );

            allPlayers.Add(player);

            // if player is local player, set index to player's index in list
            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i;
            }
        }
    }

    public void ChangeStatSend(int actorSending, int statUpdate, int amountChanged)
    {
        // package contains actor sending, stat to update, amount to change
        object[] package = new object[] { actorSending, statUpdate, amountChanged };

        PhotonNetwork.RaiseEvent(
           (byte)EventCodes.ChangeStat,
           package,
           new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true }
       );
    }

    public void ChangeStatReceive(object[] dataReceived)
    {
        int actor = (int)dataReceived[0];
        int statType = (int)dataReceived[1];
        int amount = (int)dataReceived[2];

        // loop through all players and update stats for player with matching actor number
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].actor == actor)
            {
                switch (statType)
                {
                    // kills
                    case 0:
                        allPlayers[i].kills += amount;
                        break;
                    // deaths
                    case 1:
                        allPlayers[i].deaths += amount;
                        break;
                }

                // if player is local player, update stats display
                if (i == index)
                {
                    UpdateStatsDisplay();
                }

                // fixes leaderboard not updating while open
                if (UIController.instance.leaderboard.activeInHierarchy)
                {
                    ShowLeaderboard();
                }
                break;
            }
        }
    }

    // updates and displays stats for player, kills and deaths
    public void UpdateStatsDisplay()
    {
        // makes sure player is in list
        if (allPlayers.Count > index)
        {
            UIController.instance.killsText.text = "Kills - " + allPlayers[index].kills;
            UIController.instance.deathsText.text = "Deaths - " + allPlayers[index].deaths;
        } 
        // if player is not in list, display 0 kills and deaths
        else
        {
            UIController.instance.killsText.text = "Kills - 0";
            UIController.instance.deathsText.text = "Deaths - 0";
        }
    }


    void ShowLeaderboard()
    {
        UIController.instance.leaderboard.SetActive(true);

        // clear all leaderboard players
        foreach (LeaderboardPlayer lp in leaderboardPlayers)
        {
            Destroy(lp.gameObject);
        }
        leaderboardPlayers.Clear();

        UIController.instance.leaderboardPlayerDisplay.gameObject.SetActive(false);

        // sort players by kills
        List<PlayerInfo> sorted = SortPlayers(allPlayers);

        // loop through all players and display stats for each player, sorted by kills
        foreach (PlayerInfo player in sorted)
        {
            LeaderboardPlayer newPlayerDisplay = Instantiate(
                UIController.instance.leaderboardPlayerDisplay,
                UIController.instance.leaderboardPlayerDisplay.transform.parent
            );

            newPlayerDisplay.SetDetails(player.playerName, player.kills, player.deaths);
            newPlayerDisplay.gameObject.SetActive(true);

            leaderboardPlayers.Add(newPlayerDisplay);
        }
    }

    // purely to sort highest kills to lowest kills
    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while (sorted.Count < players.Count)
        {
            // -1 to ensure first player is always selected
            int highest = -1;
            PlayerInfo selectedPlayer = players[0];

            foreach (PlayerInfo player in players)
            {
                // make sure player is not already in sorted list
                if (!sorted.Contains(player))
                {
                    // if player has more kills than current highest, set player as selection
                    if (player.kills > highest)
                    {
                        selectedPlayer = player;
                        highest = player.kills;
                    }
                }
            }
            sorted.Add(selectedPlayer);
        }

        return sorted;
    }
}


[System.Serializable]
public class PlayerInfo
{
    public string playerName;
    public int actor, kills, deaths;

    public PlayerInfo(string playerName, int actor, int kills, int deaths)
    {
        // asign values to player info
        this.playerName = playerName;
        this.actor = actor;
        this.kills = kills;
        this.deaths = deaths;

    }


}