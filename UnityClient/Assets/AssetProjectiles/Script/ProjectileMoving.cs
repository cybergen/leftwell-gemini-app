using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetProjectiles
{
    public class ProjectileMoving : MonoBehaviour
    {
        public float speed;
        public GameObject muzzle;
        public GameObject hit;
        public float destroyDelay = 3f;
        public float fireRate;

        void Start()
        {
            if (muzzle != null)
            {
                var muzzleFx = Instantiate(muzzle, transform.position, Quaternion.identity);
                muzzleFx.transform.forward = gameObject.transform.forward;
                Destroy(muzzleFx, 3f);
            }
        }

        void Update()
        {
            if (speed != 0)
            {
                transform.position += transform.forward * (speed * Time.deltaTime);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            speed = 0;

            if (hit != null)
            {
                ContactPoint contact = collision.contacts[0];
                var hitFx = Instantiate(hit, contact.point, Quaternion.LookRotation(contact.normal));
                Destroy(hitFx, 3f);
            }

            StartCoroutine(DestroyProjectileAfterDelay());
        }

        IEnumerator DestroyProjectileAfterDelay()
        {
            yield return new WaitForSeconds(destroyDelay);
            Destroy(gameObject);
        }
    }
}

