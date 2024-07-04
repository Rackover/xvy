using UnityEngine;
using System.Collections;
using UnityEditor;

public class PlaceRandomAround : MonoBehaviour {

#if UNITY_EDITOR
    [MenuItem("Tools/Duplicate randomly")]
    public static void DuplicateRandomly()
    {
        if (Selection.activeGameObject)
        {
            float maxRange = 1000;
            float distanceUnit = 60f;

            for (float distance = distanceUnit; distance < maxRange; distance+= distanceUnit)
            {
                float max = distance / 30;
                float randomOffset = Random.value;
                for (int i = 0; i < max; i++)
                {
                    GameObject inst = GameObject.Instantiate(Selection.activeGameObject);
                    inst.transform.parent = Selection.activeTransform.parent;
                    inst.transform.localPosition = Selection.activeTransform.localPosition + 
                        new Vector3(Mathf.Sin((i / max + randomOffset) * Mathf.PI * 2f), 0f, Mathf.Cos((i / max + randomOffset) * Mathf.PI * 2f)) * distance;

                    Undo.RegisterCreatedObjectUndo(inst, "creating " + inst.name);
                }
            }
        }
    }
#endif
}
