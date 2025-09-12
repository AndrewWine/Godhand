using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManagerController : MonoBehaviour, IKeyBack
{
    public const string NAME = "BattleManager";

    public void OnKeyBack()
    {
        ScreenManager.Close();
    }
}