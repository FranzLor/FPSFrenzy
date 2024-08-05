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
    public TMP_Text serverNameText;

    public GameObject errorScreen;
    public TMP_Text errorText;

    public GameObject serverBrowserScreen;
    public ServerButton theServerButton;
    private List<ServerButton> serverButtons = new List<ServerButton>();

    // Start is called before the first frame update
    void Start()
    {
        CloseMenus();

        // show loading/connecting screen
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting to Server...";

        // connect to photon server
        PhotonNetwork.ConnectUsingSettings();
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
    }

    public override void OnConnectedToMaster()
    {

        PhotonNetwork.JoinLobby();
        loadingText.text = "Connected to Server!";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);
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

    public override void OnJoinedRoom()
    {
        CloseMenus();
        serverScreen.SetActive(true);

        // set server name text
        serverNameText.text = PhotonNetwork.CurrentRoom.Name;
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

}
