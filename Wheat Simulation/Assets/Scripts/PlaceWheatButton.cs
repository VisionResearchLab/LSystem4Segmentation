using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceWheatButton : MonoBehaviour
{
    // Tells the PlaceWheat script when the button that is a sibling of this script is pressed
    public PlaceWheat placeWheat;

    public void OnButtonPress(){
        placeWheat.OnButtonPress();
    }
}
