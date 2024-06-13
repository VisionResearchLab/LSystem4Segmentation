using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;
using CTI;
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
    public RotateSun rotateSun;
    public MassAddWheat massAddWheat;


    // Main camera
    public Camera cam;

    // Variables to save and reuse when ending orbit scan mode
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private bool initialSunControlsOwnOrbit;
    private float initialSunCurrentTime;
    private float initialSunLightTemperature;
    private float initialSunLightIntensity;

    // Local variables for camera movement
    private bool timeProgressing;
    private float currentTime;
    private Vector3 cameraOrbitFocus;

    // Adjustable variables
    [SerializeField] private float takePictureInterval;
    [SerializeField] private int sunRotationSpeed;

    // Sun object
    public GameObject sun;
    private Light sunLight;

    HashSet<int> timesPicturesWereTakenAt = new HashSet<int>();

    void Start(){
        sunLight = sun.GetComponent<Light>();
        busy = false;
    }

    public void BeginOrbiting(){
        // Save initial state
        initialCameraPosition = cam.transform.position;
        initialCameraRotation = cam.transform.rotation;
        initialSunControlsOwnOrbit = rotateSun.controlsOwnOrbit;
        initialSunCurrentTime = rotateSun.currentTime;
        initialSunLightTemperature = sunLight.colorTemperature;
        initialSunLightIntensity = sunLight.intensity;

        // Start orbit scanning
        cameraOrbitFocus = GetCameraOrbitFocus();
        orbitScanning = true;
        
        // Start to move camera and sun
        currentTime = 0f;
        float _ = Time.deltaTime; // reset Time.deltaTime
        timeProgressing = true;
        rotateSun.controlsOwnOrbit = false; // Because the sun's X rotation is synced to this script when orbitScanning = true

        timesPicturesWereTakenAt = new HashSet<int>();
    }


    void Update()
    {
        if (orbitScanning && timeProgressing){
            float delta = Time.deltaTime;
            // Time controls
            currentTime += delta;
            
            // Change the sun angle at an increased rate
            rotateSun.currentTime = sunRotationSpeed * currentTime;

            if (!busy){
                TakePicture(takePictureInterval);
            }
        }
    }

    void TakePicture(float seconds){
        busy = true;
        
        Debug.Log("Taking screenshot at time: " + Time.time);

        MoveCameraRandomly();
        sunLight.colorTemperature = UnityEngine.Random.Range(5500f, 8500f);
        sunLight.intensity = UnityEngine.Random.Range(30000f, 90000f);
        
        screenShot.TakeScreenShot();
        timesPicturesWereTakenAt.Add((int) currentTime);
        Pause(seconds);
        busy = false;
    }

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
        cam.transform.SetPositionAndRotation(initialCameraPosition, initialCameraRotation);
        rotateSun.controlsOwnOrbit = initialSunControlsOwnOrbit;
        rotateSun.currentTime = initialSunCurrentTime;
        orbitScanning = false;
        sunLight.colorTemperature = initialSunLightTemperature;
        sunLight.intensity = initialSunLightIntensity;
    }

    public void ToggleOrbiting(){
        if (orbitScanning){
            EndOrbiting();
        } else {
            BeginOrbiting();
        }
    }

    private void MoveCameraRandomly(){
        Vector3 center = GetCameraOrbitFocus();
        Vector3 bound0 = GetCameraOrbitBounds()[0];
        Vector3 bound1 = GetCameraOrbitBounds()[1];

        float x_diff = bound0.x - bound1.x;
        float z_diff = bound0.z - bound1.z;
        float pos_variance = 0.4f; // How much the camera can move around within the orbit bounds

        float x_pos = cameraOrbitFocus.x + UnityEngine.Random.Range(-pos_variance * x_diff, pos_variance * x_diff);
        float y_pos = cameraOrbitFocus.y + UnityEngine.Random.Range(6f, 9f); // Wheat is 6 units tall
        float z_pos = cameraOrbitFocus.z + UnityEngine.Random.Range(-pos_variance * z_diff, pos_variance * z_diff);

        Vector3 cam_pos = new Vector3(x_pos, y_pos, z_pos);
        
        // Rotates the camera to face the center of the wheat without changing camera height
        // Vector3 lookpos = center - cam_pos;
        // lookpos.y = 0;
        // Quaternion rotation = Quaternion.LookRotation(lookpos);

        cam.transform.SetPositionAndRotation(cam_pos, cam.transform.rotation);
        cam.transform.LookAt(center);
        
        // Lower camera angle if its angle is too high and might see background
        if (cam.transform.rotation.x < 40f){
            cam.transform.Rotate(40f - cam.transform.rotation.x, 0f, 0f);
        }
    }
}
