using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public class LeafesVFXController : MonoBehaviour
{

    private VisualEffect _vfx;
    private bool _restart = false;
    private int _restartFrame = 0;


    void Start()
    {
        _vfx = GetComponent<VisualEffect>();    
    }

    private void LateUpdate()
    {
        if (_restart)
        {
            if(_restartFrame >= 1)
            {
                _vfx.SetBool("Restart", false);
                _restart = false;

            }
            else
            {
                _restartFrame++;
            }
        }
    }

    public void Grow()
    {
        _restart = true;
        _restartFrame = 0;
        _vfx.SetBool("Restart", true);
        _vfx.SetBool("Fall", false);
        _vfx.Stop();
        _vfx.Play();
    }

    public void Fall()
    {
        _vfx.SetBool("Fall", true);
    }
}
