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
}
