using UnityEditor;
using UnityEngine;

// Repairs materials that turned black after being switched to URP Simple Lit.
// Unity keeps the old shader's values inside the material file even after the shader
// changes, so this script recovers the old _MainTex / _Color from there.
// Select the black materials (or folders) in the Project window and run:
//   Tools > Materials > Repair Selected Simple Lit
public static class RepairSimpleLitMaterials
{
    const string SimpleLitName = "Universal Render Pipeline/Simple Lit";

    [MenuItem("Tools/Materials/Repair Selected Simple Lit")]
    static void Repair()
    {
        int repaired = 0;

        foreach (Object obj in Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets))
        {
            var mat = (Material)obj;
            if (mat.shader == null || mat.shader.name != SimpleLitName) continue;

            var so = new SerializedObject(mat);
            var report = new System.Text.StringBuilder();
            report.Append(mat.name).Append(": ");

            // 1) Base texture: recover it if it is empty.
            if (mat.GetTexture("_BaseMap") == null)
            {
                Texture old = ReadSavedTexture(so, "_MainTex");
                if (old == null) old = ReadSavedTexture(so, "_BaseColorMap");
                if (old != null)
                {
                    mat.SetTexture("_BaseMap", old);
                    report.Append("restored Base Map; ");
                }
                else
                {
                    report.Append("NO base texture found; ");
                }
            }

            // 2) Base color: black or fully transparent tints make everything look black.
            Color current = mat.GetColor("_BaseColor");
            if (current.maxColorComponent < 0.05f || current.a < 0.05f)
            {
                Color fallback = Color.white;
                if (TryReadSavedColor(so, "_Color", out Color old) && old.maxColorComponent >= 0.05f && old.a >= 0.05f)
                {
                    fallback = old;
                }
                mat.SetColor("_BaseColor", fallback);
                report.Append("fixed Base Color; ");
            }

            // 3) Normal map: a texture that is not imported as "Normal map" shades black.
            Texture normal = mat.GetTexture("_BumpMap");
            if (normal != null)
            {
                string path = AssetDatabase.GetAssetPath(normal);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.NormalMap)
                {
                    mat.SetTexture("_BumpMap", null);
                    mat.DisableKeyword("_NORMALMAP");
                    report.Append("removed invalid normal map; ");
                }
            }
            else
            {
                mat.DisableKeyword("_NORMALMAP");
            }

            // 4) Make sure the surface is opaque and not hidden by blend settings.
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 0f);
            mat.SetOverrideTag("RenderType", mat.IsKeywordEnabled("_ALPHATEST_ON") ? "TransparentCutout" : "Opaque");

            EditorUtility.SetDirty(mat);
            repaired++;
            Debug.Log(report.ToString(), mat);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Repair finished: {repaired} Simple Lit materials checked.");
    }

    // The old shader's values live in m_SavedProperties, even if the current shader ignores them.
    static Texture ReadSavedTexture(SerializedObject so, string propertyName)
    {
        SerializedProperty list = so.FindProperty("m_SavedProperties.m_TexEnvs");
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty entry = list.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("first").stringValue == propertyName)
            {
                return entry.FindPropertyRelative("second.m_Texture").objectReferenceValue as Texture;
            }
        }
        return null;
    }

    static bool TryReadSavedColor(SerializedObject so, string propertyName, out Color color)
    {
        SerializedProperty list = so.FindProperty("m_SavedProperties.m_Colors");
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty entry = list.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("first").stringValue == propertyName)
            {
                color = entry.FindPropertyRelative("second").colorValue;
                return true;
            }
        }
        color = Color.white;
        return false;
    }
}
