using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGrowController : MonoBehaviour
{
    public Transform Leafes;
    public ParticleSystem[] Particles;



    public void Start()
    {
        Ungrow();
    }

    public void Grow()
    {
        foreach(ParticleSystem particle in Particles)
        {
            if (particle.isPlaying)
                particle.Stop();

            particle.Play();
        }

        Leafes.gameObject.SetActive(true);
    }

    public void Ungrow()
    {
        Leafes.gameObject.SetActive(false);
    }
}
