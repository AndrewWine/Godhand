using UnityEngine;

public class NoLevelSystem : MonoBehaviour, IProgression
{
    public int Level => 1;
    public int MaxLevel => 1;

    public int TotalExp => 0;
    public int ExpRequiredThisLevel => 0;
    public int ExpInCurrentLevel => 0;
    public float Progress01 => 1f;

    public void AddExp(int amount) { /* do nothing */ }
}
