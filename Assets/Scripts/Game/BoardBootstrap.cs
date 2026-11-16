using System.Collections.Generic;
using UnityEngine;
using WeBoard.Rules;

namespace WeBoard.Game
{
    /// <summary>
    /// Uji awal (smoke test): diorama papan heks semi-3D, kamera snap 90 derajat,
    /// dan lemparan d20 memakai pustaka aturan C# murni. Semua dibangun lewat kode,
    /// jadi scene hanya perlu kosong.
    /// </summary>
    public sealed class BoardBootstrap : MonoBehaviour
    {
        private const float HexRadius = 1f;
        private const int BoardRadius = 3;
        private const float CameraDistance = 12f;
        private const float CameraPitch = 45f;

        private readonly SeededRandom _rng = new SeededRandom(2026);
        private Camera _camera;
        private Material _baseMaterial;
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
            // Material bawaan dari primitif ikut masuk build, jadi aman dipakai di WebGL.
            var probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _baseMaterial = probe.GetComponent<Renderer>().sharedMaterial;
            Destroy(probe);

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
            _camera.farClipPlane = 60f;

            var lightGo = new GameObject("Matahari");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.None;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.55f);

            // Meja kayu di bawah diorama.
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Meja";
            Destroy(table.GetComponent<Collider>());
            table.transform.position = new Vector3(0f, -0.25f, 0f);
            table.transform.localScale = new Vector3(18f, 0.5f, 18f);
            Tint(table, new Color(0.35f, 0.23f, 0.14f));

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
                Vector3 pos = ToWorld(cell);

                var tile = new GameObject("Heks " + cell);
                tile.transform.SetParent(board, false);
                tile.transform.position = pos;
                tile.transform.localScale = new Vector3(1f, height, 1f);
                tile.AddComponent<MeshFilter>().sharedMesh = hex;
                var renderer = tile.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = MakeMaterial(TileColor(level));

                if (cell == new HexCoord(0, 0))
                {
                    BuildMiniature(new Vector3(pos.x, height, pos.z));
                }
            }
        }

        private void BuildMiniature(Vector3 basePos)
        {
            var root = new GameObject("Miniatur Penjaga Perisai").transform;
            root.position = basePos;

            Part(root, PrimitiveType.Cylinder, new Vector3(0f, 0.03f, 0f), new Vector3(0.75f, 0.03f, 0.75f), new Color(0.2f, 0.35f, 0.75f));
            Part(root, PrimitiveType.Capsule, new Vector3(0f, 0.55f, 0f), new Vector3(0.45f, 0.5f, 0.45f), new Color(0.25f, 0.4f, 0.85f));
            Part(root, PrimitiveType.Sphere, new Vector3(0f, 1.2f, 0f), new Vector3(0.42f, 0.42f, 0.42f), new Color(0.95f, 0.8f, 0.65f));
        }

        private void Part(Transform parent, PrimitiveType type, Vector3 localPos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            Tint(go, color);
        }

        private void Tint(GameObject go, Color color)
        {
            go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(color);
        }

        private Material MakeMaterial(Color color)
        {
            var mat = new Material(_baseMaterial);
            mat.color = color;
            return mat;
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

        /// <summary>Prisma heks bertinggi 1 satuan; skala Y menentukan tinggi tile.</summary>
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
