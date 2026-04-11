# OPZ MVP Audit Ledger

## 2026-04-10 — Task 01 (P0): Confirm active SelectionManager and reduce runtime ambiguity

### Evidence checked
- `OPZ.Core.SelectionManager` is referenced by core runtime systems (`GameBootstrap`, `CommandSystem`, `HUDController`, and `BuildPlacementController`).
- `GameBootstrap.RequireSingleton<SelectionManager>` explicitly provisions the Core version.
- No scene or asset references were found to the GUID of `Proto/SelectionManager.cs`.
- `Proto/SelectionManager` is isolated to `Proto/*` scripts and not wired into current core loop.

### Action taken
- Marked `Assets/Scripts/Proto/SelectionManager.cs` as **legacy/deprecated** with `System.Obsolete` and legacy AddComponent menu label.
- Added an explicit warning log in `Awake` to surface accidental use in runtime scenes.

### Conclusion
- Current official runtime selector is `OPZ.Core.SelectionManager`.
- `Proto/SelectionManager` is now clearly signposted as legacy and no longer competes silently with Core.

### Next smallest logical task
- Task 02 (P0): confirm active command system (`Proto/CommandGiver` vs `Core/CommandSystem`) and apply same legacy-signaling if confirmed.

## 2026-04-10 — Task 02 (P0): Confirm active command system and reduce runtime ambiguity

### Evidence checked
- `OPZ.Core.CommandSystem` is required by `GameBootstrap.RequireSingleton<CommandSystem>`.
- `CommandSystem` is integrated with `OPZ.Core.SelectionManager` and RTS runtime command priorities.
- `Proto/CommandGiver` only references `Proto/SelectionManager` + `Proto/Mover` pipeline.
- No scene or asset references were found to the GUID of `Proto/CommandGiver.cs`.

### Action taken
- Marked `Assets/Scripts/Proto/CommandGiver.cs` as **legacy/deprecated** with `System.Obsolete` and legacy AddComponent menu label.
- Added an explicit warning log in `Awake` to surface accidental use in runtime scenes.

### Conclusion
- Current official runtime command dispatcher is `OPZ.Core.CommandSystem`.
- `Proto/CommandGiver` is now clearly signposted as legacy and no longer competes silently with Core.

### Next smallest logical task
- Task 03 (P0): confirm which camera controller is active (`RTSCameraController` vs `SimpleRTSCamera`) before any consolidation.

## 2026-04-10 — Task 03 (P0): Confirm active camera controller and reduce runtime ambiguity

### Evidence checked
- `MAP_01_CinturaoDeRuina_Blockout.unity` references `RTSCameraController` script GUID (`929b45ce97bee8242a948484eeec42b7`).
- No scene reference was found to `SimpleRTSCamera` script GUID (`5d7fe896b6faf384587fbcbcf8cf24d1`).
- `MinimapController` explicitly seeks `RTSCameraController` as camera source.
- `SimpleRTSCamera` appears only in editor setup helpers, not as active runtime integration path.

### Action taken
- Marked `Assets/Scripts/Core/SimpleRTSCamera.cs` as **legacy/deprecated** with `System.Obsolete` and legacy AddComponent menu label.
- Added an explicit warning log in `Awake` to surface accidental use in runtime scenes.

### Conclusion
- Current official runtime camera is `OPZ.Core.RTSCameraController`.
- `SimpleRTSCamera` is now clearly signposted as legacy and no longer competes silently with Core camera flow.

### Next smallest logical task
- Task 04 (P0): validate `ProductionQueue.Enqueue()` null/edge path and patch only if runtime risk is confirmed.

## 2026-04-10 — Task 04 (P0): Validate and harden `ProductionQueue.Enqueue()` null/edge paths

### Evidence checked
- `ProductionQueue.Enqueue(UnitDef def)` previously used `def` and `_building` without null guards.
- `Progress` previously divided by `trainTime` directly, allowing edge behavior if `trainTime <= 0`.
- `FinishProduction()` previously instantiated `def.prefab` without verifying prefab assignment.

