# Camera Shake System - Setup & Testing Guide

## Quick Setup

### Step 1: Setup CameraShake Component

1. Create an empty GameObject in your scene (or use an existing manager)
   - Name it `CameraShakeManager`

2. Add the `CameraShake` component to it

3. In the Inspector:
   - **Virtual Camera**: Assign your Cinemachine Virtual Camera
     - If left empty, it will auto-find one
   - **Default Duration**: 0.3s (adjust to preference)
   - **Default Intensity**: 1.5 (adjust to preference)

4. The component will automatically add/find `CinemachineBasicMultiChannelPerlin` noise component

### Step 2: Test with Built-in Context Menu

The easiest way to test:

1. Select the GameObject with `CameraShake` component
2. Right-click on the component in Inspector
3. Choose from the context menu:
   - `Test Shake - Default`
   - `Test Shake - Light`
   - `Test Shake - Medium`
   - `Test Shake - Heavy`
   - `Test Shake - Explosion`

---

## Testing Tools

You now have **3 ways** to test camera shake:

### 🎮 Method 1: Runtime Keyboard Shortcuts

Add `CameraShakeTester` component to any GameObject:

**Keyboard Shortcuts (in Play Mode):**
- `1` - Light Shake
- `2` - Medium Shake
- `3` - Heavy Shake
- `4` - Explosion Shake
- `5` - Custom Shake (adjust settings in Inspector)
- `0` or `ESC` - Stop Shake

**Features:**
- On-screen instructions
- Customizable shake parameters
- Optional UI buttons (assign a Canvas)

### 🖼️ Method 2: Editor Window

Access via: **Window → Camera Shake Tester**

**Features:**
- Clean UI for testing all shake types
- Shows preset parameters
- Custom shake with sliders
- Works in Play Mode
- Real-time feedback

### 📋 Method 3: Context Menu

Right-click `CameraShakeTester` component:
- `Test - Light Shake`
- `Test - Medium Shake`
- `Test - Heavy Shake`
- `Test - Explosion Shake`
- `Test - Custom Shake`
- `Stop Shake`

---

## Usage in Code

### Basic Usage

```csharp
// Simple shake
CameraShake.Instance.Shake();

// Custom shake
CameraShake.Instance.Shake(duration: 0.5f, intensity: 3.0f);

// Preset shakes
CameraShake.Instance.ShakeLight();
CameraShake.Instance.ShakeMedium();
CameraShake.Instance.ShakeHeavy();
CameraShake.Instance.ShakeExplosion();

// Stop shake
CameraShake.Instance.StopShake();
```

### Example: Dam Explosion Scene

```csharp
// In your GameSystemsManager.cs or event handler
FlagMonitorSystem.WatchFlagAdded("dam_broken", () =>
{
    // Camera shake
    CameraShake.Instance.ShakeExplosion();

    // Play explosion sound
    // audioSource.PlayOneShot(explosionSound);

    // Disable player movement temporarily
    // player.DisableMovement(1.5f);

    // Show monologue after shake
    // StartCoroutine(ShowMonologueAfterDelay(1.0f));

    questManager.StartQuest("investigate_dam_destruction");
});
```

### Example: Player Taking Damage

```csharp
public void TakeDamage(int damage)
{
    health -= damage;

    // Light shake when hit
    CameraShake.Instance.ShakeLight();

    // Heavier shake if critical hit
    if (damage > 50)
    {
        CameraShake.Instance.ShakeHeavy();
    }
}
```

### Example: Boss Attack

```csharp
public void BossGroundSlam()
{
    // Heavy shake for impact
    CameraShake.Instance.ShakeHeavy();

    // Custom shake with specific parameters
    // CameraShake.Instance.Shake(1.2f, 5.0f, 2.0f);
}
```

---

## Shake Presets Guide

### Light Shake
- **Duration**: 0.2s
- **Intensity**: 0.8
- **Good For**:
  - Small impacts
  - Item pickup
  - Light footsteps
  - UI feedback

### Medium Shake
- **Duration**: 0.3s
- **Intensity**: 1.5
- **Good For**:
  - Player taking damage
  - Enemy hit/killed
  - Door opening/closing
  - Spell cast

