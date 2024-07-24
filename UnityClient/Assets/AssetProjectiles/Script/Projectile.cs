using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetProjectiles
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField]
        public MouseRotation mouseRotation;

        public List<GameObject> fxPrefabs;
        public List<GameObject> areaPrefabs;
        public GameObject StartPoint;
        private int currentPrefabIndex = 0;
        private int currentAreaIndex = 0;
        private float fireRate = 1f;
        private bool isFiring = false;
        private GameObject currentAreaInstance;

        void Start()
        {
            if (fxPrefabs != null && fxPrefabs.Count > 0)
            {
                currentPrefabIndex = 0;
                fireRate = fxPrefabs[currentPrefabIndex].GetComponent<ProjectileMoving>().fireRate;
            }
            else
            {
                Debug.LogWarning("FX Prefabs list is empty or null.");
            }

            if (areaPrefabs == null || areaPrefabs.Count == 0)
            {
                Debug.LogWarning("Area prefabs list is empty or null.");
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0) && !isFiring)
            {
                StartCoroutine(FireContinuously());
            }

            if (Input.GetMouseButtonUp(0))
            {
                isFiring = false;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeFxPrefab();
            }

            if (Input.GetKeyDown(KeyCode.Tab)) 
            {
                ChangeAreaPrefab();
                SpawnAreaPrefab();
            }
        }

        IEnumerator FireContinuously()
        {
            isFiring = true;
            while (isFiring)
            {
                SpawnFx();
                yield return new WaitForSeconds(1f / fireRate);
            }
        }

        IEnumerator DestroyFxAfterDelay(GameObject fxInstance)
        {
            yield return new WaitForSeconds(2f);
            Destroy(fxInstance);
        }

        void SpawnFx()
        {
            if (StartPoint != null && fxPrefabs != null && fxPrefabs.Count > 0)
            {
                GameObject fxInstance = Instantiate(fxPrefabs[currentPrefabIndex], StartPoint.transform.position, Quaternion.identity);
                if (mouseRotation != null)
                {
                    fxInstance.transform.localRotation = mouseRotation.GetRotation();
                }

                StartCoroutine(DestroyFxAfterDelay(fxInstance));
            }
            else
            {
                Debug.Log("StartPoint is null or FX Prefabs list is empty.");
            }
        }

        void ChangeFxPrefab()
        {
            if (fxPrefabs != null && fxPrefabs.Count > 0)
            {
                currentPrefabIndex = (currentPrefabIndex + 1) % fxPrefabs.Count;
                Debug.Log("Changed FX prefab to: " + fxPrefabs[currentPrefabIndex].name);
                fireRate = fxPrefabs[currentPrefabIndex].GetComponent<ProjectileMoving>().fireRate;
            }
            else
            {
                Debug.LogWarning("FX Prefabs list is empty or null.");
            }
        }

        void ChangeAreaPrefab()
        {
            if (areaPrefabs != null && areaPrefabs.Count > 0)
            {
                currentAreaIndex = (currentAreaIndex + 1) % areaPrefabs.Count;
                Debug.Log("Changed area prefab to: " + areaPrefabs[currentAreaIndex].name);
            }
            else
            {
                Debug.LogWarning("Area prefabs list is empty or null.");
            }
        }

        void SpawnAreaPrefab()
        {
            if (currentAreaInstance != null)
            {
                Destroy(currentAreaInstance);
            }

            if (areaPrefabs != null && areaPrefabs.Count > 0)
            {
                currentAreaInstance = Instantiate(areaPrefabs[currentAreaIndex], Vector3.zero, Quaternion.identity);
                Debug.Log("Spawned area prefab: " + areaPrefabs[currentAreaIndex].name);
            }
            else
            {
                Debug.LogWarning("Area prefabs list is empty or null.");
            }
        }
    }
}