### Action taken
- Added guard clauses in `Enqueue` for `def == null`, missing `BuildingBase`, and missing `EconomyManager.Instance`.
- Hardened `Progress` to clamp and avoid division by zero (`Mathf.Max(0.01f, trainTime)`).
- Added defensive guards in `FinishProduction` for null `def` and null prefab.
- Added safe early return in `Refund` if dependencies are missing.

### Conclusion
- Critical null/edge paths around production enqueue/finish are now fail-safe with explicit logs instead of null-reference crashes.
- MVP loop risk reduced for production-related runtime failures.

### Next smallest logical task
- Task 05 (P1): verify potential double-registration behavior in `DepositPoint`.

## 2026-04-10 — Task 05 (P1): Verify and fix potential double-registration in `DepositPoint`

### Evidence checked
- `DepositPoint` previously called `TryRegister()` in both `OnEnable` and `Start`.
- `EconomyManager.RegisterDepot` avoids duplicates, but duplicate registration attempts still occurred lifecycle-wise.
- There was no internal flag in `DepositPoint` to indicate successful registration state.

### Action taken
- Replaced dual registration calls with a single state-driven flow.
- Added `_isRegistered` tracking to prevent repeated register/unregister churn.
- `OnEnable` now resets state and attempts register.
- `Update` retries registration only while not registered (covers manager-spawn ordering).
- `OnDisable` unregisters only when previously registered.

### Conclusion
- `DepositPoint` no longer performs duplicated registration attempts during normal lifecycle.
- Registration remains resilient when `EconomyManager.Instance` initializes after the depot object.

### Next smallest logical task
- Task 06 (P1): audit `WorkerUnit.ResumePreviousCommand` for stuck/loop edge cases and apply timeout fallback if needed.

## 2026-04-10 — Task 06 (P1): Audit `WorkerUnit.ResumePreviousCommand` and add fallback timeout

### Evidence checked
- `ResumeCommand()` only resumed gather when previous command was gather + valid node; otherwise worker went idle immediately.
- When fleeing/interrupting with carried resources, there was no timeout-backed fallback to retry depot return.
- `ReturnToDepot` and `Flee` both transitioned directly to `ResumePreviousCommand` without timer context.

### Action taken
- Added `resumeTimeout` (serialized) and `_resumeTimer` in `WorkerUnit`.
- Added `EnterResumeState()` helper so all transitions to resume initialize timeout consistently.
- Updated `UpdateReturn()` and `UpdateFlee()` to call `EnterResumeState()`.
- Extended `ResumeCommand()` fallback:
  - resume gather when previous node is valid;
  - if carrying resources, attempt nearest depot return;
  - if no depot is available, wait until timeout and then fail-safe to idle with warning.
- Added state cleanup (`_targetNode`, `_previousNode`, `_previousState`, `_resumeTimer`) on final idle fallback.

### Conclusion
- Worker resume behavior is now resilient to interrupt/depot edge cases and no longer drops into immediate ambiguous idle when carrying resources.
- Timeout fallback prevents indefinite resume churn when required runtime dependencies (depot) are temporarily unavailable.

### Next smallest logical task
- Task 07 (P1): evaluate whether legacy marker warnings should be editor-only to avoid runtime log noise during intentional proto test scenes.

## 2026-04-10 — Task 07 (P1): Make legacy marker warnings editor/dev-only

### Evidence checked
- Legacy components (`Proto.SelectionManager`, `Proto.CommandGiver`, `SimpleRTSCamera`) emitted `Debug.LogWarning` unconditionally in `Awake`.
- This could generate avoidable log noise in non-development runtime builds when legacy/proto scenes are intentionally loaded.

### Action taken
- Wrapped legacy warning logs with `#if UNITY_EDITOR || DEVELOPMENT_BUILD` in all three components.
- Kept `[System.Obsolete]` and legacy `AddComponentMenu` signposting unchanged.

