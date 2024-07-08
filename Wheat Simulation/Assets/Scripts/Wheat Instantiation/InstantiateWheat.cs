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

    // Number of times to attempt placing in a unique position before just placing the wheat
    private int numberOfPlaceAttempts = 3;

    void Awake(){
        if (IW != null && IW != this){
            GameObject.Destroy(IW);
        } else {
            IW = this;
        }

        DontDestroyOnLoad(this);
    }

public void GenerateWheat(Vector3 requestedPosition, bool tryPlaceAgainIfFail)
{
    GameObject[] wheatPrefabs = Wheat.GetAllWheatPrefabs();
    int numberOfWheatPrefabs = wheatPrefabs.Length;
    if (numberOfWheatPrefabs > 0)
    {
        int chosenWheatPrefabIndex = Random.Range(0, numberOfWheatPrefabs - 1);
        GameObject wheatPrefab = wheatPrefabs[chosenWheatPrefabIndex];

        bool placed = false;

        if (tryPlaceAgainIfFail)
        {
            int placeAttempts = 0;
            while (placed == false)
            {
                placeAttempts++;
                requestedPosition = massAddWheat.GetPositionInWheatBounds();
                Vector3 position = TryFindPosition(requestedPosition);
                Quaternion rotation = TryFindRotation();

                GameObject newWheat = ObjectPooler.Instance.SpawnFromPool(wheatPrefab.name, position, rotation);
                newWheat.transform.SetParent(parent);

                if (placeAttempts != numberOfPlaceAttempts)
                {
                    bool overlapping = false;
                    foreach (WheatData wheatData in newWheat.GetComponentsInChildren<WheatData>())
                    {
                        if (wheatData.IsOverlappingWheat())
                        {
                            overlapping = true;
                            newWheat.SetActive(false); // Instead of Destroy
                        }
                    }

                    if (!overlapping)
                    {
                        placed = true;
                    }
                }
                else
                {
                    placed = true;
                }
            }
        }
        else
        {
            Vector3 position = TryFindPosition(requestedPosition);
            Quaternion rotation = TryFindRotation();

            GameObject newWheat = ObjectPooler.Instance.SpawnFromPool(wheatPrefab.name, position, rotation);
            newWheat.transform.SetParent(parent);

            foreach (WheatData wheatData in newWheat.GetComponentsInChildren<WheatData>())
            {
                if (wheatData.IsOverlappingWheat())
                {
                    newWheat.SetActive(false); // Instead of Destroy
                }
            }
        }
    }
    else
    {
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
