using UnityEngine;

/// <summary>
/// Simple dice face builder: attaches pip spheres to each face of the cube.
/// No custom meshes, no textures. Just spheres as dots.
///
/// Face layout: +Y=1  -Y=6  +X=2  -X=5  +Z=3  -Z=4
/// Face 1 pip is RED (casino convention).
/// </summary>
public class DiceFaceBuilder : MonoBehaviour
{
    private static Shader s_cachedShader;
    private static Material s_whiteMaterial;
    private static Material s_darkMaterial;
    private static Material s_redMaterial;

    [SerializeField] private float pipScale  = 0.15f;   // size of each dot sphere
    [SerializeField] private float pipOffset = 0.502f;  // how far from centre (just outside face)

    // Pip positions per face; each Vector2 is local (x,y) offset on that face
    private static readonly Vector2[][] Pips = {
        new[]{ new Vector2( 0,     0)    },                                                                     // 1
        new[]{ new Vector2(-.28f,  .28f), new Vector2( .28f, -.28f) },                                         // 2
        new[]{ new Vector2(-.28f,  .28f), new Vector2(  0f,   0f),  new Vector2( .28f, -.28f) },               // 3
        new[]{ new Vector2(-.28f,  .28f), new Vector2( .28f,  .28f), new Vector2(-.28f, -.28f), new Vector2(.28f, -.28f) }, // 4
        new[]{ new Vector2(-.28f,  .28f), new Vector2( .28f,  .28f), new Vector2(0f, 0f),
               new Vector2(-.28f, -.28f), new Vector2( .28f, -.28f) },                                         // 5
        new[]{ new Vector2(-.28f,  .30f), new Vector2( .28f,  .30f),
               new Vector2(-.28f,   0f), new Vector2( .28f,   0f),
               new Vector2(-.28f, -.30f), new Vector2( .28f, -.30f) },                                         // 6
    };

    // (faceValue, outward-normal, right-axis, up-axis)
    private static readonly (int val, Vector3 n, Vector3 r, Vector3 u)[] Faces = {
        (1,  Vector3.up,       Vector3.right,    Vector3.forward ),
        (6, -Vector3.up,       Vector3.right,   -Vector3.forward ),
        (2,  Vector3.right,    Vector3.forward,  Vector3.up      ),
        (5, -Vector3.right,   -Vector3.forward,  Vector3.up      ),
        (3,  Vector3.forward, -Vector3.right,    Vector3.up      ),
        (4, -Vector3.forward,  Vector3.right,    Vector3.up      ),
    };

    private void Awake() => BuildPips();

    public void BuildPips()
    {
        // Clear old pips
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        EnsureMaterials();

        // Apply white to the dice body
        var mr = GetComponent<MeshRenderer>();
        if (mr) mr.sharedMaterial = s_whiteMaterial;

        foreach (var (val, n, r, u) in Faces)
        {
            var mat = (val == 1) ? s_redMaterial : s_darkMaterial;
            foreach (var uv in Pips[val - 1])
            {
                Vector3 pos = n * pipOffset + r * uv.x + u * uv.y;

                var pip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pip.name = $"Pip_{val}";
                pip.transform.SetParent(transform, false);
                pip.transform.localPosition = pos;
                pip.transform.localScale    = Vector3.one * pipScale;

                Destroy(pip.GetComponent<SphereCollider>());
                pip.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
        }
    }

    private static void EnsureMaterials()
    {
        if (s_whiteMaterial != null && s_darkMaterial != null && s_redMaterial != null)
            return;

        s_cachedShader ??= Shader.Find("Universal Render Pipeline/Lit")
            ?? Shader.Find("Standard")
            ?? Shader.Find("Diffuse");

        s_whiteMaterial = MakeMat(new Color(0.96f, 0.96f, 0.96f));
        s_darkMaterial = MakeMat(new Color(0.08f, 0.08f, 0.08f));
        s_redMaterial = MakeMat(new Color(0.85f, 0.08f, 0.08f));
    }

    private static Material MakeMat(Color c)
    {
        return new Material(s_cachedShader) { color = c };
    }
}

