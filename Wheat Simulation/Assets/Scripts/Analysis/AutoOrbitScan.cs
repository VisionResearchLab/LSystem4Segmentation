using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;
using CTI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class AutoOrbitScan : MonoBehaviour
{
    // Determines whether we are in auto orbit scanning mode (moves camera around wheat and takes pictures)
    public bool orbitScanning;

    // Only true when the application is busy taking a screenshot and labeling it
    private bool busy;

    
    // Scripts that have functions to be called
    public ScreenShot screenShot;
    public MassAddWheat massAddWheat;


    // Main camera
    public Camera cam;

    // Variables to save and reuse when ending orbit scan mode
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    

    // Local variables for camera movement
    private float currentTime;
    private Vector3 cameraOrbitFocus;


    // Adjustable variables
    [SerializeField] private float minCameraHeight;
    [SerializeField] private float maxCameraHeight;


    HashSet<int> timesPicturesWereTakenAt = new HashSet<int>();

    // Switch between light sources
    [SerializeField] private GameObject defaultSun;
    [SerializeField] private List<GameObject> lightSources = new List<GameObject>();
    [SerializeField] private int pictureCountPerLightSwitch;
    private GameObject activeLightSource;

    private int picturesTaken = 0;

    void Start(){
        busy = false;
        activeLightSource = defaultSun;
    }

    public IEnumerator BeginOrbiting(){
        busy = true;

        // Save initial camera state
        initialCameraPosition = cam.transform.position;
        initialCameraRotation = cam.transform.rotation;

        // Start orbit scanning
        cameraOrbitFocus = GetCameraOrbitFocus();
        

        yield return StartCoroutine(SwitchLightSource());
        // Start to move camera
        orbitScanning = true;

        busy = false;
    }


    void Update()
    {
        if (orbitScanning && !busy){
            // If we have taken (a multiple of pictureCountPerLightSwitch that is not 0) pictures, swap the light source
            if (picturesTaken != 0 && picturesTaken % pictureCountPerLightSwitch == 0){
                StartCoroutine(SwitchLightSource());
                picturesTaken = 0; // prevent loop
            } 
            // Otherwise, take a picture
            else {
                StartCoroutine(TakePicture());
            }
        }
    }

    IEnumerator TakePicture(){
        busy = true;
        Debug.Log("Taking screenshot at time: " + Time.time);

        MoveCameraRandomly();

        yield return StartCoroutine(screenShot.ScreenshotSequenceEnum(0.5f));
        picturesTaken ++;

        busy = false;
        yield return null;
    }

    private Vector3 GetCameraOrbitFocus(){
        Vector3 pos1 = massAddWheat.boundary1.transform.position;
        Vector3 pos2 = massAddWheat.boundary2.transform.position;

        float x = (pos1.x + pos2.x)/2.0f;
        float y = -Mathf.Infinity; // redefined later by raycast
        float z = (pos1.z + pos2.z)/2.0f;

        // Define the y-component of the camera's focal point as the average wheat height above the ground in the center of the generation bounds
        Vector3 rayCastHitPoint = Vector3.down;
        Ray ray = new Ray(origin: new Vector3(x, Math.Max(pos1.y, pos2.y), z), direction: Vector3.down);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, Mathf.Abs(pos1.y - pos2.y), Wheat.groundLayerMask)){
            y = hit.point.y;
        }
        y += 0.5f; // Wheat is 6 units high, but if the camera aims at the top of the wheat, it will see the background

        return new Vector3(x,y,z);
    }

    private Vector3[] GetCameraOrbitBounds(){
        Vector3 pos1 = massAddWheat.boundary1.transform.position;
        Vector3 pos2 = massAddWheat.boundary2.transform.position;

        Vector3[] positions = new Vector3[2]; 
        positions[0] = pos1;
        positions[1] = pos2;

        return positions;
    }

    public void EndOrbiting(){
        orbitScanning = false;
        cam.transform.SetPositionAndRotation(initialCameraPosition, initialCameraRotation);
        StartCoroutine(SwitchLightSource(newLightSource:defaultSun));
    }

    private void MoveCameraRandomly(){
        Vector3 center = GetCameraOrbitFocus();
        Vector3 bound0 = GetCameraOrbitBounds()[0];
        Vector3 bound1 = GetCameraOrbitBounds()[1];

        float x_diff = bound0.x - bound1.x;
        float z_diff = bound0.z - bound1.z;
        float pos_variance = 0.4f; // How much the camera can move around within the orbit bounds

        float x_pos = cameraOrbitFocus.x + UnityEngine.Random.Range(-pos_variance * x_diff, pos_variance * x_diff);
        float y_pos = cameraOrbitFocus.y + UnityEngine.Random.Range(minCameraHeight, minCameraHeight + maxCameraHeight); // Wheat is 6 units tall
        float z_pos = cameraOrbitFocus.z + UnityEngine.Random.Range(-pos_variance * z_diff, pos_variance * z_diff);

        Vector3 cam_pos = new Vector3(x_pos, y_pos, z_pos);

        cam.transform.SetPositionAndRotation(cam_pos, cam.transform.rotation);
        cam.transform.LookAt(center);
        
        // Lower camera angle if its angle is too high and might see background
        if (cam.transform.rotation.x < 40f){
            cam.transform.Rotate(40f - cam.transform.rotation.x, 0f, 0f);
        }
    }

    public IEnumerator SwitchLightSource(GameObject newLightSource = null){
        busy = true;
        float timeToWait = 5.0f;
        
        // If a light source argument is not given, get a random one from the light sources list.
        if (newLightSource == null){
            newLightSource = lightSources[UnityEngine.Random.Range(0, lightSources.Count - 1)];
        }

        // Set the active light source to the new light source.
        activeLightSource = newLightSource;
        newLightSource.SetActive(true);

        // Disable all other light sources.
        if (newLightSource != defaultSun){
            defaultSun.SetActive(false);
        }
        foreach (GameObject lightSource in lightSources){
            if (lightSource != newLightSource){
                lightSource.SetActive(false);
            }
        }

        yield return new WaitForSeconds(timeToWait); // need to wait for lighting to bake

        busy = false;
    }
}
