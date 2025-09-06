using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenTemplateController : MonoBehaviour, IKeyBack
{
    public const string NAME = "ScreenTemplate";

    public void OnKeyBack()
    {
        ScreenManager.Close();
    }
}