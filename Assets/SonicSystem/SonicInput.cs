using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;




/// <summary>
///  Based on audio we invoke whichever action this is set to invoke.
///  ANything +='s with that action will be called.
/// 
/// </summary>
public class SonicInput : SonicSystem
{

    public float speed = 10f;

    //public Enums.SonicInput actionToUpdate = Enums.SonicInput.None; // set and forget in insepctor

    // public int enumInt = 0;




    private void Update()
    {
        enumIndex = (int)sonicInputType;
        value += Time.deltaTime * speed;

        if (value > 1f)
        {
            value = 0f;
        }
        //Debug.Log(this.name + " will Invoke " + player.ID.events[enumInt]);
        player.ID.events[enumIndex]?.Invoke(value);


    }
}