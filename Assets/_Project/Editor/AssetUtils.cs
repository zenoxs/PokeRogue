using System.IO;
using UnityEngine;

public class AssetUtils
{
    public static string GetAssetRelativePath(string path) => Path.Join("Assets", Path.GetRelativePath(Application.dataPath, path));
}