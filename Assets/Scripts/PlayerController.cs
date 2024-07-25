using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
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
    public float timeBetweenShots = 0.15f;
    private float shotCounter;
    // overheating 
    public float maxHeat = 8f, heatPerShot = 1f, coolRate = 4f, overheatCoolRate = 5f;
    private float heatCounter = 0f;
    private bool overHeated = false;

    // guns
    public Gun[] allGuns;
    private int selectedGun = 0;


    // Start is called before the first frame update
    void Start()
    {
        // hide and lock cursor at start
        Cursor.lockState = CursorLockMode.Locked;

        camera = Camera.main;

        // set slider max value
        UIController.instance.weaponOverheatSlider.maxValue = maxHeat;

        SwitchGun();
    }

    // Update is called once per frame
    void Update()
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
        } else
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
        } else
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

        // gun overheating
        if (!overHeated)
        {
            // call shoot funct
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }

            if (Input.GetMouseButton(0))
            {
                shotCounter -= Time.deltaTime;

                if (shotCounter <= 0)
                {
                    Shoot();
                }
            }

            heatCounter -= coolRate * Time.deltaTime;
        } else
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
            SwitchGun();

        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun--;

            if (selectedGun < 0)
            {
                selectedGun = allGuns.Length - 1;
            }

            SwitchGun();
        }
       

        // unlock cursor using ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        } else if (Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
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
            // fixes bullet impact flickering with hit.normal
            GameObject bulletImpactObject = Instantiate(bulletImpact,
                                                        hit.point + (hit.normal * 0.002f),
                                                        Quaternion.LookRotation(hit.normal,
                                                        Vector3.up)
            );

            Destroy(bulletImpactObject, 10f);
        }

        shotCounter = timeBetweenShots;

        heatCounter += heatPerShot;

        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;

            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        } 

    }

    private void LateUpdate()
    {
        camera.transform.position = viewPoint.position;
        camera.transform.rotation = viewPoint.rotation;
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
    }
}
