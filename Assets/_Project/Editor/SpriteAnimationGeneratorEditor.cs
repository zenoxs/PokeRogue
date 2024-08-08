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
    private string outputPath = "Assets/Actors/";
    private string sourcePath = "";
    private string assetName = "";

    private static readonly ActorOrientation[] spriteOrientation = new[]
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

    private static readonly string[] whiteListAnimation = new[] { "Walk", "Idle", "Attack" };

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
            sourcePath = EditorUtility.OpenFolderPanel("Source folder", "Assets", "");
            assetName = Path.GetFileName(sourcePath);
        }

        GUILayout.EndHorizontal();
        #endregion

        #region outputh path
        GUILayout.BeginHorizontal();

        outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        if (GUILayout.Button("Select", GUILayout.Width(100)))
        {
            outputPath = EditorUtility.OpenFolderPanel("Output folder", "Assets", "");

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
            if (!whiteListAnimation.Contains(animInfo.Name))
            {
                continue;
            }

            if (animInfo is Anim anim)
            {
                var sourceImgPath = Path.Join(sourcePath, anim.Name + "-Anim.png");
                SliceTextureAndCreateAnimationClip(anim.Name, sourceImgPath, anim.FrameWidth, anim.FrameHeight, anim.Durations, ref clips);

            }
            if (animInfo is AnimCopyOf animCopyOf)
            {
                var animReference = (Anim)animData.Anims.First(a => a.Name == animCopyOf.CopyOf);
                var sourceImgPath = Path.Join(sourcePath, animCopyOf.CopyOf + "-Anim.png");
                SliceTextureAndCreateAnimationClip(animCopyOf.Name, sourceImgPath, animReference.FrameWidth, animReference.FrameHeight, animReference.Durations, ref clips);
            }

        }

        CreateAnimatorController(clips);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Success", "Animations generated successfully.", "OK");
    }

    private void SliceTextureAndCreateAnimationClip(string animName, string sourceImgPath, int frameWidth, int frameHeight, List<int> durations, ref List<AnimationClip> clips)
    {
        var outputAnimPath = Path.Join(outputPath, assetName, animName);
        var (texturePath, texture) = TextureUtils.LoadAndCopyTexture(sourceImgPath, outputAnimPath);

        List<Sprite> sprites = TextureUtils.SliceTextureIntoSprites(texture, texturePath, frameWidth, frameHeight);

        if (texture.height >= spriteOrientation.Count() * frameHeight)
        {
            // Height directions
            foreach (var (orientation, row) in spriteOrientation.Select((value, i) => (value, i)))
            {
                var clip = CreateAnimationClip(animName, orientation, row, durations, sprites);
                clips.Add(clip);
            }
        }
        else
        {
            // Single direction
            var clip = CreateAnimationClip(animName, null, 0, durations, sprites);
            clips.Add(clip);
        }
    }

    private AnimationClip CreateAnimationClip(string animName, ActorOrientation? orientation, int row, List<int> durations, List<Sprite> sprites)
    {
        // Create a new AnimationClip
        AnimationClip clip = new AnimationClip
        {
            frameRate = durations.Aggregate((acc, duration) => acc + duration)
        };

        // Enable loop time
        AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

        // Sprite binding to add key frame sprites
        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[durations.Count];

        float cumulativeTime = 0f;
        for (int i = 0; i < durations.Count; i++)
        {
            int frameIndex = i + row * durations.Count;
            if (frameIndex >= sprites.Count)
            {
                Debug.LogError($"Frame index {frameIndex} out of range for animation '{animName}'.");
                return null;
            }

            Sprite sprite = sprites[frameIndex];

            // Set the keyframe for the current frame duration
            keyFrames[i] = new ObjectReferenceKeyframe
            {
                time = cumulativeTime,
                value = sprite
            };

            var duration = durations[i]; // Duration for this specific frame

            // Update the cumulative time for the next keyframe
            cumulativeTime += duration / clip.frameRate;
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyFrames);

        // Add the AnimationEvent to call OnFinished at the end of the animation
        AnimationEvent animationEvent = new AnimationEvent
        {
            time = cumulativeTime, // Set event time to the end of the animation
            functionName = "OnFinished",
            stringParameter = animName
        };

        AnimationUtility.SetAnimationEvents(clip, new[] { animationEvent });


        var clipName = orientation != null ? animName + "_" + orientation : animName;
        string path = AssetUtils.GetAssetRelativePath(Path.Combine(outputPath, assetName, animName, clipName + ".anim"));

        AssetDatabase.CreateAsset(clip, path);

        return clip;
    }

    private void CreateAnimatorController(List<AnimationClip> clips)
    {
        // Create Animator Controller
        string controllerPath = AssetUtils.GetAssetRelativePath(Path.Combine(outputPath, assetName, assetName + "_Animator.controller"));
        var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (animatorController == null)
        {
            animatorController = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        // Remove previous clips
        // Iterate through each layer in the AnimatorController
        foreach (var layer in animatorController.layers)
        {
            AnimatorStateMachine stateMachine = layer.stateMachine;

            // Iterate through each state in the state machine
            for (int i = stateMachine.states.Length - 1; i >= 0; i--)
            {
                stateMachine.RemoveState(stateMachine.states[i].state);
            }
        }

        foreach (AnimationClip clip in clips)
        {
            // Add state for each clip
            AnimatorState state = animatorController.AddMotion(clip);
            state.name = clip.name;
        }

        // Set default state to "Idle_South" in each layer
        foreach (AnimatorControllerLayer layer in animatorController.layers)
        {
            AnimatorStateMachine stateMachine = layer.stateMachine;
            AnimatorState idleSouthState = stateMachine.FindStateByName("Idle_South");

            if (idleSouthState != null)
            {
                stateMachine.defaultState = idleSouthState;
                Debug.Log("Set default state to 'Idle_South' for layer: " + layer.name);
            }
            else
            {
                Debug.LogWarning("'Idle_South' state not found in layer: " + layer.name);
            }
        }

        // Save the changes made to the AnimatorController
        EditorUtility.SetDirty(animatorController);
        AssetDatabase.SaveAssets();
    }
}