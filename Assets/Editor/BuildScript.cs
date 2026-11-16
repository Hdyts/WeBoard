using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WeBoard.BuildTools
{
    /// <summary>
    /// Titik masuk build tanpa kepala (batchmode) untuk game-ci/unity-builder.
    /// Scene awal dibuat lewat kode saat build, jadi tidak ada berkas scene yang ditulis tangan.
    /// </summary>
    public static class BuildScript
    {
        private const string ScenePath = "Assets/Scenes/Boot.unity";

        public static void BuildWebGL()
        {
            int exitCode = 1;
            try
            {
                string outputPath = GetArg("-customBuildPath") ?? "build/WebGL/WebGL";
                Debug.Log("[WeBoard] Output build: " + outputPath);

                string parent = Path.GetDirectoryName(Path.GetFullPath(outputPath));
                if (!string.IsNullOrEmpty(parent))
                {
                    Directory.CreateDirectory(parent);
                }

                PlayerSettings.companyName = "WeBoard";
                PlayerSettings.productName = "Tujuh Segel";
                // Tanpa kompresi supaya mudah dilayani host statis mana pun (Vercel mengompresi sendiri).
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

                CreateBootScene();

                var options = new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = outputPath,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                };

                BuildReport report = BuildPipeline.BuildPlayer(options);
                BuildSummary summary = report.summary;
                Debug.Log("[WeBoard] Hasil build: " + summary.result + ", ukuran " + summary.totalSize + " byte, "
                          + summary.totalErrors + " error, " + summary.totalWarnings + " peringatan");

                exitCode = summary.result == BuildResult.Succeeded ? 0 : 1;
            }
            catch (Exception e)
            {
                Debug.LogError("[WeBoard] Build gagal: " + e);
                exitCode = 1;
            }

            EditorApplication.Exit(exitCode);
        }

        private static void CreateBootScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string GetArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}
