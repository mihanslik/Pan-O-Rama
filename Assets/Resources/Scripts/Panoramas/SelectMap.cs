using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class SelectMap : MonoBehaviour
{
    public MapTag[] Maps;
    private MapVariant _lastActive;

    public void ActivateMap(MapVariant variant)
    {
        foreach(MapTag map in Maps)
        {
            if(map.Variant == variant)
            {
                map.gameObject.SetActive(true);
            }
            else
            {
                map.gameObject.SetActive(false);
            }
        }

        _lastActive = variant;
    }
}
