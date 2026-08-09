using System;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

public class DataManager : OdinMenuEditorWindow
{
    private static Type[] typesToDisplay = TypeCache.GetTypesWithAttribute<ManageableDataAttribute>()
        .OrderBy(m => m.Name)
        .ToArray();
    
    private Type selectedType;

    [MenuItem("Tools/Data Manager")]
    private static void OpenEditor() => GetWindow<DataManager>();

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        foreach (var type in typesToDisplay)
        {
            tree.AddAllAssetsAtPath(type.Name, "Assets/", type, true, true);
        }

        return tree;
    }
}