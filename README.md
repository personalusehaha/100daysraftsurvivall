# 100 Days Raft Survival

A 3D Android survival game built in Unity 6 (URP). Survive on the open ocean,
collect floating resources, build up your raft, and explore tropical islands.

## Status
🚧 Phase 1 in development — Ocean, Raft, Player, Resources, Islands, Day/Night cycle.

## Requirements
- Unity 6000.x LTS with Universal Render Pipeline (URP)
- Android Build Support module
- A PC/Mac (or cloud Unity instance) to open and build the project —
  Unity Editor does not run on Android phones.

## Project Structure
See `Assets/Scripts/` for all gameplay code, organized by system:
- `Core/` – Game state, day/night cycle
- `Player/` – Movement, swimming, camera
- `Raft/` – Floating raft physics, climbing
- `Ocean/` – Gerstner wave ocean simulation
- `Resources/` – Floating collectible items
- `Islands/` – Procedural island spawning
- `Environment/` – Wind, birds, water particles
- `Audio/` – Ambient ocean sound management

## Setup (once you have Unity open on a PC/Mac)
1. Create a new **3D (URP)** project in Unity 6.
2. Clone/copy this repo's `Assets/Scripts` folder into your project's `Assets/`.
3. Follow the **Asset Requirements** notes in each script's section to import
   or create the needed 3D models, textures, and audio clips.
4. Create a scene named `MainOcean`, add the components as described.
5. Switch platform to Android (File > Build Settings > Android).

## Phase 1 Scope
- Realistic animated ocean (Gerstner waves)
- Floating wooden raft
- Third-person player controller + surface-only swimming
- Climb raft from water
- Floating resources (planks, logs, metal, plastic, rope, fish, barrels, crates, coin chests)
- Procedural tropical islands (palm trees, huts, docks, caves, etc.)
- Dynamic day/night cycle (5 min/day)
- Ambient ocean audio, birds, wind, water particles
- Mobile (Android) performance optimizations
