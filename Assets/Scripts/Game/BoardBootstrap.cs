using System.Collections.Generic;
using UnityEngine;
using WeBoard.Rules;

namespace WeBoard.Game
{
    /// <summary>
    /// Uji awal (smoke test): diorama papan heks semi-3D, kamera snap 90 derajat,
    /// dan lemparan d20 memakai pustaka aturan C# murni. Semua bentuk dibuat dari mesh
    /// buatan sendiri (tanpa CreatePrimitive, yang bergantung pada kelas yang dibuang
    /// saat build WebGL). Bahan diambil dari Resources/WeBoardLit yang dibuat saat build.
    /// </summary>
    public sealed class BoardBootstrap : MonoBehaviour
    {
        private const float HexRadius = 1f;
        private const int BoardRadius = 3;
        private const float CameraDistance = 27f;
        private const float CameraPitch = 45f;

        private readonly SeededRandom _rng = new SeededRandom(2026);
        private readonly Dictionary<HexCoord, float> _tileHeights = new Dictionary<HexCoord, float>();
        private Camera _camera;
        private Material _litMaterial;
        private float _yaw = 45f;
        private float _targetYaw = 45f;
        private string _status = "Tekan Lempar d20";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            var go = new GameObject("WeBoardBootstrap");
            go.AddComponent<BoardBootstrap>();
        }

        private void Start()
        {
            _litMaterial = Resources.Load<Material>("WeBoardLit");
            if (_litMaterial == null)
            {
                Debug.LogError("[WeBoard] Material WeBoardLit tidak ditemukan di Resources; memakai cadangan tanpa cahaya.");
                _litMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            BuildEnvironment();
            BuildBoard();
        }

        private void BuildEnvironment()
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            _camera = camGo.AddComponent<Camera>();
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = new Color(0.12f, 0.09f, 0.07f);
            _camera.fieldOfView = 28f;
            _camera.nearClipPlane = 0.3f;
            _camera.farClipPlane = 100f;

            var lightGo = new GameObject("Matahari");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.None;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.55f);

            // Meja kayu berbentuk heks di bawah diorama; permukaannya sejajar y = 0.
            Piece("Meja", CreateHexPrism(8f, 1f), new Vector3(0f, -0.5f, 0f), new Vector3(1f, 0.5f, 1f), new Color(0.35f, 0.23f, 0.14f), null);

