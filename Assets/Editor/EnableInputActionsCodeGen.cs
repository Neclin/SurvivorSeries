using UnityEditor;
using UnityEngine;

public class EnableInputActionsCodeGen
{
    public static void Execute()
    {
        string[] guids = AssetDatabase.FindAssets("t:InputActionAsset");
        if (guids.Length == 0)
        {
            Debug.LogWarning("[CodeGen] No InputActionAsset found.");
            return;
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[CodeGen] Reimported: {path}");
        }

        AssetDatabase.Refresh();
        Debug.Log("[CodeGen] Done. Check Assets/ for InputSystem_Actions.cs");
    }
}