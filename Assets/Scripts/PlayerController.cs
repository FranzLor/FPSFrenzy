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


    // Start is called before the first frame update
    void Start()
    {
        // hide and lock cursor at start
        Cursor.lockState = CursorLockMode.Locked;

        camera = Camera.main;
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

        characterController.Move(movement * Time.deltaTime);

        // apply gravity
        movement.y += Physics.gravity.y * Time.deltaTime;

    }


    private void LateUpdate()
    {
        camera.transform.position = viewPoint.position;
        camera.transform.rotation = viewPoint.rotation;
    }
}
