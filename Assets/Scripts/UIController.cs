using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class UIController : MonoBehaviour
{
    // singleton
    public static UIController instance;

    private void Awake()
    {
        instance = this;
    }

    public TMP_Text overheatedMessage;
    public Slider weaponOverheatSlider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
