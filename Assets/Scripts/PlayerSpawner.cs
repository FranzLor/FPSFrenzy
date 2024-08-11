using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject playerPrefab;
    private GameObject player;

    public GameObject deathFX;


    // Start is called before the first frame update
    void Start()
    {
        // make sure we are connected to the server
        if (PhotonNetwork.IsConnected)
        {
            // spawn player
            SpawnPlayer();
        }

    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();

        // save player game object for deleting later
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    public void Die()
    {
        // play death fx
        PhotonNetwork.Instantiate(deathFX.name, player.transform.position, Quaternion.identity);

        PhotonNetwork.Destroy(player);

        SpawnPlayer();
    }
}
