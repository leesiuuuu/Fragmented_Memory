using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public class SpritePivotTool
{
    [MenuItem("Tools/Set Selected Sprite Pivot")]
    static void SetPivot()
    {
        foreach (Object obj in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();
            SpriteRect[] spriteRects = dataProvider.GetSpriteRects();

            for (int i = 0; i < spriteRects.Length; i++)
            {
                SpriteRect rect = spriteRects[i];
                if (obj.name.ToLower().Contains("attack"))
                {
                    rect.alignment = SpriteAlignment.Custom;
                    rect.pivot = new Vector2(0.3f, 0f);
                }
                else if (obj.name.ToLower().Contains("move"))
                {
                    rect.alignment = SpriteAlignment.BottomCenter;
                }
                spriteRects[i] = rect;
            }

            dataProvider.SetSpriteRects(spriteRects);
            dataProvider.Apply();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }
}
