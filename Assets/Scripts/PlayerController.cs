using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform viewPoint;
    public float mouseSensitivity = 1f;
    private float verticalRotationStore = 0f;
    private Vector2 mouseInput;

    public bool invertVerticalLook = false;

    // Start is called before the first frame update
    void Start()
    {
        // hide and lock cursor at start
        Cursor.lockState = CursorLockMode.Locked;
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

    }
}
