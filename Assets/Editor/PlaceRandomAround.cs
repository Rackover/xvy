using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class PlaceRandomAround : MonoBehaviour {

#if UNITY_EDITOR
    [MenuItem("Tools/Duplicate randomly (tree map)")]
    public static void DuplicateRandomlyForTreeMap()
    {
        float maxRange = 1000;
        float distanceUnit = 60f;
        float maxPerDistance = 30f;
        DuplicateRandomly(maxRange, distanceUnit, maxPerDistance);
    }

    [MenuItem("Tools/Duplicate randomly (arecibo)")]
    public static void DuplicateRandomlyForArecibo()
    {
        DuplicateRandomly(1000f, 30f, 15f, 300f);
    }


    public static void DuplicateRandomly(float maxRange, float distanceUnit, float maxPerDistance, float startDistance = 0f)
    {
        if (Selection.activeGameObject)
        {
            System.Random rand = new System.Random();

            List<Object> selectionAfter = new List<Object>();

            for (float distance = startDistance + distanceUnit; distance < maxRange; distance += distanceUnit)
            {
                float max = distance / maxPerDistance;
                float randomOffset = (float)rand.NextDouble();
                for (int i = 0; i < max; i++)
                {
                    Object prefabRoot = PrefabUtility.GetPrefabParent(Selection.activeGameObject);

                    GameObject inst = PrefabUtility.InstantiatePrefab(prefabRoot) as GameObject;

                    inst.transform.parent = Selection.activeTransform.parent;
                    inst.transform.localPosition = Selection.activeTransform.localPosition +
                        new Vector3(Mathf.Sin((i / max + randomOffset) * Mathf.PI * 2f), 0f, Mathf.Cos((i / max + randomOffset) * Mathf.PI * 2f)) * distance;

                    Undo.RegisterCreatedObjectUndo(inst, "creating " + inst.name);
                    selectionAfter.Add(inst);
                }
            }

            Selection.objects = selectionAfter.ToArray();
        }
    }

    [MenuItem("Tools/Fall down on surface")]
    public static void FallOnSurface()
    {
        if (Selection.activeGameObject)
        {
            var objects = Selection.gameObjects;

            List<Object> selectionAfter = new List<Object>();

            for (int i = 0; i < objects.Length; i++)
            {
                Ray ray = new Ray(objects[i].transform.position, Vector3.down);
                RaycastHit[] hits = Physics.RaycastAll(ray);

                objects[i].name = "Pine tree";
                bool placed = false;

                if (hits.Length > 0)
                {
                    for (int j = 0; j < hits.Length; j++)
                    {
                        if (hits[j].collider.gameObject == objects[i])
                        {
                            continue;
                        }

                        if (hits[j].collider.gameObject.name != "Terrain")
                        {
                            continue;
                        }

                        objects[i].transform.position = hits[j].point;
                        objects[i].name = "Pine tree (placed on " + hits[j].collider.gameObject.name + ")";
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                {
                    objects[i].name = "misplaced";
                    selectionAfter.Add(objects[i]);
                }
            }

            Selection.objects = selectionAfter.ToArray();
        }
    }

    [MenuItem("Tools/Place in Grid")]
    public static void PlaceInGrid()
    {
        if (Selection.activeGameObject)
        {
            float gridInterval = 100f;


            var objects = Selection.gameObjects;

            int width = Mathf.CeilToInt(Mathf.Sqrt(objects.Length));
            int height = width;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    int index = x + z * width;

                    if (index >= objects.Length)
                    {
                        break;
                    }

                    objects[index].transform.localPosition = new Vector3(x * gridInterval, 0f , z * gridInterval);
                }
            }
        }
    }
#endif
}
