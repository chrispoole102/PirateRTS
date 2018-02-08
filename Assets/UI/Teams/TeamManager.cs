using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour//Only used in server
{
    public static Color[] teams = {
        new Color(10f/255f,43f/255f,125f/255f),//blue
        new Color(125f/255f,10f/255f,10f/255f),//red
        new Color(25f/255f,125f/255f,10f/255f),//green
        new Color(113f/255f,10f/255f,125f/255f),//purple
        new Color(206f/255f, 131f/255f,0),//orange
        new Color(199f/255f,206f/255f,0)//yellow
    };
    public static bool[] teamTaken = new bool[6];
}
