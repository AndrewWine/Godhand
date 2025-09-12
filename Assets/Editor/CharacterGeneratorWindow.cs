using UnityEngine;
using UnityEditor;
using System.IO;

public class CharacterGeneratorWindow : EditorWindow
{
    private GameObject basePrefab; 
    private string modelFolder = "Assets/Resources/Characters_Presets";
    private string traitsFolder = "Assets/Resources/Traits";

    private enum CharacterType { Custom, Human, Mob }
    private CharacterType selectedType = CharacterType.Custom;

    [MenuItem("Tools/Character Generator")]
    public static void ShowWindow()
    {
        GetWindow<CharacterGeneratorWindow>("Character Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Base Prefab", EditorStyles.boldLabel);
        selectedType = (CharacterType)EditorGUILayout.EnumPopup("Character Type", selectedType);

        if (selectedType == CharacterType.Human)
        {
            basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Human.prefab");
            EditorGUILayout.LabelField("Using prefab:", "Assets/Prefabs/Human.prefab");
        }
        else if (selectedType == CharacterType.Mob)
        {
            basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Mob.prefab");
            EditorGUILayout.LabelField("Using prefab:", "Assets/Prefabs/Mob/Mob.prefab");
        }
        else
        {
            basePrefab = (GameObject)EditorGUILayout.ObjectField("Template Prefab", basePrefab, typeof(GameObject), false);
        }

        GUILayout.Space(10);
        modelFolder = EditorGUILayout.TextField("Models Folder", modelFolder);
        traitsFolder = EditorGUILayout.TextField("Traits Folder", traitsFolder);

        GUILayout.Space(20);
        if (GUILayout.Button("Generate Character"))
        {
            GenerateCharacter();
        }
    }

    private void GenerateCharacter()
    {
        if (basePrefab == null)
        {
            Debug.LogError(" Chưa chọn prefab gốc!");
            return;
        }

        // 1. Clone prefab gốc
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);

        // 2. Random model
        GameObject randomModel = LoadRandomPrefab(modelFolder);
        GameObject modelInstance = null;
        if (randomModel != null)
        {
            modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(randomModel, instance.transform);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
        }

        // 3. Random class
        CharacterClass[] availableClasses = { CharacterClass.Warrior, CharacterClass.Mage, CharacterClass.Assassin };
        CharacterClass randomClass = availableClasses[Random.Range(0, availableClasses.Length)];

        //  Map Role theo class
        Role roleForClass = Role.Tank;
        switch (randomClass)
        {
            case CharacterClass.Warrior:
                roleForClass = Role.Tank;
                break;
            case CharacterClass.Assassin:
                roleForClass = Role.MeleeDPS;
                break;
            case CharacterClass.Mage:
                roleForClass = Role.Mage;
                break;
        }

        // 4. Random trait
        Trait randomTrait = LoadRandomTrait(traitsFolder);

        // 5. Random Personality
        Personality[] personalities = (Personality[])System.Enum.GetValues(typeof(Personality));
        Personality randomPersonality = personalities[Random.Range(0, personalities.Length)];

        // 6. Tạo CharacterData SO
        string dataFolder = "Assets/Data";
        if (!Directory.Exists(dataFolder)) Directory.CreateDirectory(dataFolder);

        string dataBaseName = $"SO_{instance.name}_{randomClass}";
        string dataPath = GetUniquePath(dataFolder, dataBaseName, ".asset");

        CharacterData charData = ScriptableObject.CreateInstance<CharacterData>();
        charData.characterName = $"{instance.name}_{randomClass}";
        charData.classChar = randomClass;
        charData.role = roleForClass;                     //  set role
        charData.personality = randomPersonality;         //  set personality

