using System.Collections.Generic;
using UnityEngine;

public enum ExpCurveMode { Table, Formula }

[CreateAssetMenu(fileName = "ExperienceCurve", menuName = "Game/Experience Curve")]
public class ExperienceCurve : ScriptableObject
{
    [Header("General")]
    [Min(2)] public int maxLevel = 15;
    public ExpCurveMode mode = ExpCurveMode.Formula;

    [Header("Table Mode")]
    public List<int> expToNext = new List<int>(); 

    [Header("Formula Mode")]
    [Tooltip("EXP lan toi L -> L+1 = round(baseExp * pow(growth, L-1) * pow(L, power))")]
    public int baseExp = 100;
    [Range(1f, 2f)] public float growth = 1.15f;
    [Range(0f, 2f)] public float power = 1.0f;

    public int ExpRequiredForNextLevel(int level)
    {
        // level: 1..maxLevel-1
        level = Mathf.Clamp(level, 1, maxLevel - 1);
        if (mode == ExpCurveMode.Table)
        {
            if (expToNext == null || expToNext.Count < maxLevel - 1)
                return baseExp; // fallback
            return Mathf.Max(1, expToNext[level - 1]);
        }
        // Formula
        double v = baseExp * System.Math.Pow(growth, level - 1) * System.Math.Pow(level, power);
        return Mathf.Max(1, Mathf.RoundToInt((float)System.Math.Round(v)));
    }

    public int TotalExpToReachLevel(int targetLevel)
    {
        targetLevel = Mathf.Clamp(targetLevel, 1, maxLevel);
        int total = 0;
        for (int l = 1; l < targetLevel; l++)
            total += ExpRequiredForNextLevel(l);
        return total;
    }
}
