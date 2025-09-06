using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScreensController : MonoBehaviour, IKeyBack
{
    public const string NAME = "TestScreens";

    public void OnKeyBack()
    {
        ScreenManager.Close();
    }
}