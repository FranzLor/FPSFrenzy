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
    private int index;

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

        // send event to master client
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
    }

    public void ListPlayerSend()
    {

    }

    public void ListPlayerReceive(object[] dataReceived)
    {

    }

    public void ChangeStatSend()
    {

    }

    public void ChangeStatReceive(object[] dataReceived)
    {

    }
}


[System.Serializable]
public class PlayerInfo
{
    public string playerName;
    public int actor, kills, deaths;

    public PlayerInfo(string playerName, int actor, int kills, int deaths)
    {
        playerName = this.playerName;
        actor = this.actor;
        kills = this.kills;
        deaths = this.deaths;
    }
}
