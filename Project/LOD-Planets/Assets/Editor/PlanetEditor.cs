using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Planet planet = (Planet)target;

        if (GUILayout.Button("Generate Entire Planet"))
        {
            if(RequireQuadTemplate()) {
                planet.distanceToPlayer = planet.Size + 3000;
                planet.distanceToPlayerPow2 = planet.distanceToPlayer * planet.distanceToPlayer;
                planet.PlayerPos = Vector3.zero;
                planet.position = planet.transform.position;
                planet.Initialize();
                planet.GenerateMesh();
                planet.GenerateTexture();
                planet.UpdateShaders();
                planet.CachedPlanet = planet.CachePlanet();
            }
        }

        /// <summary>
        /// Returns true if quad templates have been generated and false if they have not, along with logging a warning.
        /// </summary>
        bool RequireQuadTemplate() {
            if(!Presets.Generated) {
                foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))
                {
                    if(obj.GetComponent<Presets>() != null) { 
                        Debug.LogWarning("QUAD TEMPLATE MISSING. Generate one by pressing the button on the Presets component.");
                        EditorGUIUtility.PingObject(obj);
                        break;
                    };
                }
            }

            return Presets.Generated;
        }
    }
}
