using UnityEditor;
using UnityEngine;

// Converts the selected materials (or every material inside the selected folders)
// to "Universal Render Pipeline/Simple Lit", keeping texture, color, normal map,
// emission and alpha cutout. Put this file in an "Editor" folder.
//
// IMPORTANT: values are read from the OLD shader's properties, so this only keeps
// textures if the old shader still exists in the project. If a material comes out
// without a texture, assign the Base Map by hand.
public static class MaterialsToSimpleLit
{
    const string SimpleLitName = "Universal Render Pipeline/Simple Lit";

    static readonly string[] BaseTextureNames = { "_BaseMap", "_MainTex", "_BaseColorMap", "_Albedo", "_Diffuse" };
    static readonly string[] ColorNames = { "_BaseColor", "_Color", "_TintColor" };
    static readonly string[] NormalNames = { "_BumpMap", "_NormalMap", "_Normal" };
    static readonly string[] CutoffNames = { "_Cutoff", "_AlphaCutoff" };

    [MenuItem("Tools/Materials/Convert Selected To URP Simple Lit")]
    static void ConvertSelected()
    {
        Shader simpleLit = Shader.Find(SimpleLitName);
        if (simpleLit == null)
        {
            Debug.LogError("Simple Lit shader not found. Is URP installed?");
            return;
        }

        int converted = 0;
        int skipped = 0;

        foreach (Object obj in Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets))
        {
            var mat = (Material)obj;
            string path = AssetDatabase.GetAssetPath(mat);

            // Packages are read-only, and already converted materials need nothing.
            if (path.StartsWith("Packages/") || mat.shader == simpleLit)
            {
                skipped++;
                continue;
            }

            // Transparent URP materials (water, glass) need a manual decision.
            if (mat.HasProperty("_Surface") && mat.GetFloat("_Surface") > 0.5f)
            {
                Debug.LogWarning($"Skipped transparent material (review by hand): {mat.name}", mat);
                skipped++;
                continue;
            }

            // 1) Read everything from the old shader BEFORE swapping it.
            Texture baseTex = FindTexture(mat, BaseTextureNames);
            Texture normalTex = FindTexture(mat, NormalNames);
            Color color = FindColor(mat, ColorNames);
            Texture emissionTex = mat.HasProperty("_EmissionMap") ? mat.GetTexture("_EmissionMap") : null;
            Color emissionColor = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;

            bool cutout = mat.IsKeywordEnabled("_ALPHATEST_ON")
                          || mat.shader.name.ToLowerInvariant().Contains("cutout");
            float cutoff = 0.5f;
            foreach (string n in CutoffNames)
            {
                if (mat.HasProperty(n))
                {
                    cutout = true;
                    cutoff = mat.GetFloat(n);
                    break;
                }
            }

            // 2) Swap the shader and write the values back.
            Undo.RecordObject(mat, "Convert to URP Simple Lit");
            mat.shader = simpleLit;

            mat.SetTexture("_BaseMap", baseTex);
            mat.SetColor("_BaseColor", color);

            if (normalTex != null)
            {
                mat.SetTexture("_BumpMap", normalTex);
                mat.EnableKeyword("_NORMALMAP");
            }

            if (emissionTex != null || emissionColor.maxColorComponent > 0.01f)
            {
                mat.SetTexture("_EmissionMap", emissionTex);
                mat.SetColor("_EmissionColor", emissionColor);
                mat.EnableKeyword("_EMISSION");
            }

            if (cutout)
            {
                mat.SetFloat("_AlphaClip", 1f);
                mat.SetFloat("_Cutoff", cutoff);
                mat.SetFloat("_Cull", 0f);          // Both faces (leaves, grass)
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.renderQueue = 2450;             // AlphaTest queue
            }

            // Soft, low specular so the low-poly look stays matte and calm.
            if (mat.HasProperty("_SpecColor")) mat.SetColor("_SpecColor", new Color(0.1f, 0.1f, 0.1f, 1f));
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.1f);

            EditorUtility.SetDirty(mat);
            converted++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Simple Lit conversion: {converted} converted, {skipped} skipped.");
    }

    static Texture FindTexture(Material mat, string[] names)
    {
        foreach (string n in names)
        {
            if (!mat.HasProperty(n)) continue;
            Texture t = mat.GetTexture(n);
            if (t != null) return t;
        }
        return null;
    }

    static Color FindColor(Material mat, string[] names)
    {
        foreach (string n in names)
        {
            if (mat.HasProperty(n)) return mat.GetColor(n);
        }
        return Color.white;
    }
}
