﻿using UnityEngine;
using System.Collections;

public class UniformSwap : Powerup
{

    /* not finished
       not needed until other game types
     * */

    public override bool Apply(Player player)
    {
        if (player.otherPlayer != null)
        {
            //need to make it so player models switch also
            //player prefab needs to be setup for this to work smoothly.

            string tempName = player.name.text;
            player.name.text = player.otherPlayer.name.text;
            player.otherPlayer.name.text = tempName;
            return true;
        }
        return false;
    }
}
