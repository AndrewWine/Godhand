using UnityEngine;
using UnityEditor;
using System.IO;

public class AddWeaponSlotTool
{
    [MenuItem("Tools/Characters/Add Weapon Slots To Prefabs")]
    private static void AddWeaponSlotsToPrefabs()
    {
        string folderPath = "Assets/Resources/Characters_Presets";
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        foreach (string guid in prefabGUIDs)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab == null) continue;

            // mở prefab để edit
            GameObject instance = PrefabUtility.LoadPrefabContents(assetPath);

            // add slot cho tay phải
            AddWeaponSlot(instance.transform,
                "Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_R/Shoulder_R/Elbow_R/Hand_R",
                "weaponSlot");

            // add slot cho tay trái
            AddWeaponSlot(instance.transform,
                "Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_L/Shoulder_L/Elbow_L/Hand_L",
                "weaponSlot");

            // lưu lại prefab
            PrefabUtility.SaveAsPrefabAsset(instance, assetPath);
            PrefabUtility.UnloadPrefabContents(instance);

            Debug.Log($"[AddWeaponSlotTool] Đã thêm weaponSlot vào prefab {assetPath}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void AddWeaponSlot(Transform root, string bonePath, string slotName)
    {
        Transform hand = root.Find(bonePath);
        if (hand == null)
        {
            Debug.LogWarning($"[AddWeaponSlotTool] Không tìm thấy bone {bonePath}");
            return;
        }

        Transform exist = hand.Find(slotName);
        if (exist != null) return; // đã có rồi thì bỏ qua

        GameObject slot = new GameObject(slotName);
        slot.transform.SetParent(hand, false); // giữ local pos/rot = 0
        slot.transform.localPosition = Vector3.zero;
        slot.transform.localRotation = Quaternion.identity;
        slot.transform.localScale = Vector3.one;
    }
}
