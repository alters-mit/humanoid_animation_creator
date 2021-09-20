using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;


/// <summary>
/// Create animation asset bundles and records.
/// </summary>
public static class AssetBundleCreator
{
    /// <summary>
    /// The directory of the assets bundles.
    /// </summary>
    private const string ASSET_BUNDLES_DIRECTORY = "AssetBundles";


    /// <summary>
    /// The targets and the corresponding record URL keys.
    /// </summary>
    private readonly static Dictionary<BuildTarget, string> targets = new Dictionary<BuildTarget, string>
    {
        { BuildTarget.StandaloneWindows64, "Windows" },
        { BuildTarget.StandaloneOSX, "Darwin" },
        { BuildTarget.StandaloneLinux64, "Linux" },
    };


    [MenuItem("TDW/Test")]
    public static void Test()
    {
        Create("idle_neutral_1");
    }


    /// <summary>
    /// Create asset bundles of an animation file.
    /// </summary>
    /// <param name="name">The name of the animation file.</param>
    public static void Create(string name)
    {
        AssetBundleBuild[] builds = new AssetBundleBuild[]
        {
            new AssetBundleBuild
            {
                assetBundleName = name,
                assetNames = new string[] { "Assets/Resources/Animations/" + name + ".anim" }
            }
        };
        // Create the directory for all asset bundles.
        string assetBundlesDirectory = Path.Combine(Application.dataPath, ASSET_BUNDLES_DIRECTORY, name);
        if (!Directory.Exists(assetBundlesDirectory))
        {
            Directory.CreateDirectory(assetBundlesDirectory);
        }
        AnimationClip clip = Resources.Load<AnimationClip>("Animations/" + name);
        // Create the record.
        string record = "{\"name\":\"" + clip.name + "\"," +
            "\"duration\":" + clip.length + "," +
            "\"loop\":" + (clip.isLooping ? "True" : "False") + "," +
            "\"framerate\":" + clip.frameRate + "," +
            "\"urls\":{";
        foreach (BuildTarget target in targets.Keys)
        {
            string path = "file:///" + Path.Combine(Application.dataPath, ASSET_BUNDLES_DIRECTORY,
                name, target.ToString(), name + ".anim");
            record += "\"" + targets[target] + "\":\"" + path + "\",";
        }
        record = record.Substring(0, record.Length - 1);
        record += "}}";
        record = record.Replace('\\', '/');
        // Write the record.
        File.WriteAllText(Path.Combine(Application.dataPath, ASSET_BUNDLES_DIRECTORY, clip.name, "record.json"), record);

        // Create a new asset bundle for each target.
        foreach (BuildTarget target in targets.Keys)
        {
            string targetDirectory = Path.Combine(assetBundlesDirectory, target.ToString());
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }
            // Build the asset bundles.
            BuildPipeline.BuildAssetBundles(targetDirectory,
                builds,
                BuildAssetBundleOptions.None,
                target);
        }
    }
}