using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class ServerButton : MonoBehaviour
{
    public TMP_Text buttonText;

    private RoomInfo info;
    
    public void SetButtonDetails(RoomInfo inputInfo)
    {
        info = inputInfo;

        // set button text to server name
        buttonText.text = info.Name;
    }

    public void OpenServer()
    {
        Launcher.instance.JoinServer(info);
    }
}
