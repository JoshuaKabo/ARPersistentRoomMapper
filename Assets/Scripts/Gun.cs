using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Gun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f; //Gun's firing distance
    public Transform muzzle; //Where the bullet comes out of

    // come back to hit particle
    // public GameObject impactParticle;

    private VisualEffect muzzleFlash;

    public float impactForce = 60f;
    public float fireRate = 15f;
    private float nextTimeToFire = 0f;



    private void Awake()
    {
        muzzleFlash = GetComponentInChildren<VisualEffect>();
    }

    // this will be called by interactible's reference
    public void fire()
    {
        if (Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            muzzleFlash.Play();
            handle_hit();
        }
    }

    private void handle_hit()
    {
        RaycastHit hit;
        if (Physics.Raycast(muzzle.transform.position, muzzle.transform.forward, out hit, range))
        {
            HP target = hit.transform.GetComponent<HP>();
            if (target != null)
            {// if that wasn't a wall or something...
                target.TakeDamage(34f);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }

            // Hit particle... Come back to this...
            // GameObject instace_impactParticle = Instantiate(impactParticle, hit.point, Quaternion.LookRotation(hit.normal));
            // Destroy(instace_impactParticle, 2f);
        }
    }
}
