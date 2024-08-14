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

    public float respawnTime = 5f;


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

    public void Die(string damager)
    {
        // death screen with killer name
        UIController.instance.deathText.text = "Killed by " + damager;

        // change stat for player who died, increment death by 1
        MatchManager.instance.ChangeStatSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

        if (player != null)
        {
            // destroy player
            StartCoroutine(DieCo());
        }
    }

    // coroutine for player death
    public IEnumerator DieCo()
    {
        // play death fx
        PhotonNetwork.Instantiate(deathFX.name, player.transform.position, Quaternion.identity);

        PhotonNetwork.Destroy(player);
        player = null;
        UIController.instance.deathScreen.SetActive(true);

        // stops coroutine for 5 seconds - respawn time var
        yield return new WaitForSeconds(respawnTime);

        UIController.instance.deathScreen.SetActive(false);

        // as long as game is playing, respawn player
        // null for round end
        if (MatchManager.instance.gameState == MatchManager.GameState.Playing && player == null)
        {
            SpawnPlayer();
        }

    }
}
