# SurvivalFPSGame Project Context

Last updated: 2026-04-17

This file is a handoff document for future sessions. It summarizes the current implementation state, important files, known issues, and next actions.

## Project Overview

- Unity survival FPS project.
- Main gameplay scene is `Assets/Scenes/map.unity`.
- `Assets/Scenes/Level.unity` is a setup/menu-oriented scene, but `LevelMenuRuntime` has been extended so the same runtime menu also works in `map`.
- The project already contains multiple weapon prefabs, enemy spawn controllers, HUD, and save/load menu code.

## What Was Implemented

### Combat and Feedback

- Melee weapon support was added in code.
- Hit marker feedback was added for successful hits.
- Hit sound is triggered on successful hits.
- Bullet hit detection was made more robust by checking both collision and trigger hit paths.
- Bullet effects now have fallback runtime particles if prefab references are missing.

### Enemy Drops

- Enemy death can spawn an AmmoBox with 5% probability.
- AmmoBox drop now has launch force so it visually flies out when spawned.
- Drop configuration is currently on `AxeZomb`.

### Menu / Scene Handoff

- `LevelMenuRuntime` now works in both `Level` and `map` scenes.
- The runtime menu is created automatically, so `map` does not need a manually copied menu Canvas.

## Important Files

### Combat and Enemy Flow

- `Assets/Scripts/Weapon.cs`
- `Assets/Scripts/Bullet.cs`
- `Assets/Scripts/AxeZomb.cs`
- `Assets/Scripts/WeaponManager.cs`
- `Assets/Scripts/SoundManager.cs`
- `Assets/Scripts/HUDManager.cs`
- `Assets/Scripts/InteractionManager.cs`

### Menu / Runtime UI

- `Assets/Scripts/GameMenuController.cs`
- `Assets/Scripts/LevelMenuRuntime.cs`
- `Assets/Scripts/PlayerHealth.cs`

### Scene / Map

- `Assets/Scenes/map.unity`
- `Assets/Scenes/Level.unity`
- `Assets/Scenes/map/NavMesh-NavMesh Surface.asset`

### Prefabs

- `Assets/Prefabs/AxeZomb.prefab`
- `Assets/Prefabs/Bullet.prefab`
- `Assets/Prefabs/Effect/BulletImpactEffect.prefab`
- `Assets/Prefabs/Effect/BloodSprayFX.prefab`

## Current Behavior

### Weapon / Bullet

- Weapons instantiate `Bullet` and assign `bulletDamage`.
- `Bullet` resolves enemy hits more defensively than before.
- If `GlobalReferences.bloodEffectPrefab` or `bulletImpactEffectPrefab` is missing, the bullet code now creates fallback runtime particle effects instead of crashing.

### Enemy Death

- `AxeZomb.TakeDamage()` destroys the zombie after a death animation delay.
- On death, it adds score.
- On death, it tries to spawn an AmmoBox if the 5% roll succeeds.
- AmmoBox drop prefab and impulse are serialized on `AxeZomb`.

### Menu

- `LevelMenuRuntime` shows the pause/start/game-over UI in `Level` and `map`.
- `GameMenuController` handles save, load, retry, exit, and scene switching.

## Known Setup Required In Unity

These are still important because some behavior depends on Inspector references:

- `AxeZomb.prefab` has `ammoBoxDropPrefab` currently unset and must be assigned to the AmmoBox prefab you want to drop.
- `GlobalReferences` should ideally have its effect prefabs assigned, even though fallback effects now prevent crashes.
- `SoundManager` should have hit marker and melee sound clips assigned if you want custom audio.
- Recommended tiny hit-tick sound asset for the "Tạch" feedback: `Assets/Voices - Essentials/Voice_Male/Voice_Male_Hit/Voice_Male_V1_Hit_Short_Mono_01.wav`.
- If you want a softer alternative, try the female hit short clip with the same naming pattern.
- If you want a dedicated melee icon in HUD, add or verify a `Melee_Weapon` sprite resource.

## Current Risks / Things To Verify

- Bullet damage was the main place where enemy hits could fail if collision/trigger hierarchy did not match expectations. This was hardened, but it should still be tested in `map`.
- `LevelMenuRuntime` now supports `map`, but the scene must still load the script at runtime correctly.
- The AmmoBox drop is chance-based, so you need to verify the prefab is configured with a collider and pickup behavior that matches the existing interaction flow.

## Suggested Next Steps

1. Open `map.unity` and verify enemy bullets reduce health and trigger hit feedback.
2. Assign `AmmoBox` prefab to `AxeZomb.prefab` if not already set.
3. Verify `GlobalReferences` has effect prefabs assigned in the scene.
4. Test the runtime menu in `map` with Escape, death, and restart flows.
5. If needed, do a small pass on the melee weapon prefab setup in the scene.

## Notes For Future Sessions

- Keep edits minimal and preserve current Unity scene structure.
- Avoid large manual scene YAML edits unless necessary.
- Prefer runtime bootstrapping for shared UI/menu logic.
- If a prefab reference is missing, prefer an inspector fix first, but fallback code is acceptable when it prevents runtime crashes.
