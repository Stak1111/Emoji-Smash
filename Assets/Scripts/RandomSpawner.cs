using UnityEngine;
using System.Collections.Generic;

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawn Area (world space)")]
    public Vector2 minBounds;   // bottom-left corner
    public Vector2 maxBounds;   // top-right corner

    [Header("Units")]
    public GameObject[] players; // up to 4 players (assign manually or via UI script)
    public GameObject[] enemies; // up to 8 enemies

    [Header("Settings")]
    public float minDistance = 1.5f; // minimum distance apart to avoid overlap
    public int maxAttempts = 50;     // tries before giving up on a position

    void Start()
    {
        // Make a list of all objects that need placement
        List<GameObject> allObjects = new List<GameObject>();

        foreach (var p in players)
        {
            if (p != null) allObjects.Add(p);
        }
        foreach (var e in enemies)
        {
            if (e != null) allObjects.Add(e);
        }

        PlaceObjectsRandomly(allObjects);
    }

    void PlaceObjectsRandomly(List<GameObject> objects)
    {
        List<Vector3> placedPositions = new List<Vector3>();

        foreach (GameObject obj in objects)
        {
            if (obj == null) continue;

            bool placed = false;
            int attempts = 0;

            while (!placed && attempts < maxAttempts)
            {
                attempts++;

                // Pick random position
                Vector3 candidate = new Vector3(
                    Random.Range(minBounds.x, maxBounds.x),
                    Random.Range(minBounds.y, maxBounds.y),
                    obj.transform.position.z
                );

                // Check distance from all already-placed objects
                bool tooClose = false;
                foreach (Vector3 pos in placedPositions)
                {
                    if (Vector3.Distance(candidate, pos) < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    obj.transform.position = candidate;
                    placedPositions.Add(candidate);
                    placed = true;
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"{obj.name} could not find a valid spawn location!");
            }
        }
    }
}
