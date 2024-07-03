using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering.HighDefinition;
using System;
using UnityEngine.UIElements;

public class PlaceWheat : MonoBehaviour
{
    // Variables to handle the position of wheat that is being placed
    private Vector3 mouseScreenPosition;
    private Vector3 mouseWorldPosition;

    // Camera that is in use
    [SerializeField] private Camera cam;

    public void PlaceWheatAtCursor(){
        // https://forum.unity.com/threads/help-on-object-attached-to-mouse.167316/
        mouseScreenPosition = Input.mousePosition;
        mouseWorldPosition = cam.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, cam.nearClipPlane+1));

        Ray ray = new Ray(origin: cam.transform.position, direction: (mouseWorldPosition - cam.transform.position).normalized);
        RaycastHit hit;

        // Layermask 6 is the wheat layermask
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, Wheat.groundLayerMask)){
            InstantiateWheat.IW.GenerateWheat(hit.point, false);
        }
    }
}
