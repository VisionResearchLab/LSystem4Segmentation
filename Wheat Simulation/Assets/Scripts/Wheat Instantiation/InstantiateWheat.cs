using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class InstantiateWheat : MonoBehaviour
{
    public static InstantiateWheat IW;
    public MassAddWheat massAddWheat;
    
    // When the wheat is randomly generated, it will be shifted downward and rotated slightly.
    public float downTranslationMin = 0.1f;
    public float downTranslationMax = 0.75f;
    public float xRotationMax = 15f;
    public float yRotationMax = 360f;
    public float zRotationMax = 15f;

    // Parent object of the instantiated wheats
    public Transform parent;

    void Awake(){
        if (IW != null && IW != this){
            GameObject.Destroy(IW);
        } else {
            IW = this;
        }

        DontDestroyOnLoad(this);
    }

    public void GenerateWheat(Vector3 requestedPosition, bool tryPlaceAgainIfFail){
        GameObject[] wheatPrefabs = Wheat.GetAllWheatPrefabs();
        int numberOfWheatPrefabs = wheatPrefabs.Length;
        if (numberOfWheatPrefabs > 0){

            int chosenWheatPrefabIndex = Random.Range(0, numberOfWheatPrefabs-1);
            GameObject wheatPrefab = wheatPrefabs[chosenWheatPrefabIndex];

            bool placed = false;

            // Try to instantiate the wheat in a position that does not overlap with other wheat objects.
            if (tryPlaceAgainIfFail){ // For MassAddWheat
                int placeAttempts = 0;
                while (placed == false && placeAttempts < 5){
                    requestedPosition = massAddWheat.GetPositionInWheatBounds();
                    // The wheat will be placed at a slightly lower position than requested, as wheat grows partly inside of the ground.
                    Vector3 position = TryFindPosition(requestedPosition);

                    // The wheat is randomly rotated within given bounds
                    Quaternion rotation = TryFindRotation();

                    // Instantiate the wheat and set its parent to the given parent object
                    GameObject newWheat = Instantiate(wheatPrefab, position, rotation);
                    newWheat.transform.SetParent(parent);

                    foreach (WheatData wheatData in newWheat.GetComponentsInChildren<WheatData>()){
                        Debug.Log("Checking for overlaps");
                        if (wheatData.IsOverlappingWheat()){
                            // If the position was occupied, try again somewhere else, up to 5 times
                            Debug.Log("Could not place wheat");
                            placeAttempts ++;
                            Destroy(newWheat);
                        }
                        else 
                        {
                            Debug.Log("Placed wheat");
                            placed = true;
                        }
                    }
                }
            } 
            else // for PlaceWheat
            {
                // The wheat will be placed at a slightly lower position than requested, as wheat grows partly inside of the ground.
                Vector3 position = TryFindPosition(requestedPosition);

                // The wheat is randomly rotated within given bounds
                Quaternion rotation = TryFindRotation();

                // Instantiate the wheat and set its parent to the given parent object
                GameObject newWheat = Instantiate(wheatPrefab, position, rotation);
                newWheat.transform.SetParent(parent);

                foreach (WheatData wheatData in newWheat.GetComponentsInChildren<WheatData>()){
                    if (wheatData.IsOverlappingWheat()){
                        Destroy(newWheat);
                    }
                }
            }
            // Error message if no available space was found
            if (!placed){
                Debug.Log("Error: Could not find space for the wheat object!");
            }

        } else {
            Debug.Log("Error: No wheat prefabs found!");
        }
        
    }

    private Vector3 TryFindPosition(Vector3 requestedPosition){
        float downTranslation = Random.Range(downTranslationMin, downTranslationMax);
        Vector3 position = requestedPosition - new Vector3(0, downTranslation, 0);
        return position;
    }

    private Quaternion TryFindRotation(){
        float xRotation = Random.Range(-xRotationMax, xRotationMax);
        float yRotation = Random.Range(-yRotationMax, yRotationMax);
        float zRotation = Random.Range(-zRotationMax, zRotationMax);
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
        return rotation;
    }
}