### Conclusion
- Legacy signaling remains strong during development/editor workflows.
- Production runtime log noise from intentional legacy/proto usage is reduced.

### Next smallest logical task
- Task 08 (P1): evaluate extracting repeated magic numbers into a shared constants file with zero behavioral change.

## 2026-04-10 — Task 08 (P0 runtime issue): Units stuck at spawn corner (off-NavMesh safeguard)

### Evidence checked
- Reported symptom: units remain stuck near spawn square/corner and ignore move commands.
- `UnitBase.CommandMove()` and `MoveTo()` previously issued `SetDestination` directly with no guard/recovery when agent was off NavMesh.
- Spawn offsets and runtime instantiation can place agents slightly outside baked NavMesh, causing silent movement failure.

### Action taken
- Added NavMesh safety layer inside `UnitBase`:
  - `EnsureOnNavMesh()` tries `NavMesh.SamplePosition` + `Agent.Warp(...)` near current position.
  - `TrySetDestination(...)` centralizes movement command checks and only issues destination when agent is valid/on-navmesh.
  - `CommandMove()` and internal `MoveTo()` now use `TrySetDestination(...)`.
- Added one-time warning when a unit remains off NavMesh and command cannot be applied.
- Called `EnsureOnNavMesh()` in both `Awake()` and `InitUnit()` to cover scene-placed and runtime-spawned units.

### Conclusion
- Units spawned slightly off NavMesh now auto-snap to nearest valid NavMesh and can move normally.
- Movement commands now fail safely with actionable log feedback instead of silently freezing at spawn.

### Next smallest logical task
- Task 09 (P1): verify NavMesh coverage and spawn-point placement in scene/buildings to reduce need for runtime recovery.

## 2026-04-10 — Task 09 (P1): Improve production spawn placement on NavMesh

### Evidence checked
- Even with unit-side NavMesh recovery, spawn points near bounds/corners can still produce awkward initial placement.
- `ProductionQueue` previously used a single `NavMesh.SamplePosition` probe then fell back directly, with no multi-probe strategy around edge spawn points.

### Action taken
- Replaced single-probe spawn resolution with `ResolveSpawnPositionOnNavMesh(...)`.
- New flow:
  - probe desired spawn point on NavMesh using adaptive radius (based on prefab `NavMeshAgent` radius when available);
  - if miss, probe an 8-direction fallback ring around desired point;
  - if still miss, warn and keep desired position.
- `FinishProduction()` now routes all spawn position resolution through this helper.

### Conclusion
- Unit spawns are now more robust near map edges and building corners.
- Combined with `UnitBase` off-navmesh recovery, this reduces stuck-at-spawn occurrences from both spawn-side and unit-side failure paths.

### Next smallest logical task
- Task 10 (P1): review map/building spawn-point defaults in scene assets to align authoring with runtime spawn safety.

## 2026-04-10 — Task 10 (P0 runtime issue): Only a subset of units responding to move commands

### Evidence checked
- Reported symptom: only a small subset of characters move; others remain unresponsive.
- Existing off-navmesh recovery radius could be insufficient for units spawned/placed farther from baked NavMesh.
- Command path needed a stronger last-attempt recovery tied to commanded destination.

### Action taken
- Added configurable NavMesh recovery parameters in `UnitBase`:
  - `navMeshRecoveryRadius` (default 25f) for current-position recovery;
  - `destinationRecoveryRadius` (default 12f) for destination-based fallback.
- Enhanced `TrySetDestination(...)`:
  - if still off-navmesh after normal recovery, attempts `NavMesh.SamplePosition` near the commanded destination and warps there;
  - only aborts command when both recovery paths fail.
- Updated `EnsureOnNavMesh()` to use the larger configurable recovery radius.

### Conclusion
- Movement command acceptance is now more tolerant for units spawned/positioned farther from valid NavMesh.
- This should reduce the "only part of the units move" scenario by recovering more agents before command rejection.

