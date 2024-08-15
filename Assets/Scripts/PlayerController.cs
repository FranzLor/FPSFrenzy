using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    // camera
    public Transform viewPoint;
    private Camera camera;

    // mouse sens
    public float mouseSensitivity = 1f;
    private float verticalRotationStore = 0f;
    private Vector2 mouseInput;
    public bool invertVerticalLook = false;

    // movement
    public float moveSpeed = 8f, runSpeed = 12f;
    private float activeMoveSpeed = 0f;
    private Vector3 moveDirection, movement;

    // character controller
    public CharacterController characterController;

    // jumping
    public float jumpForce = 8f, gravityModifier = 2.5f;

    // ground check
    public Transform groundCheckpoint;
    private bool isGrounded = false;
    public LayerMask groundLayers;

    // bullets
    public GameObject bulletImpact;
    private float shotCounter;
    public float muzzleDisplayTime = 0.02f;
    private float muzzleCounter = 0f;
    // overheating 
    public float maxHeat = 8f, coolRate = 4f, overheatCoolRate = 5f;
    private float heatCounter = 0f;
    private bool overHeated = false;

    // guns
    public Gun[] allGuns;
    private int selectedGun = 0;

    public GameObject playerHitImpact;

    public int maxHealth = 100;
    private int currentHealth;

    // player animations
    public Animator anim;
    public GameObject playerModel;
    public Transform modelGunPoint, gunHolder;




    // Start is called before the first frame update
    void Start()
    {
        // hide and lock cursor at start
        Cursor.lockState = CursorLockMode.Locked;

        camera = Camera.main;

        // set slider max value
        UIController.instance.weaponOverheatSlider.maxValue = maxHeat;

        // set gun, use RPC to set gun for all players clients
        //SwitchGun();
        photonView.RPC("SetGun", RpcTarget.All, selectedGun);

        // set health to max at start
        currentHealth = maxHealth;

        // if first person, hide player model
        if (photonView.IsMine)
        {
            playerModel.SetActive(false);

            // health slider
            // updates health slider value per player
            UIController.instance.healthSlider.maxValue = maxHealth;
            UIController.instance.healthSlider.value = currentHealth;
        } else
        {
            // set gun position for gun adjuster holder - zero transform pos
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }
        
        // moved to player spawner script

        // gets spawn point from funct
        //Transform newTransform = SpawnManager.instance.GetSpawnPoint();
        //transform.position = newTransform.position;
        //transform.rotation = newTransform.rotation;

    }

    // Update is called once per frame
    void Update()
    {
        // make sure the view is ours first
        if (photonView.IsMine)
        {
            // get mouse inputs
            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;

            // rotate player horizontally
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
                                                  transform.rotation.eulerAngles.y + mouseInput.x,
                                                  transform.rotation.eulerAngles.z
            );

            // rotate player vertically using clamp
            verticalRotationStore += mouseInput.y;
            verticalRotationStore = Mathf.Clamp(verticalRotationStore, -60f, 60f);

            if (invertVerticalLook)
            {
                // rotate camera vertically using vertical rotation stored
                viewPoint.rotation = Quaternion.Euler(verticalRotationStore,
                                                      viewPoint.rotation.eulerAngles.y,
                                                      viewPoint.rotation.eulerAngles.z
                );
            }
            else
            {
                viewPoint.rotation = Quaternion.Euler(-verticalRotationStore,
                                                      viewPoint.rotation.eulerAngles.y,
                                                      viewPoint.rotation.eulerAngles.z
                );
            }

            // move player
            moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            // check if player is running
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // running
                activeMoveSpeed = runSpeed;
            }
            else
            {
                // walking
                activeMoveSpeed = moveSpeed;
            }

            float yVelocity = movement.y;

            // normalize move direction to prevent faster diagonal movement
            movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * activeMoveSpeed;
            movement.y = yVelocity;

            // fixes y velocity when grounded
            if (characterController.isGrounded)
            {
                movement.y = 0f;
            }

            isGrounded = Physics.Raycast(groundCheckpoint.position, Vector3.down, 0.25f, groundLayers);

            // check player jump input
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                movement.y = jumpForce;
            }


            // apply gravity
            movement.y += Physics.gravity.y * Time.deltaTime * gravityModifier;

            characterController.Move(movement * Time.deltaTime);

            // deactivate muzzle flash before shooting frame
            if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
            {
                muzzleCounter -= Time.deltaTime;

                if (muzzleCounter <= 0)
                {
                    allGuns[selectedGun].muzzleFlash.SetActive(false);
                }
            }


            // gun overheating
            if (!overHeated)
            {
                // call shoot funct
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }

                if (Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
                {
                    shotCounter -= Time.deltaTime;

                    if (shotCounter <= 0)
                    {
                        Shoot();
                    }
                }

                heatCounter -= coolRate * Time.deltaTime;
            }
            else
            {
                heatCounter -= overheatCoolRate * Time.deltaTime;

                if (heatCounter <= 0)
                {
                    overHeated = false;

                    UIController.instance.overheatedMessage.gameObject.SetActive(false);
                }
            }

            if (heatCounter < 0)
            {
                heatCounter = 0f;
            }

            UIController.instance.weaponOverheatSlider.value = heatCounter;

            // switch guns using mouse scroll
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                selectedGun++;

                if (selectedGun >= allGuns.Length)
                {
                    selectedGun = 0;
                }
                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);

            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                selectedGun--;

                if (selectedGun < 0)
                {
                    selectedGun = allGuns.Length - 1;
                }

                //SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            }

            // switch guns using num keys
            for (int i = 0; i < allGuns.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    selectedGun = i;
                    //SwitchGun();
                    photonView.RPC("SetGun", RpcTarget.All, selectedGun);
                }
            }

            // animation parameters for player
            anim.SetBool("grounded", isGrounded);
            anim.SetFloat("speed", moveDirection.magnitude);


            // unlock cursor using ESC
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                // account for options screen being not active
                if (Input.GetMouseButtonDown(0) && !UIController.instance.optionsScreen.activeInHierarchy)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }

    private void Shoot()
    {
        // create ray from camera
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        ray.origin = camera.transform.position;

        // check if ray hits something
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // check if ray hits player
            if (hit.collider.gameObject.tag == "Player")
            {
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);

                // deal damage to player on all clients
                hit.collider.gameObject.GetPhotonView().RPC(
                    "DealDamage", 
                    RpcTarget.All, 
                    photonView.Owner.NickName,
                    allGuns[selectedGun].shotDamage,
                    PhotonNetwork.LocalPlayer.ActorNumber
                );
            } 
            // hits other objs
            else
            {
                // fixes bullet impact flickering with hit.normal
                GameObject bulletImpactObject = Instantiate(bulletImpact,
                                                            hit.point + (hit.normal * 0.002f),
                                                            Quaternion.LookRotation(hit.normal,
                                                            Vector3.up)
                );

                Destroy(bulletImpactObject, 10f);
            }
        }

        // get selected gun data
        shotCounter = allGuns[selectedGun].timeBetweenShots;

        heatCounter += allGuns[selectedGun].heatPerShot;

        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;

            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }

        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    // called when player is hit on every client
    [PunRPC]
    public void DealDamage(string damager, int damageAmount, int actor)
    {
        // keep function small for network load
        TakeDamage(damager, damageAmount, actor);
    }

    public void TakeDamage(string damager, int damageAmount, int actor)
    {
        if (photonView.IsMine)
        {
            currentHealth -= damageAmount;

            if (currentHealth <= 0)
            {
                // make sure health is not negative
                currentHealth = 0;

                //Debug.Log(photonView.Owner.NickName + " Hit By: " + damager);
                PlayerSpawner.instance.Die(damager);

                // updates stats for player who killed, increment 1 kill
                MatchManager.instance.ChangeStatSend(actor, 0, 1);
            }

            // set health slider value
            UIController.instance.healthSlider.value = currentHealth;
        }
    }

    private void LateUpdate()
    {
        // check if player is ours
        if (photonView.IsMine)
        {
            if (MatchManager.instance.gameState == MatchManager.GameState.Playing)
            {
                camera.transform.position = viewPoint.position;
                camera.transform.rotation = viewPoint.rotation;
            }
            else
            {
                // moves camera to map camera point after round ends
                // put in late update to destroy camera right away
                camera.transform.position = MatchManager.instance.mapCameraPoint.position;
                camera.transform.rotation = MatchManager.instance.mapCameraPoint.rotation;
            }
        }
    }

    void SwitchGun()
    {
        // hide all guns
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }

        // show selected gun
        allGuns[selectedGun].gameObject.SetActive(true);

        // makes sure to hide muzzle flash when switching guns
        allGuns[selectedGun].muzzleFlash.SetActive(false);
    }

    // server updates which guns switched for players
    [PunRPC]
    public void SetGun(int gunToSwitch)
    {
        // check if gun to switch is within array length. switch
        if (gunToSwitch < allGuns.Length)
        {
            selectedGun = gunToSwitch;
            SwitchGun();
        }
    }
}