            UpdateCamera();
        }

        private void BuildBoard()
        {
            Mesh hex = CreateHexPrism(HexRadius * 0.96f, 1f);
            var board = new GameObject("Papan").transform;

            foreach (HexCoord cell in new HexCoord(0, 0).Within(BoardRadius))
            {
                int level = _rng.Roll(3);
                float height = 0.3f + 0.15f * (level - 1);
                _tileHeights[cell] = height;
                Piece("Heks " + cell, hex, ToWorld(cell), new Vector3(1f, height, 1f), TileColor(level), board);
            }

            // Miniatur: pahlawan di tengah papan dan satu musuh sebagai contoh.
            Mesh pawn = CreateLathe(PawnProfile, 24);
            PlacePawn(pawn, "Miniatur Penjaga Perisai", new HexCoord(0, 0), 1f, new Color(0.25f, 0.4f, 0.85f));
            PlacePawn(pawn, "Miniatur Jamur Berjalan", new HexCoord(2, -1), 0.85f, new Color(0.75f, 0.25f, 0.3f));
        }

        private void PlacePawn(Mesh pawn, string name, HexCoord cell, float scale, Color color)
        {
            Vector3 pos = ToWorld(cell);
            pos.y = _tileHeights[cell];
            Piece(name, pawn, pos, new Vector3(scale, scale, scale), color, null);
        }

        private GameObject Piece(string name, Mesh mesh, Vector3 pos, Vector3 scale, Color color, Transform parent)
        {
            var go = new GameObject(name);
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;

            var mat = new Material(_litMaterial);
            mat.color = color;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        private static Color TileColor(int level)
        {
            switch (level)
            {
                case 1: return new Color(0.85f, 0.72f, 0.50f);
                case 2: return new Color(0.78f, 0.60f, 0.40f);
                default: return new Color(0.65f, 0.45f, 0.30f);
            }
        }

        /// <summary>Heks bermata runcing ke atas: koordinat aksial ke posisi dunia.</summary>
        private static Vector3 ToWorld(HexCoord c)
        {
            float x = HexRadius * Mathf.Sqrt(3f) * (c.Q + c.R * 0.5f);
            float z = HexRadius * 1.5f * c.R;
            return new Vector3(x, 0f, z);
        }

        /// <summary>Siluet miniatur (jari-jari, tinggi) dari alas bulat sampai kepala, diputar mengelilingi sumbu Y.</summary>
        private static readonly Vector2[] PawnProfile =
        {
            new Vector2(0.40f, 0.00f),
            new Vector2(0.40f, 0.08f),
            new Vector2(0.30f, 0.12f),
            new Vector2(0.17f, 0.35f),
            new Vector2(0.15f, 0.60f),
            new Vector2(0.25f, 0.70f),
            new Vector2(0.12f, 0.78f),
            new Vector2(0.22f, 0.92f),
            new Vector2(0.26f, 1.10f),
            new Vector2(0.20f, 1.26f),
            new Vector2(0.08f, 1.34f),
            new Vector2(0.00f, 1.36f)
        };

        /// <summary>Memutar profil (jari-jari, tinggi) menjadi mesh bersumbu Y.</summary>
        private static Mesh CreateLathe(Vector2[] profile, int segments)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();

            for (int p = 0; p < profile.Length; p++)
            {
                for (int s = 0; s <= segments; s++)
                {
                    float a = 2f * Mathf.PI * s / segments;
                    verts.Add(new Vector3(Mathf.Cos(a) * profile[p].x, profile[p].y, Mathf.Sin(a) * profile[p].x));
                }
            }

            int stride = segments + 1;
            for (int p = 0; p < profile.Length - 1; p++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int p0 = p * stride + s;
                    int p1 = p0 + 1;
                    int p3 = (p + 1) * stride + s;
                    int p2 = p3 + 1;
                    tris.Add(p0);
                    tris.Add(p2);
                    tris.Add(p1);
                    tris.Add(p0);
                    tris.Add(p3);
                    tris.Add(p2);
                }
            }

            var mesh = new Mesh { name = "Lathe" };
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Prisma heks bertinggi <paramref name="height"/>; skala Y menentukan tinggi akhir.</summary>
        private static Mesh CreateHexPrism(float radius, float height)
        {
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var tris = new List<int>();

            var corners = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                float a = Mathf.Deg2Rad * (60f * i - 30f);
                corners[i] = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            }

            // Sisi atas.
            int center = verts.Count;
            verts.Add(new Vector3(0f, height, 0f));
            normals.Add(Vector3.up);
            for (int i = 0; i < 6; i++)
            {
                verts.Add(corners[i] + Vector3.up * height);
                normals.Add(Vector3.up);
            }
            for (int i = 0; i < 6; i++)
            {
                tris.Add(center);
                tris.Add(center + 1 + (i + 1) % 6);
                tris.Add(center + 1 + i);
            }

            // Sisi samping.
            for (int i = 0; i < 6; i++)
            {
                int j = (i + 1) % 6;
                Vector3 outward = (corners[i] + corners[j]).normalized;
                int b = verts.Count;
                verts.Add(corners[i]);
                verts.Add(corners[j]);
                verts.Add(corners[j] + Vector3.up * height);
                verts.Add(corners[i] + Vector3.up * height);
                for (int k = 0; k < 4; k++)
                {
                    normals.Add(outward);
                }
                tris.Add(b);
                tris.Add(b + 2);
                tris.Add(b + 1);
                tris.Add(b);
                tris.Add(b + 3);
                tris.Add(b + 2);
            }

            var mesh = new Mesh { name = "HexPrism" };
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private void LateUpdate()
        {
            _yaw = Mathf.LerpAngle(_yaw, _targetYaw, 1f - Mathf.Exp(-8f * Time.deltaTime));
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            if (_camera == null)
            {
                return;
            }

            Quaternion yawRot = Quaternion.Euler(0f, _yaw, 0f);
            float pitchRad = Mathf.Deg2Rad * CameraPitch;
            Vector3 offset = new Vector3(0f, Mathf.Sin(pitchRad), -Mathf.Cos(pitchRad)) * CameraDistance;
            _camera.transform.position = yawRot * offset;
            _camera.transform.LookAt(Vector3.zero);
        }

        private void OnGUI()
        {
            float scale = Mathf.Max(0.5f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            GUI.Label(new Rect(16, 12, 640, 28), "Tujuh Segel: Kerajaan Abu (uji Fase 1)");
            GUI.Label(new Rect(16, 36, 640, 28), "Papan heks radius " + BoardRadius + " | jarak 3 heks dari pusat: " + HexCoord.Distance(new HexCoord(0, 0), new HexCoord(3, 0)));

            if (GUI.Button(new Rect(16, 76, 160, 40), "Lempar d20"))
            {
                CheckResult r = CheckResolver.Roll(_rng, AbilityMath.Modifier(14), DifficultyClass.Medium);
                _status = "d20 = " + r.NaturalRoll + " + " + r.Modifier + " = " + r.Total + " melawan DC " + r.Dc + ": " + OutcomeText(r.Outcome);
            }
            GUI.Label(new Rect(190, 84, 700, 28), _status);

            if (GUI.Button(new Rect(16, 126, 76, 36), "< Putar"))
            {
                _targetYaw -= 90f;
            }
            if (GUI.Button(new Rect(100, 126, 76, 36), "Putar >"))
            {
                _targetYaw += 90f;
            }
        }

        private static string OutcomeText(CheckOutcome outcome)
        {
            switch (outcome)
            {
                case CheckOutcome.CriticalSuccess: return "Berhasil luar biasa";
                case CheckOutcome.Success: return "Berhasil";
                case CheckOutcome.Failure: return "Gagal";
                default: return "Gagal dramatis";
            }
        }
    }
}
