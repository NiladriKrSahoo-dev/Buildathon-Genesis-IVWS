<p align="center">
  <img src="sins_logo.png" alt="SINS Logo" width="400"/>
</p>

<h1 align="center">S I N S</h1>
<p align="center">
  <em>A First-Person Psychological Horror Investigation Game</em>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Engine-Unity%206-000000?style=for-the-badge&logo=unity" alt="Unity"/>
  <img src="https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=csharp" alt="C#"/>
  <img src="https://img.shields.io/badge/Genre-Psychological%20Horror-8B0000?style=for-the-badge" alt="Horror"/>
  <img src="https://img.shields.io/badge/Status-In%20Development-orange?style=for-the-badge" alt="Status"/>
</p>

---

## 🎮 About

**SINS** is a first-person psychological horror game set inside a single, claustrophobic room. You wake up with no memory of what happened — but the room remembers everything. Piece together the dark truth by examining evidence, playing cassette recordings, and unlocking secrets hidden in the furniture, walls, and shadows.

> *"He wasn't always like this. But since the accident, he talks to people who aren't there."*

---

## 🕹️ Gameplay

| Feature | Description |
|---------|-------------|
| **🔍 Evidence Investigation** | Examine a whiskey bottle, torn photographs, burned diary pages, and more — each revealing fragments of a disturbing story |
| **📼 Cassette Tape System** | Find and play cassette tapes on the gramophone to hear recorded confessions and memories |
| **🔑 Puzzle Solving** | Discover hidden keys, unlock desk drawers, and uncover what's been sealed away |
| **🚪 Locked Room Mystery** | The front door is locked. You can't leave until you confront the truth |
| **🎵 Atmospheric Audio** | Gramophone music, environmental ambience, and voice narration build tension throughout |
| **💡 Dynamic Lighting** | Antique chandelier with warm flickering light creates an unsettling, lived-in atmosphere |

---

## 🎯 Controls

| Key | Action |
|-----|--------|
| `W A S D` | Move |
| `Mouse` | Look around |
| `E` | Interact with objects |
| `ESC` | Pause menu |

---

## 📖 Story Synopsis

You find yourself inside a dimly lit room — a room that clearly belongs to someone troubled. An empty bourbon bottle sits on the desk beside a stained note dripping with guilt. A photograph from an anniversary trip has your face violently gouged out with a razor blade. A half-burned diary page whispers of accidents and people who aren't there.

As you investigate deeper — unlocking drawers with hidden keys, playing recorded confessions on an old gramophone — the pieces of a fractured life begin to assemble into something far more sinister than you expected.

**The room is your confession booth. The evidence is your jury.**

---

## 🏗️ Technical Architecture

```
Assets/
├── GameManager.cs              # Core game state, objectives, inventory management
├── PlayerController.cs         # First-person movement and mouse look
├── EvidenceManager.cs          # Spawns and positions evidence items in the room
├── EvidenceItem.cs             # Individual evidence interaction + caption display
├── DeskInvestigation.cs        # Locked desk drawer puzzle system
├── KeyPickup.cs                # Brass key discovery and inventory
├── TapePickup.cs               # Cassette tape collection
├── TapePlayer.cs               # Gramophone tape playback system
├── GramophoneController.cs     # Background music player
├── Door.cs                     # Locked door interaction
├── ClockController.cs          # Wall clock with real-time ticking
├── CeilingLampManager.cs       # Dynamic chandelier lighting + room ambience
├── RoomStateManager.cs         # Master room setup orchestrator
├── ModernHUDManager.cs         # In-game HUD (objectives, captions, interact prompts)
├── IntroScreenManager.cs       # Cinematic title screen with fade-in logo
├── InteractPromptUI.cs         # "Press E" proximity prompt system
└── Scenes1/
    └── MAIN.unity              # Primary game scene
```

---

## 🎨 Key Features

### Cinematic Intro
The game opens with a dramatic title card — the **SINS** blood-splatter logo fades in from darkness, followed by the tagline *"What happened in this room?"*, before fading out into the game world.

### Evidence System
Each piece of evidence has:
- **Proximity detection** — "Press E to Interact" appears when you approach
- **Caption overlay** — Detailed narration text appears on screen
- **Progressive storytelling** — Each item reveals a new fragment of the truth

### Atmospheric Lighting
- Warm antique chandelier with subtle flicker creates a horror ambience
- Balanced directional light keeps the room visible while maintaining shadows
- Emissive materials on chandelier arms simulate glowing filament bulbs

### Locked Room Progression
1. Find the **cassette tape** → Play it on the **gramophone**
2. Examine **evidence items** scattered around the room
3. Find the **hidden brass key** → Unlock the **desk drawers**
4. Discover the final truth → Attempt to **open the door**

---

## 🚀 Getting Started

### Prerequisites
- **Unity 6** (6000.x or later)
- **TextMeshPro** package (included with Unity)

### Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/NiladriKrSahoo-dev/Buildathon-Genesis-IVWS.git
   ```
2. Open the project in **Unity Hub** → **Unity 6**
3. Open `Assets/Scenes1/MAIN.unity`
4. Press **Play** ▶️

---

## 👥 Team

Built for the **Genesis Buildathon** hackathon.

---

## 📄 License

This project is developed as part of a hackathon submission. All 3D assets are sourced from the Unity Asset Store and respective licenses apply.

---

<p align="center">
  <strong>🩸 Every room has a story. This one has a confession. 🩸</strong>
</p>