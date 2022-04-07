using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    [SerializeField]
    private int numOverlap = 0;
    void OnTriggerEnter2D(){
        numOverlap++;
    }

    void OnTriggerExit2D(){
        numOverlap--;
    }

    public bool isPlaceable(){
        return (numOverlap == 0);
    }
}
