using UnityEngine;

/// <summary>
/// Attach this to a dedicated orthographic camera whose "Output Texture" is set
/// to a RenderTexture. That RenderTexture is then used as the source image on
/// the minimap RawImage in the Canvas.
///
/// The camera follows the player every LateUpdate so it stays centred on them.
///
/// Setup:
///   1. Create a new Camera in the scene and name it "Minimap Camera".
///   2. Set its Culling Mask to only include the layers you want on the map
///      (add a "Minimap" layer for icons you only want to appear on the map).
///   3. Set the camera to Orthographic.
///   4. Create a RenderTexture asset (Assets → Create → Render Texture).
///      Recommended size: 256 × 256 or 512 × 512.
///   5. Drag that RenderTexture into the camera's "Output Texture" field.
///   6. Add THIS component to that camera.
///   7. Assign the Player transform (or leave blank — it will find it).
///   8. In the Canvas, add a RawImage and drag the same RenderTexture into its Texture field.
/// </summary>
[RequireComponent(typeof(Camera))]
public class MinimapCamera : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  INSPECTOR
    // ─────────────────────────────────────────────
    [Header("Target")]
    [Tooltip("The transform the minimap should follow. Leave blank to auto-find Player.")]
    [SerializeField] private Transform target;

    [Header("View")]
    [Tooltip("World-units visible from centre to edge (half the visible height).")]
    [SerializeField] private float orthographicSize = 10f;

    [Header("Z Depth")]
    [Tooltip("Z position of the camera. Must be NEGATIVE in a 2D game so the camera\n"
             + "looks toward +Z where your sprites sit (e.g. -10 is the same as your Main Camera).")]
    [SerializeField] private float cameraZ = -10f;

    // ─────────────────────────────────────────────
    //  INTERNALS
    // ─────────────────────────────────────────────
    private Camera minimapCam;

    // ─────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────
    private void Awake()
    {
        minimapCam = GetComponent<Camera>();
        minimapCam.orthographic = true;
        minimapCam.orthographicSize = orthographicSize;

        // Auto-find player if not assigned
        if (target == null)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                target = player.transform;
                Debug.Log("[MinimapCamera] Target auto-assigned to Player.");
            }
            else
            {
                Debug.LogWarning("[MinimapCamera] No target assigned and no Player found in scene!");
            }
        }
    }

    /// <summary>LateUpdate ensures the camera catches the player's final position that frame.</summary>
    private void LateUpdate()
    {
        if (target == null) return;

        // Follow player XY but keep our own Z so we always look toward the sprites.
        // cameraZ should be negative (e.g. -10) so the camera faces +Z toward Z=0 sprites.
        Vector3 pos = target.position;
        pos.z = cameraZ;
        transform.position = pos;
    }

    // ─────────────────────────────────────────────
    //  PUBLIC API
    // ─────────────────────────────────────────────

    /// <summary>Zoom in/out at runtime (e.g. from a settings menu).</summary>
    public void SetZoom(float newSize)
    {
        orthographicSize = Mathf.Max(1f, newSize);
        minimapCam.orthographicSize = orthographicSize;
    }

    /// <summary>Override the follow target at runtime.</summary>
    public void SetTarget(Transform newTarget) => target = newTarget;
}