### Next smallest logical task
- Task 11 (P1): add optional debug counters/telemetry for command-rejected units to quantify remaining off-navmesh cases.

## 2026-04-10 — Task 11 (P0 runtime issue): Click-to-move accepted only in limited map area

### Evidence checked
- Reported symptom and screenshot indicate movement orders only work in a small region.
- Previous move command required raycast hit strictly on `groundLayer` before issuing move.
- In scenes with mixed layers/colliders, many valid click positions can fail this filter even when nearby NavMesh exists.

### Action taken
- Added `TryResolveMovePoint(Ray ray, out Vector3 movePoint)` in `CommandSystem`.
- New move-point resolution pipeline:
  1. raycast on configured `groundLayer`;
  2. fallback raycast on any collider (`~0`);
  3. fallback projection to world XZ plane;
  4. final snap with `NavMesh.SamplePosition(..., 30f, NavMesh.AllAreas)`.
- Priority 5 move command now uses resolved NavMesh point instead of raw ground-layer hit point.

### Conclusion
- Click-to-move now works across a broader playable area and no longer depends exclusively on correct ground-layer tagging.
- Movement command path is consistent with NavMesh-based navigation constraints.

### Next smallest logical task
- Task 12 (P1): add optional gizmo/debug overlay for resolved move points and failed click reasons to speed map authoring QA.

## 2026-04-10 — Task 12 (P1): Add debug reason logging for failed move-point resolution

### Evidence checked
- Runtime move issues can come from multiple stages (raycast layer miss, no collider, no plane intersection, no nearby NavMesh).
- Previous pipeline only returned true/false, making field diagnosis slower.

### Action taken
- Added `debugMoveResolution` toggle in `CommandSystem` Inspector.
- `TryResolveMovePoint(...)` now returns both success/failure and a textual reason when it fails.
- `HandleRightClick(...)` now logs rejection reason when debug toggle is enabled.

### Conclusion
- Movement failures can now be diagnosed quickly in live scene runs without invasive debug code.
- This should accelerate pinpointing whether remaining issues are layer setup, geometry hit, or NavMesh coverage.

### Next smallest logical task
- Task 13 (P1): add lightweight editor gizmo for last resolved move point and last rejection candidate.

## 2026-04-10 — Task 13 (Process): Add GitHub automation scaffolding to reduce manual flow

### Evidence checked
- Current workflow required many manual steps and caused friction for mobile-based merges.
- Repository had no CI workflow, PR template, or central automation setup guide.

### Action taken
- Added `.github/workflows/unity-ci.yml` with Unity CI path and secret-aware fallback messaging.
- Added `.github/pull_request_template.md` with MVP validation checklist.
- Added `Docs/DevOps/GITHUB_AUTOMATION_SETUP.md` with step-by-step setup:
  - Unity secrets
  - branch protection
  - auto-merge
  - daily Desktop+Mobile flow

### Conclusion
- Repository now has baseline automation artifacts to reduce merge bureaucracy and standardize validation.
- Team can enable CI + protected branches once secrets are configured.

### Next smallest logical task
- Task 14 (P1): add a lightweight CI job for non-Unity checks (markdown/yaml lint) that runs even without Unity secrets.

## 2026-04-11 — Task 14 (UX): Silence legacy warnings by default

### Evidence checked
- Runtime/editor logs were showing legacy warnings for `Proto.SelectionManager`, `Proto.CommandGiver`, and `SimpleRTSCamera`.
- Warnings were useful during migration, but noisy for normal testing once legacy status was already known.

### Action taken
- Added serialized toggle `showLegacyWarning` in all three legacy scripts.
- Legacy warnings now only log when this toggle is manually enabled in Inspector.
- Deprecated markers (`[System.Obsolete]` + legacy component menu names) remain in place.

### Conclusion
- Console noise is reduced for regular gameplay testing.
- Teams can re-enable warnings per-object when auditing legacy scene usage.

### Next smallest logical task
- Task 15 (P1): add a one-time scene validator utility that lists legacy components without spamming runtime logs.
