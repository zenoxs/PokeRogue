using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;
using UnityEditor.Animations;

public class SpriteAnimationGeneratorEditor : EditorWindow
{
    private string outputPath = "Assets/Animations/";
    private string sourcePath = "";

    [MenuItem("Tools/Sprite Animation Generator")]
    public static void ShowWindow()
    {
        GetWindow<SpriteAnimationGeneratorEditor>("Sprite Animation Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Sprite Animation Generator", EditorStyles.boldLabel);

        #region source path
        GUILayout.BeginHorizontal();

        sourcePath = EditorGUILayout.TextField("Source Path", sourcePath);

        if (GUILayout.Button("Select", GUILayout.Width(100)))
        {
            sourcePath = EditorUtility.OpenFolderPanel("Source folder", "", "");
            Debug.Log(sourcePath);

        }

        GUILayout.EndHorizontal();
        #endregion

        #region outputh path
        GUILayout.BeginHorizontal();

        outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        if (GUILayout.Button("Select", GUILayout.Width(100)))
        {
            outputPath = EditorUtility.OpenFolderPanel("Output folder", "", "");
            Debug.Log(outputPath);

        }

        GUILayout.EndHorizontal();
        #endregion

        if (GUILayout.Button("Generate Animations"))
        {
            if (sourcePath != null && outputPath != null)
            {
                GenerateAnimations();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please assign both Source and Output path.", "OK");
            }
        }
    }

    private void GenerateAnimations()
    {
        var files = Directory.GetFiles(sourcePath);
        var animDataXmlPath = files.FirstOrDefault(f => f.EndsWith("AnimData.xml"));

        if (animDataXmlPath == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to find AnimData.xml in the source path.", "OK");
            return;
        }

        var animDataXml = File.ReadAllText(animDataXmlPath);

        var animData = AnimDataDeserializer.Deserialize(animDataXml);

        var images = files.Where(f => f.EndsWith(".png")).ToArray();
        // .Select(file => new List<Sprite>(AssetDatabase.LoadAllAssetsAtPath(file).OfType<Sprite>())).ToArray();
        Debug.Log(images);

        foreach (var img in images)
        {
            var (texturePath, texture) = TextureUtilities.LoadAndCopyTexture(img, outputPath);
            TextureUtilities.SliceTextureIntoSprites(texture, texturePath, 32, 56);
        }

        // List<Sprite> sprites = new(AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(spriteAtlas)).OfType<Sprite>());

        // List<AnimationClip> clips = new();

        // foreach (var animInfo in animData.Anims)
        // {
        //     if (animInfo is Anim anim)
        //     {
        //         // var clip = CreateAnimationClip(anim.Name, anim.Index, anim.FrameWidth, anim.FrameHeight, anim.Durations, sprites);
        //         // clips.Add(clip);
        //     }

        // }

        // CreateAnimatorController(clips);


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Animations generated successfully.", "OK");
    }

    private AnimationClip CreateAnimationClip(string animName, int index, int frameWidth, int frameHeight, List<int> durations, List<Sprite> sprites)
    {
        AnimationClip clip = new AnimationClip
        {
            frameRate = 60
        };

        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[durations.Count];

        for (int i = 0; i < durations.Count; i++)
        {
            int frameIndex = index + i;
            if (frameIndex >= sprites.Count)
            {
                Debug.LogError($"Frame index {frameIndex} out of range for animation '{animName}'.");
                return null;
            }

            Sprite sprite = sprites[frameIndex];

            keyFrames[i] = new ObjectReferenceKeyframe
            {
                time = i / (float)clip.frameRate,
                value = sprite
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyFrames);

        string path = Path.Combine(outputPath, animName + ".anim");
        AssetDatabase.CreateAsset(clip, path);

        return clip;
    }

    private void CreateAnimatorController(List<AnimationClip> clips)
    {
        // Create Animator Controller
        string controllerPath = Path.Combine(outputPath, "AnimatorController.controller");
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        foreach (AnimationClip clip in clips)
        {
            // Add state for each clip
            AnimatorState state = animatorController.AddMotion(clip);
            state.name = clip.name;
        }
    }
}