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
    private string assetName = "";

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

        assetName = EditorGUILayout.TextField("Asset Name", assetName);

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
                var outputAnimPath = Path.Join(outputPath, anim.Name);
                var (texturePath, texture) = TextureUtils.LoadAndCopyTexture(sourceImgPath, outputAnimPath);

                List<Sprite> sprites = TextureUtils.SliceTextureIntoSprites(texture, texturePath, anim.FrameWidth, anim.FrameHeight);

                if (texture.height >= spriteOrientation.Count() * anim.FrameHeight)
                {
                    // Height directions
                    foreach (var (orientation, row) in spriteOrientation.Select((value, i) => (value, i)))
                    {

                        var clip = CreateAnimationClip(anim.Name, orientation, row, anim.Durations, sprites);
                        clips.Add(clip);
                    }
                }
                else
                {
                    // Single direction
                    var clip = CreateAnimationClip(anim.Name, null, 0, anim.Durations, sprites);
                    clips.Add(clip);
                }
            }
        }

        CreateAnimatorController(clips);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Animations generated successfully.", "OK");
    }

    private AnimationClip CreateAnimationClip(string animName, ActorOrientation? orientation, int row, List<int> durations, List<Sprite> sprites)
    {
        AnimationClip clip = new AnimationClip
        {
            frameRate = durations.Aggregate((acc, duration) => acc + duration)

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

            var duration = durations[i]; // Duration for this specific frame

            // Calculate the cumulative time for this keyframe
            float cumulativeTime = 0f;
            for (int j = 0; j < i; j++)
            {
                cumulativeTime += durations[j] / clip.frameRate;
            }

            // Set the keyframe for the current frame duration
            keyFrames[i] = new ObjectReferenceKeyframe
            {
                time = cumulativeTime,
                value = sprite
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyFrames);

        var clipName = orientation != null ? orientation + "_" + animName : animName;
        string path = AssetUtils.GetAssetRelativePath(Path.Combine(outputPath, animName, clipName + ".anim"));
        AssetDatabase.CreateAsset(clip, path);

        return clip;
    }

    private void CreateAnimatorController(List<AnimationClip> clips)
    {
        // Create Animator Controller
        // TODO: avoid creating a new controller if there is already one there
        string controllerPath = AssetUtils.GetAssetRelativePath(Path.Combine(outputPath, assetName + "_Animator.controller"));
        AnimatorController animatorController = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        foreach (AnimationClip clip in clips)
        {
            // Add state for each clip
            AnimatorState state = animatorController.AddMotion(clip);
            state.name = clip.name;
        }

    }
}