using System;
using System.Collections;
using System.Collections.Generic;
using GameLogic.UI;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUILayout.Button("OPen"))
        {
            GameApp.UI.ShowUI<UIHomeWindow>();
        }
    }
}
