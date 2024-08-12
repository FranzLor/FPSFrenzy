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


    public void NewPlayerSend()
    {

    }

    public void NewPlayerReceive(object[] dataReceived)
    {

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
