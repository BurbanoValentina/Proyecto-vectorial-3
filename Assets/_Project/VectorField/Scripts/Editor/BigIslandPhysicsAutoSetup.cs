using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace VectorFieldTools.Editor
{
    [InitializeOnLoad]
    public static class BigIslandPhysicsAutoSetup
    {
        private const string SceneName = "BigIsland";

        static BigIslandPhysicsAutoSetup()
        {
            EditorApplication.delayCall += TrySetup;
            EditorSceneManager.sceneOpened += (_, __) => EditorApplication.delayCall += TrySetup;
        }

        private static void TrySetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.isLoaded)
                return;

            if (!string.Equals(scene.name, SceneName, StringComparison.OrdinalIgnoreCase))
                return;

            var vectorField = UnityEngine.Object.FindAnyObjectByType<VectorFieldRuntimeController>();
            if (vectorField == null)
                return; // el bootstrapper lo crea primero

            bool changed = false;

            // 1) Barcos: Rigidbody + BoatVectorFollower
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t == null) continue;

                    // Heurística: objetos "Boat" o "Boat (n)"
                    if (!t.name.StartsWith("Boat", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Evitar tocar hijos internos del modelo si ya hay un controlador en un padre
                    if (t.GetComponentInParent<BoatVectorFollower>() != null)
                        continue;

                    changed |= EnsureBoatPhysics(t.gameObject, vectorField);
                }
            }

            // 2) Muelle/Dock: asegurar collider (estático) y layer de obstáculo si existe
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t == null) continue;

                    if (!LooksLikeDock(t.name))
                        continue;

                    changed |= EnsureStaticCollider(t.gameObject);
                    changed |= EnsureObstacleLayer(t.gameObject);
                }
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }

        private static bool EnsureBoatPhysics(GameObject boatGo, VectorFieldRuntimeController vectorField)
        {
            bool changed = false;

            // Rigidbody
            var rb = boatGo.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = boatGo.AddComponent<Rigidbody>();
                changed = true;
            }

            if (rb != null)
            {
                if (rb.useGravity)
                {
                    rb.useGravity = false;
                    changed = true;
                }

                // Mantener el barco estable (solo XZ + rotación Y)
                var desired = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                if (rb.constraints != desired)
                {
                    rb.constraints = desired;
                    changed = true;
                }
            }

            // Collider
            if (boatGo.GetComponent<Collider>() == null)
            {
                boatGo.AddComponent<BoxCollider>();
                changed = true;
            }

            // Seguir vector field
            var follower = boatGo.GetComponent<BoatVectorFollower>();
            if (follower == null)
            {
                follower = boatGo.AddComponent<BoatVectorFollower>();
                changed = true;
            }

            if (follower != null)
            {
                if (follower.vectorField == null)
                {
                    follower.vectorField = vectorField;
                    changed = true;
                }
                follower.usePhysics = true;
                follower.autoMove = true;
            }

            if (changed)
            {
                EditorUtility.SetDirty(boatGo);
                if (rb != null) EditorUtility.SetDirty(rb);
                if (follower != null) EditorUtility.SetDirty(follower);
            }

            return changed;
        }

        private static bool EnsureStaticCollider(GameObject go)
        {
            if (go.GetComponent<Collider>() != null)
                return false;

            // Preferir MeshCollider si hay mesh
            var mf = go.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                var mc = go.AddComponent<MeshCollider>();
                mc.convex = false;
                EditorUtility.SetDirty(mc);
                EditorUtility.SetDirty(go);
                return true;
            }

            var bc = go.AddComponent<BoxCollider>();
            EditorUtility.SetDirty(bc);
            EditorUtility.SetDirty(go);
            return true;
        }

        private static bool EnsureObstacleLayer(GameObject go)
        {
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");
            if (obstacleLayer == -1)
                return false;

            if (go.layer == obstacleLayer)
                return false;

            go.layer = obstacleLayer;
            EditorUtility.SetDirty(go);
            return true;
        }

        private static bool LooksLikeDock(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            name = name.ToLowerInvariant();
            return name.Contains("dock") || name.Contains("muelle") || name.Contains("pier") || name.Contains("jetty") || name.Contains("wharf");
        }
    }
}
