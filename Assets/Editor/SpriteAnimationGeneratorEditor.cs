using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;
using UnityEditor.Animations;
using System;

public class SpriteAnimationGeneratorEditor : EditorWindow
{
    private string outputPath = "Assets/Animations/";
    private string sourcePath = "";

    private static ActorOrientation[] spriteOrientation = new[]
    {
        ActorOrientation.SouthWest,
        ActorOrientation.West,
        ActorOrientation.NorthWest,
        ActorOrientation.North,
        ActorOrientation.NorthEast,
        ActorOrientation.East,
        ActorOrientation.SouthEast,
        ActorOrientation.South

    };

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
        var animDataXmlPath = Path.Join(sourcePath, "AnimData.xml");

        if (animDataXmlPath == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to find AnimData.xml in the source path.", "OK");
            return;
        }

        var animDataXml = File.ReadAllText(animDataXmlPath);
        var animData = AnimDataDeserializer.Deserialize(animDataXml);

        List<AnimationClip> clips = new();

        foreach (var animInfo in animData.Anims)
        {
            if (animInfo is Anim anim)
            {
                var sourceImgPath = Path.Join(sourcePath, anim.Name + "-Anim.png");
                var (texturePath, texture) = TextureUtils.LoadAndCopyTexture(sourceImgPath, outputPath);

                List<Sprite> sprites = TextureUtils.SliceTextureIntoSprites(texture, texturePath, anim.FrameWidth, anim.FrameHeight);

                if (texture.height >= spriteOrientation.Count() * anim.FrameHeight)
                {
                    // Height directions
                    foreach (var (orientation, row) in spriteOrientation.Select((value, i) => (value, i)))
                    {
                        var clipName = orientation + "_" + anim.Name;
                        var clip = CreateAnimationClip(clipName, row, anim.Durations, sprites);
                        clips.Add(clip);
                    }
                }
                else
                {
                    // Single direction
                    var clip = CreateAnimationClip(anim.Name, 0, anim.Durations, sprites);
                    clips.Add(clip);
                }
            }
        }

        CreateAnimatorController(clips);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Animations generated successfully.", "OK");
    }

    private AnimationClip CreateAnimationClip(string animName, int row, List<int> durations, List<Sprite> sprites)
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
            int frameIndex = i + row * durations.Count;
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

        string path = AssetUtils.GetAssetRelativePath(Path.Combine(outputPath, animName + ".anim"));
        AssetDatabase.CreateAsset(clip, path);

        return clip;
    }

    private void CreateAnimatorController(List<AnimationClip> clips)
    {
        // Create Animator Controller
        string controllerPath = AssetUtils.GetAssetRelativePath(Path.Combine(outputPath, "AnimatorController.controller"));
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        foreach (AnimationClip clip in clips)
        {
            // Add state for each clip
            AnimatorState state = animatorController.AddMotion(clip);
            state.name = clip.name;
        }
    }
}