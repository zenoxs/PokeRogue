using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEditor.U2D.Sprites;

public class TextureUtils
{
    public static (string, Texture2D) LoadAndCopyTexture(string sourceImgPath, string outputFolderPath)
    {
        string fileName = Path.GetFileName(sourceImgPath);

        // Create the output folder if it doesn't exist
        if (!Directory.Exists(outputFolderPath))
        {
            Directory.CreateDirectory(outputFolderPath);
        }

        string outputFilePath = Path.Combine(outputFolderPath, fileName);

        // Copy the file to the output folder
        File.Copy(sourceImgPath, outputFilePath, true);
        Debug.Log("File copied to: " + outputFilePath);

        // Load texture from the copied file
        byte[] fileData = File.ReadAllBytes(outputFilePath);
        var sourceTexture = new Texture2D(2, 2);
        sourceTexture.LoadImage(fileData);

        // Clone the texture
        var clonedTexture = CloneTexture(sourceTexture, Path.GetFileNameWithoutExtension(fileName));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return (outputFilePath, clonedTexture);
    }

    public static Texture2D CloneTexture(Texture2D original, string name)
    {
        // Create a new Texture2D with the same dimensions and format
        Texture2D cloned = new Texture2D(original.width, original.height, original.format, original.mipmapCount > 1);

        // Copy the pixel data
        cloned.SetPixels(original.GetPixels());
        cloned.name = name;
        cloned.Apply(); // Apply the changes to update the texture

        return cloned;
    }

    public static List<Sprite> SliceTextureIntoSprites(Texture2D texture, string texturePath, int spriteWidth, int spriteHeight)
    {
        var relativeTexturePath = AssetUtils.GetAssetRelativePath(texturePath);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(relativeTexturePath);

        if (importer == null)
        {
            Debug.LogError("Could not find texture at path: " + relativeTexturePath);
            return new();
        }

        importer.isReadable = true;
        importer.textureType = TextureImporterType.Sprite;

        TextureImporterSettings importerSettings = new TextureImporterSettings();
        importer.ReadTextureSettings(importerSettings);
        importerSettings.spritePixelsPerUnit = 64;
        importerSettings.spriteMode = (int)SpriteImportMode.Multiple;
        importer.SetTextureSettings(importerSettings);

        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;

        // Calculate the grid size for slicing
        int columns = texture.width / spriteWidth;
        int rows = texture.height / spriteHeight;

        // Slice the texture into sprites
        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();
        var spriteRects = new List<SpriteRect>();
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                // Define the rectangle for each sprite
                Rect rect = new Rect(x * spriteWidth, y * spriteHeight, spriteWidth, spriteHeight);

                SpriteRect spriteRect = new SpriteRect
                {
                    alignment = (int)SpriteAlignment.Center,
                    border = Vector4.zero,
                    name = $"{texture.name}_{y}_{x}",
                    pivot = new Vector2(0.5f, 0.5f),
                    rect = rect
                };

                spriteRects.Add(spriteRect);
            }
        }
        dataProvider.SetSpriteRects(spriteRects.ToArray());
        dataProvider.Apply();

        importer.SaveAndReimport();


        // Apply changes to the importer and re-import the asset
        AssetDatabase.ImportAsset(relativeTexturePath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        // Load the sprites from the texture
        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(relativeTexturePath);

        var generatedSprites = new List<Sprite>();

        foreach (var sprite in sprites)
        {
            if (sprite is Sprite)
            {
                generatedSprites.Add(sprite as Sprite);
            }
        }

        Debug.Log("Texture sliced into sprites successfully.");

        return generatedSprites;

    }
}
