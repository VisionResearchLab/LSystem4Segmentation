using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlaceWheat : MonoBehaviour
{
    // The text on the button that allows the user to place wheat; usually "Start" or "Stop"
    public TextMeshProUGUI buttonText;

    // True if the user is currently placing wheat
    private bool currentlyPlacingWheat;


    // Variables to handle the position of wheat that is being placed
    private Vector3 mouseScreenPosition;
    private Vector3 mouseWorldPosition;

    
    // Start is called before the first frame update
    void Start()
    {
        buttonText.text = "Start";
        currentlyPlacingWheat = false;
    }

    // Called from the button object
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

    void Update(){
        if (currentlyPlacingWheat && Input.GetKeyDown(KeyCode.Mouse0)){
            // https://forum.unity.com/threads/help-on-object-attached-to-mouse.167316/
            mouseScreenPosition = Input.mousePosition;
            mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, GetComponent<Camera>().nearClipPlane+1));

            Ray ray = new Ray(origin: GetComponent<Camera>().transform.position, direction: (mouseWorldPosition - GetComponent<Camera>().transform.position).normalized);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)){
                InstantiateWheat.IW.GenerateWheat(hit.point);
            }
        }
    }
}
