using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGround : MonoBehaviour
{
    public string groundType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name.Equals("Terrain"))
        {
            groundType = "dirt";
        }
        else if (other.name.Equals("WOTER"))
        {
            groundType = "water";
        }
        else
        {
            groundType = "nu";
        }
    }
}
