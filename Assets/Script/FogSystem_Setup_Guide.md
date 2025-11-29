# 2D Fog System Setup Guide

This guide explains how to set up the animated fog shader for your top-down 2D game.

## Files Created
- `AnimatedFog.shader` - URP shader for animated fog effect
- `FogController.cs` - Script to control fog properties at runtime
- `CameraShake.cs` - Bonus camera shake system (if needed)

## Setup Instructions

### Step 1: Create Fog Material

1. In Unity, go to your `Assets` folder
2. Right-click → **Create → Material**
3. Name it `FogMaterial`
4. In the Inspector, change **Shader** to `Custom/AnimatedFog2D`
5. Adjust the default properties:
   - **Fog Color**: Grayish-blue (R:0.7, G:0.7, B:0.8) for eerie effect
   - **Density**: 0.5 (adjust to taste)
   - **Scroll Speed**: X:0.02, Y:0.01 (slow movement)
   - **Noise Scale**: 1.5 (size of fog clouds)
   - **Layer Count**: 2 (more layers = more depth)

### Step 2: Create Fog Sprite

1. Create a **white square sprite** (or use any white texture):
   - In Unity: Right-click → **Create → Sprites → Square**
   - Or use an existing white texture

2. In your scene hierarchy:
   - Right-click → **2D Object → Sprite**
   - Name it `FogLayer`

3. Configure the sprite:
   - **Sprite Renderer**:
     - Assign your white sprite to **Sprite**
     - Assign `FogMaterial` to **Material**
     - Set **Sorting Layer** appropriately (above ground, below characters)
     - Set **Order in Layer** to control depth

4. **Scale** the sprite to cover your desired fog area:
   - For full-screen fog: Scale to cover the entire camera view
   - For area-specific fog: Scale to cover just that area

5. **Position** the sprite:
   - Z position should be appropriate for your 2D setup (usually 0 or slightly above ground)

### Step 3: Add FogController Script

1. Select your `FogLayer` GameObject
2. **Add Component → Fog Controller**
3. Configure settings in Inspector:
   - **Fog Density**: 0.5 (0 = invisible, 1 = opaque)
   - **Fog Color**: Choose your eerie color
   - **Scroll Speed**: Adjust for faster/slower fog movement
   - **Noise Scale**: Lower = larger fog clouds, Higher = smaller details
   - **Use Multiple Layers**: Check for more depth
   - **Layer Count**: 2-3 (more = more atmospheric but slightly slower)

### Step 4: Adjust Lighting for Eerie Effect

To maximize the eerie atmosphere:

1. **Global Light** (your main directional light):
   - Reduce **Intensity** to 0.3-0.5
   - Use cool color (slight blue tint)

2. **Add Point Lights** for limited visibility:
   - Create **2D Point Lights** in dark areas
   - Short **Outer Radius** (limited range)
   - Add **Falloff Intensity**
   - Consider adding flickering lights

3. **Optional Post-Processing**:
   - Add **Volume → Global Volume**
   - Enable **Vignette** (darkens edges)
   - Use **Color Adjustments** (desaturate, add blue tint)
   - Add **Bloom** for light glow through fog

## Usage from Code

### Basic Control

```csharp
// Access the fog controller (if using singleton)
FogController.Instance.SetDensity(0.8f);

// Change fog color
FogController.Instance.SetColor(new Color(0.3f, 0.3f, 0.4f));

// Change fog speed
FogController.Instance.SetScrollSpeed(new Vector2(0.05f, 0.02f));
```

### Fade In/Out

```csharp
// Fade fog in over 3 seconds to 80% density
FogController.Instance.FadeIn(0.8f, 3f);

// Fade fog out over 2 seconds
FogController.Instance.FadeOut(2f);

// Enable/disable fog
FogController.Instance.SetActive(false);
```

### Area-Specific Fog

If you want fog only in certain areas:

1. Create multiple fog sprites with `FogController`
2. Don't use singleton (remove `Instance` usage)
3. Reference each fog controller individually
4. Enable/disable as player enters/exits areas

```csharp
[SerializeField] private FogController eerieAreaFog;

void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        eerieAreaFog.FadeIn(0.7f, 2f);
    }
}
```

## Example: Eerie Forest Area

```csharp
public class EerieForestArea : MonoBehaviour
{
    [SerializeField] private FogController forestFog;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Player enters eerie area
            forestFog.SetColor(new Color(0.4f, 0.5f, 0.6f)); // Blue-gray
            forestFog.FadeIn(0.8f, 3f); // Heavy fog

            // Optionally adjust lighting
            // DayNightCycle.Instance.SetAmbientIntensity(0.3f);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Player leaves eerie area
            forestFog.FadeOut(2f);
        }
    }
}
```

## Tips for Best Results

### For Maximum Eeriness:
- **Fog Color**: Use desaturated blues/grays (avoid bright colors)
- **Density**: 0.6-0.8 for thick, mysterious fog
- **Lighting**: Very low ambient light (0.2-0.4)
- **Scroll Speed**: Slow (0.01-0.03) for ominous feel
- **Layer Count**: 2-3 layers for organic depth

### Performance:
- **Single Layer**: Fastest, good for mobile
- **2 Layers**: Good balance (recommended)
- **3 Layers**: Most atmospheric, slightly slower

### Layering:
- Fog should render **above** ground/background
- Fog should render **below** characters (so they're visible through fog)
- Use **Sorting Layers** to control this

## Troubleshooting

**Fog not visible:**
- Check Sorting Layer/Order in Layer
- Ensure Fog Density > 0
- Check sprite is scaled large enough
- Verify material is assigned correctly

**Fog too bright/dark:**
- Adjust Fog Color alpha
- Check Fog Density
- Adjust Global Light intensity

**Fog not animating:**
- Shader uses `_Time`, which auto-updates
- Check that the game is running (not paused)
- Verify Scroll Speed is not (0, 0)

**Performance issues:**
- Reduce Layer Count to 1
- Reduce sprite size (don't cover unnecessary areas)
- Use simpler noise scale

## Advanced: Multiple Fog Layers

For even more depth, create multiple fog sprites:

1. **Bottom Layer**: Large, slow-moving (Scale: 1.0, Speed: 0.01)
2. **Middle Layer**: Medium, moderate speed (Scale: 1.3, Speed: 0.02)
3. **Top Layer**: Small, fast details (Scale: 0.8, Speed: 0.03)

Set different sorting orders and densities for each.

---

Enjoy your eerie fog! 🌫️
