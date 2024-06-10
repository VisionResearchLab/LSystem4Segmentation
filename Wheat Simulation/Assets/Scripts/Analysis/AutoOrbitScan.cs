using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using CTI;
using UnityEngine;
using UnityEngine.UIElements;

public class AutoOrbitScan : MonoBehaviour
{
    // Determines whether we are in auto orbit scanning mode (moves camera around wheat and takes pictures)
    public bool orbitScanning;
    
    // Scripts that have functions to be called
    public ScreenShot screenShot;
    public RotateSun rotateSun;
    public MassAddWheat massAddWheat;


    // Main camera
    public Camera cam;

    // Variables to save
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private bool initialSunControlsOwnOrbit;
    private float initialSunCurrentTime;

    // Local variables for camera movement
    private bool timeProgressing;
    private float currentTime;
    private Vector3 cameraOrbitFocus;

    // Adjustable variables
    int takePictureInterval = 10; // in seconds
    float timeToWaitForScan = 5.0f;


    public void BeginOrbiting(){
        // Save initial state
        initialCameraPosition = cam.transform.position;
        initialCameraRotation = cam.transform.rotation;
        initialSunControlsOwnOrbit = rotateSun.controlsOwnOrbit;
        initialSunCurrentTime = rotateSun.currentTime;

        // Start orbit scanning
        cameraOrbitFocus = GetCameraOrbitFocus();
        orbitScanning = true;
        
        // Start to move camera and sun
        currentTime = 0f;
        float _ = Time.deltaTime; // reset Time.deltaTime
        timeProgressing = true;
        rotateSun.controlsOwnOrbit = false; // Because the sun's X rotation is synced to this script when orbitScanning = true
    }


    void Update()
    {
        if (orbitScanning && timeProgressing){
            // Time controls
            currentTime += Time.deltaTime;
            
            // Sun controls
            rotateSun.currentTime = currentTime;

            // Camera controls
            cam.transform.SetPositionAndRotation(cameraOrbitFocus, new Quaternion(0f, -90f, 0f, 0f)); // TODO: FIX

            // Pause periodically to take picture
            HashSet<int> timesPicturesWereTakenAt = new HashSet<int>();
            if (!(timesPicturesWereTakenAt.Contains((int) currentTime)) && (int) currentTime % takePictureInterval == 0){
                screenShot.TakeScreenShot();
                timesPicturesWereTakenAt.Add((int) currentTime);
                Pause(timeToWaitForScan);
            }
        }
    }

    // Needs >5 seconds to allow scan to happen
    IEnumerator Pause(float seconds){
        // Start pause
        timeProgressing = false;
        yield return new WaitForSeconds(seconds);

        // End pause
        float _ = Time.deltaTime; // reset Time.deltaTime
        timeProgressing = true;
    }

    private Vector3 GetCameraOrbitFocus(){
        Vector3 pos1 = massAddWheat.boundary1.transform.position;
        Vector3 pos2 = massAddWheat.boundary2.transform.position;

        float x = (pos1.x + pos2.x)/2.0f;
        float y = -Mathf.Infinity; // redefined later by raycast
        float z = (pos1.z + pos2.z)/2.0f;

        // Define the y-component of the camera's focal point as the average wheat height above the ground in the center of the generation bounds
        Vector3 rayCastHitPoint = Vector3.down;
        Ray ray = new Ray(origin: new Vector3(x, Math.Max(pos1.y, pos2.y), z), direction: new Vector3(0, -1, 0));
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, Mathf.Abs(pos1.y - pos2.y), Wheat.groundLayerMask)){
            y = hit.point.y;
        }
        y += 3.5f;

        return new Vector3(x,y,z);
    }

    public void EndOrbiting(){
        cam.transform.SetPositionAndRotation(initialCameraPosition, initialCameraRotation);
        rotateSun.controlsOwnOrbit = initialSunControlsOwnOrbit;
        rotateSun.currentTime = initialSunCurrentTime;
        orbitScanning = false;
    }

    public void ToggleOrbiting(){
        if (orbitScanning){
            EndOrbiting();
        } else {
            BeginOrbiting();
        }
    }
}