### Heavy Shake
- **Duration**: 0.5s
- **Intensity**: 3.0
- **Good For**:
  - Large enemy defeated
  - Boss attack
  - Heavy object falling
  - Bridge collapse

### Explosion Shake
- **Duration**: 0.8s
- **Intensity**: 4.0
- **Good For**:
  - Explosions
  - Building destruction (your dam!)
  - Major story events
  - Earthquake effects

---

## Customization

### Adjust Presets in Inspector

1. Select GameObject with `CameraShake` component
2. Expand preset categories:
   - **Light Shake**: Adjust duration/intensity
   - **Medium Shake**: Adjust duration/intensity
   - **Heavy Shake**: Adjust duration/intensity
   - **Explosion Shake**: Adjust duration/intensity

### Custom Shake Parameters

```csharp
float duration = 0.5f;    // How long shake lasts (seconds)
float intensity = 2.0f;   // Shake amplitude/strength
float frequency = 1.0f;   // Shake speed (higher = faster)

CameraShake.Instance.Shake(duration, intensity, frequency);
```

**Parameter Guidelines:**
- **Duration**: 0.1s - 2.0s (short to long)
- **Intensity**: 0.5 - 5.0 (subtle to extreme)
- **Frequency**: 0.5 - 3.0 (slow to fast)

---

## Troubleshooting

### Shake Not Working

**Problem**: Calling shake but nothing happens

**Solutions**:
1. Check that `CameraShake` component exists in scene
2. Ensure Virtual Camera is assigned
3. Check that Cinemachine is installed
4. Verify camera has `CinemachineBasicMultiChannelPerlin` component
5. Check Console for error messages

### Shake Too Subtle/Strong

**Problem**: Shake intensity doesn't feel right

**Solutions**:
1. Adjust intensity values in Inspector presets
2. Test with Editor Window to find good values
3. Consider camera distance/zoom level
4. Try different frequency values

### Shake Doesn't Stop

**Problem**: Shake continues indefinitely

**Solutions**:
1. Call `CameraShake.Instance.StopShake()`
2. Check for multiple overlapping shake calls
3. Verify coroutines are completing properly

### Performance Issues

**Problem**: Shake causes frame drops

**Solutions**:
1. Reduce shake frequency
2. Avoid calling shake every frame
3. Use lighter shake presets
4. Check Cinemachine settings

---

## Advanced Tips

### Combining with Other Effects

```csharp
public void CreateImpact()
{
    // Camera shake
    CameraShake.Instance.ShakeHeavy();

    // Particle effect
    Instantiate(impactParticles, impactPosition, Quaternion.identity);

    // Sound effect
    AudioSource.PlayClipAtPoint(impactSound, impactPosition);

    // Screen flash
    // ScreenFlash.Instance.Flash(Color.white, 0.1f);

    // Time slow
    // Time.timeScale = 0.5f;
    // Invoke("ResetTimeScale", 0.2f);
}
```

### Directional Shake

For more advanced effects, you can modify the noise component directly:

```csharp
var noise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
// Adjust specific axes for directional effects
```

### Randomized Shake

```csharp
public void RandomShake()
{
    float randomIntensity = Random.Range(1.0f, 3.0f);
    float randomDuration = Random.Range(0.2f, 0.5f);
    CameraShake.Instance.Shake(randomDuration, randomIntensity);
}
```

---

## Integration with Your Game

### Story Events
```csharp
// Water crisis discovery
CameraShake.Instance.ShakeLight();

// Dam construction complete
CameraShake.Instance.ShakeMedium();

// Dam destruction
CameraShake.Instance.ShakeExplosion();

// Mbok Randa anger
CameraShake.Instance.ShakeHeavy();
```

### Combat
```csharp
// Player hit
CameraShake.Instance.ShakeLight();

// Enemy defeated
CameraShake.Instance.ShakeMedium();

// Boss phase transition
CameraShake.Instance.ShakeHeavy();
```

---

Enjoy your camera shake effects! 📹✨
