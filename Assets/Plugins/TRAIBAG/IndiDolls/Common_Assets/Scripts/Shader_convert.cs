using UnityEngine;
using UnityEditor;

namespace TRAIBAG.Tools
{
    public class ReplaceShaders : EditorWindow
    {
        [MenuItem("Tools/TRAIBAG/Convert URP Shaders")]
        public static void ShowWindow()
        {
            GetWindow<ReplaceShaders>("Convert URP Shaders");
        }

        private void OnGUI()
        {
            GUILayout.Label("AUTO: Replace shaders based on material name", EditorStyles.boldLabel);
            GUILayout.Space(5);

            if (GUILayout.Button("Auto Replace Shaders"))
                ReplaceShadersAuto();

            GUILayout.Space(15);

            GUILayout.Label("MANUAL: Replace shaders manually", EditorStyles.boldLabel);
            GUILayout.Space(5);

            if (GUILayout.Button("Apply Custom Shader"))
                ReplaceShadersManual("TRAIBAG/IndiDolls_Custom_urp");

            if (GUILayout.Button("Apply Custom Hair Shader"))
                ReplaceShadersManual("TRAIBAG/IndiDolls_Hair_urp");
        }

        private static void ReplaceShadersAuto()
        {
            Object[] selection = Selection.objects;
            int count = 0;

            foreach (var obj in selection)
            {
                if (obj is Material mat)
                {
                    string targetShaderName = mat.name.ToLower().Contains("hair")
                        ? "TRAIBAG/IndiDolls_Hair_urp"
                        : "TRAIBAG/IndiDolls_Custom_urp";

                    Shader targetShader = Shader.Find(targetShaderName);
                    if (targetShader != null)
                    {
                        mat.shader = targetShader;
                        count++;
                    }
                    else
                        Debug.LogWarning($"Shader not found: {targetShaderName}");
                }
            }

            EditorUtility.DisplayDialog("Auto Replace", $"{count} materials updated based on name.", "OK");
        }

        private static void ReplaceShadersManual(string targetShaderName)
        {
            Object[] selection = Selection.objects;
            int count = 0;

            Shader targetShader = Shader.Find(targetShaderName);
            if (targetShader == null)
            {
                EditorUtility.DisplayDialog("Error", $"Shader not found: {targetShaderName}", "OK");
                return;
            }

            foreach (var obj in selection)
            {
                if (obj is Material mat)
                {
                    mat.shader = targetShader;
                    count++;
                }
            }

            EditorUtility.DisplayDialog("Manual Replace", $"{count} materials updated to '{targetShaderName}'.", "OK");
        }
    }
}
