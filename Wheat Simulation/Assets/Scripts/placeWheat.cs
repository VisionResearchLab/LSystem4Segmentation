using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlaceWheat : MonoBehaviour
{
    public TextMeshProUGUI buttonText;

    private bool currentlyPlacingWheat;
    
    // Start is called before the first frame update
    void Start()
    {
        buttonText.text = "Start";
        currentlyPlacingWheat = false;
    }

    public void OnButtonPress(){
        if (currentlyPlacingWheat){
            buttonText.text = "Start";
            currentlyPlacingWheat = false;
        } 
        else {
            buttonText.text = "Stop";
            currentlyPlacingWheat = true;
        }
    }
}
