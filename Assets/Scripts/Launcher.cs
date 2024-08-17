using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject loadingScreen;
    public TMP_Text loadingText;

    public GameObject menuButtons;

    public GameObject createServerScreen;
    public TMP_InputField serverNameInput;

    public GameObject serverScreen;
    public TMP_Text serverNameText, playerNameLabel;
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();

    public GameObject errorScreen;
    public TMP_Text errorText;

    public GameObject serverBrowserScreen;
    public ServerButton theServerButton;
    private List<ServerButton> serverButtons = new List<ServerButton>();

    public GameObject nameInputScreen;
    public TMP_InputField nameInput;
    // set to false to show name input screen first time,
    // then set to true to skip name input screen after end screen
    public static bool hasSetName = false;

    public string levelToPlay;
    public GameObject startButton;

    public GameObject serverTestButton;

    public string[] allMaps;
    public bool changeMapBetweenRounds = true;



    // Start is called before the first frame update
    void Start()
    {
        CloseMenus();

        // show loading/connecting screen
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to Server...";

        // fixes warning issue - make sure to connect with settings
        if (!PhotonNetwork.IsConnected)
        {
            // connect to photon server
            PhotonNetwork.ConnectUsingSettings();
        }

        // only show server test button in editor - saves time
#if UNITY_EDITOR
        serverTestButton.SetActive(true);
#endif

        // set cursor to visible, since end round hides it
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseMenus()
    {
        // close all menus elements
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createServerScreen.SetActive(false);
        serverScreen.SetActive(false);
        errorScreen.SetActive(false);
        serverBrowserScreen.SetActive(false);
        nameInputScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();

        // syncs scene for all players - start game
        PhotonNetwork.AutomaticallySyncScene = true;

        loadingText.text = "Connected to Server!";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);

        // set player name with random num
        PhotonNetwork.NickName = "Player" + Random.Range(0, 1000).ToString();

        // sets player name first time
        if (!hasSetName)
        {
            CloseMenus();
            nameInputScreen.SetActive(true);

            // load player name prefs
            if (PlayerPrefs.HasKey("PlayerName"))
            {
                nameInput.text = PlayerPrefs.GetString("PlayerName");
            }
        }
        // makes sure to keep player name if already set
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void OpenServerCreate()
    {
        CloseMenus();
        createServerScreen.SetActive(true);
    }

    public void CreateServer()
    {
        if (!string.IsNullOrEmpty(serverNameInput.text))
        {
            RoomOptions options = new RoomOptions();
            // max players per server - change as needed
            options.MaxPlayers = 2;

            // creates server based on options
            PhotonNetwork.CreateRoom(serverNameInput.text, options);

            CloseMenus();
            loadingText.text = "Creating Server...";
            loadingScreen.SetActive(true);
        }

    }

    public void CloseCreateServer()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        CloseMenus();
        serverScreen.SetActive(true);

        // set server name text
        serverNameText.text = PhotonNetwork.CurrentRoom.Name;

        ListPlayers();

        // makes sure host is only one that can start game
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    private void ListPlayers()
    {
        foreach (TMP_Text player in allPlayerNames)
        {
            Destroy(player.gameObject);
        }
        allPlayerNames.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        
        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);

            // add to list of player names
            allPlayerNames.Add(newPlayerLabel);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Failed to Create Server: " + message;
        CloseMenus();
        errorScreen.SetActive(true);
    }

    public void CloseErrorScreen()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void LeaveServer()
    {
        PhotonNetwork.LeaveRoom();

        CloseMenus();
        loadingText.text = "Leaving Server...";
        loadingScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void OpenServerBrowser()
    {
        CloseMenus();
        serverBrowserScreen.SetActive(true);

        // no loading screen needed here 
    }

    public void CloseServerBrowser()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> serverList)
    {
        foreach (ServerButton sb in serverButtons)
        {
            // destroy buttons first 
            Destroy(sb.gameObject);
        }
        // only removes ref if put before destroy
        serverButtons.Clear();

        // hide original server button template - always being copied
        theServerButton.gameObject.SetActive(false);


        for (int i = 0; i < serverList.Count; i++) 
        {
            // only show servers that are not full and not removed from list
            if (serverList[i].PlayerCount != serverList[i].MaxPlayers && !serverList[i].RemovedFromList)
            {
                ServerButton newButton = Instantiate(theServerButton, theServerButton.transform.parent);
                newButton.SetButtonDetails(serverList[i]);
                newButton.gameObject.SetActive(true);

                // add to list of buttons
                serverButtons.Add(newButton);
            }
        }
    }

    public void JoinServer(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);

        CloseMenus();
        loadingText.text = "Joining Server...";
        loadingScreen.SetActive(true);
    }

    public void SetUsername()
    {
        // set player name
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text;

            // saves player name prefs
            PlayerPrefs.SetString("PlayerName", nameInput.text);

            CloseMenus();
            menuButtons.SetActive(true);

            hasSetName = true;
        }
    }

    public void StartGame()
    {
        //PhotonNetwork.LoadLevel(levelToPlay);

        //load random map from list
        PhotonNetwork.LoadLevel(allMaps[Random.Range(0, allMaps.Length)]);
    }

    // if host leaves, switches to another player as host
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    // creates testing server - not needed for final game
    public void QuickJoin()
    {
        PhotonNetwork.CreateRoom("Test Server");
        CloseMenus();
        loadingText.text = "Creating Test Server...";
        loadingScreen.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