        AssetDatabase.CreateAsset(charData, dataPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[CharacterGenerator]  Created CharacterData SO: {dataPath} | Class: {randomClass}, Role: {roleForClass}, Personality: {randomPersonality}");

        // 7. Gán vào script Character
        Character charScript = instance.GetComponent<Character>();
        if (charScript != null)
        {
            SerializedObject so = new SerializedObject(charScript);
            so.FindProperty("characterData").objectReferenceValue = charData;
            if (randomTrait != null)
                so.FindProperty("trait").objectReferenceValue = randomTrait;
            so.ApplyModifiedProperties();
        }

        // 8. Attach vũ khí
        if (modelInstance != null)
        {
            AttachWeapon(modelInstance.transform, randomClass, "RightHandWp", "Hand_R");
            AttachWeapon(modelInstance.transform, randomClass, "LeftHandWp", "Hand_L");
        }

        // 9. Lưu prefab
        string prefabFolder = "Assets/Prefabs/GeneratedCharacters";
        if (!Directory.Exists(prefabFolder)) Directory.CreateDirectory(prefabFolder);

        string prefabBaseName = $"{instance.name}_{randomClass}";
        string prefabPath = GetUniquePath(prefabFolder, prefabBaseName, ".prefab");

        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
        DestroyImmediate(instance);

        Debug.Log($"[CharacterGenerator]  Saved Prefab: {prefabPath}");
    }


    // === Utilities ===
    private GameObject LoadRandomPrefab(string folder)
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { folder });
        if (guids.Length == 0) return null;
        string path = AssetDatabase.GUIDToAssetPath(guids[Random.Range(0, guids.Length)]);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    private Trait LoadRandomTrait(string folder)
    {
        string[] guids = AssetDatabase.FindAssets("t:Trait", new[] { folder });
        if (guids.Length == 0) return null;
        string path = AssetDatabase.GUIDToAssetPath(guids[Random.Range(0, guids.Length)]);
        return AssetDatabase.LoadAssetAtPath<Trait>(path);
    }

    private string GetUniquePath(string folder, string baseName, string extension)
    {
        string path = $"{folder}/{baseName}{extension}";
        int counter = 1;
        while (File.Exists(path))
        {
            path = $"{folder}/{baseName}_{counter}{extension}";
            counter++;
        }
        return path;
    }

    private void AttachWeapon(Transform modelRoot, CharacterClass classChar, string weaponFolderName, string handType)
    {
        string path = $"Assets/Resources/Weapons/{classChar}/{weaponFolderName}";
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { path });
        if (guids.Length == 0)
        {
            Debug.LogWarning($"[CharacterGenerator]  Không tìm thấy prefab weapon trong {path}");
            return;
        }

        string weaponPath = AssetDatabase.GUIDToAssetPath(guids[Random.Range(0, guids.Length)]);
        GameObject weaponPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(weaponPath);

        // chính xác bone path
        string bonePath = handType == "Hand_R"
            ? "Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_R/Shoulder_R/Elbow_R/Hand_R/weaponSlot"
            : "Root/Hips/Spine_01/Spine_02/Spine_03/Clavicle_L/Shoulder_L/Elbow_L/Hand_L/weaponSlot";

        Transform hand = modelRoot.Find(bonePath);
        if (hand == null)
        {
            Debug.LogWarning($"[CharacterGenerator]  Không tìm thấy bone {bonePath} trong {modelRoot.name}");
            return;
        }

        if (weaponPrefab != null)
        {
            GameObject wp = Object.Instantiate(weaponPrefab, hand);

            // default
            wp.transform.localPosition = Vector3.zero;
            wp.transform.localRotation = Quaternion.identity;
            wp.transform.localScale = new Vector3(100f, 100f, 100f);

            // Nếu là Warrior và tay trái thì ép transform theo cấu hình shield
            if (classChar == CharacterClass.Warrior && handType == "Hand_L")
            {
                wp.transform.localPosition = Vector3.zero;    // shield bám thẳng vào tay
                wp.transform.localRotation = Quaternion.Euler(-90f, -140f, -35f);
                wp.transform.localScale = new Vector3(100f, 100f, 100f);
            }

            // Nếu là Right hand (tất cả class) thì ép transform theo cấu hình axe/sword
            if (handType == "Hand_R")
            {
                wp.transform.localPosition = new Vector3(9f, 4.5f, 0f);
                wp.transform.localRotation = Quaternion.Euler(0f, 90f, -90f);
                wp.transform.localScale = new Vector3(100f, 100f, 100f);
            }

            Debug.Log($"[CharacterGenerator] Weapon '{wp.name}' gắn vào {bonePath} cho class {classChar}, hand {handType}");
        }

    }
}
