<div align="center">

<img width="220" height="220" alt="SINS logo" src="https://github.com/user-attachments/assets/25f0df07-10e4-4b50-b89c-1716219990d9" />

# S I N S

### *A First-Person Psychological Horror Investigation Game*

<img src="https://img.shields.io/badge/Engine-Unity%206-000000?style=for-the-badge&logo=unity" alt="Unity"/>
<img src="https://img.shields.io/badge/Language-C%23-239120?style=for-the-badge&logo=csharp" alt="C#"/>
<img src="https://img.shields.io/badge/Genre-Psychological%20Horror-8B0000?style=for-the-badge" alt="Horror"/>
<img src="https://img.shields.io/badge/Status-In%20Development-orange?style=for-the-badge" alt="Status"/>
<br/>
<img src="https://img.shields.io/badge/Theme-ONE%20ROOM-6a0dad?style=for-the-badge" alt="Theme"/>
<img src="https://img.shields.io/badge/Event-Genesis%20Buildathon-48cae4?style=for-the-badge" alt="Genesis Buildathon"/>

**• [About](#-about) • [Gameplay](#️-gameplay) • [Controls](#-controls) • [Story](#-story-synopsis) • [Storyboard](#️-storyboard-flowchart) • [Architecture](#️-technical-architecture) • [Getting Started](#-getting-started) • [Credits](#-asset-credits--third-party-licenses) •**

</div>

<br/>

> 🩸 *"He wasn't always like this. But since the accident, he talks to people who aren't there."*

<br/>

## 🎮 About

**SINS** is a first-person psychological horror game set inside a single, claustrophobic room. You wake up with no memory of what happened — but the room remembers everything. Piece together the dark truth by examining evidence, playing cassette recordings, and unlocking secrets hidden in the furniture, walls, and shadows.

<br/>

## 🕹️ Gameplay

<div align="center">

| Feature | Description |
|:---:|---|
| 🔍 **Evidence Investigation** | Examine a whiskey bottle, torn photographs, burned diary pages, and more — each revealing fragments of a disturbing story |
| 📼 **Cassette Tape System** | Find and play cassette tapes on the gramophone to hear recorded confessions and memories |
| 🔑 **Puzzle Solving** | Discover hidden keys, unlock desk drawers, and uncover what's been sealed away |
| 🚪 **Locked Room Mystery** | The front door is locked. You can't leave until you confront the truth |
| 🎵 **Atmospheric Audio** | Gramophone music, environmental ambience, and voice narration build tension throughout |
| 💡 **Dynamic Lighting** | Antique chandelier with warm, flickering light creates an unsettling, lived-in atmosphere |

</div>

<br/>

## 🎯 Controls

<div align="center">

| Key | Action |
|:---:|:---|
| <kbd>W</kbd> <kbd>A</kbd> <kbd>S</kbd> <kbd>D</kbd> | Move |
| 🖱️ Mouse | Look around |
| <kbd>E</kbd> | Interact with objects |
| <kbd>Esc</kbd> | Pause menu |

</div>

<br/>

## 📖 Story Synopsis

You find yourself inside a dimly lit room — a room that clearly belongs to someone troubled. An empty bourbon bottle sits on the desk beside a stained note dripping with guilt. A photograph from an anniversary trip has your face violently gouged out with a razor blade. A half-burned diary page whispers of accidents and people who aren't there.

As you investigate deeper — unlocking drawers with hidden keys, playing recorded confessions on an old gramophone — the pieces of a fractured life begin to assemble into something far more sinister than you expected.

> ### 🩸 The room is your confession booth. The evidence is your jury.

<br/>

## 🗺️ Storyboard Flowchart

<div align="center"><em>Narrative progression through the game's seven scenes</em></div>
<br/>

```mermaid
graph TD
    A["<b>Scene 1: The Awakening</b><br/>• Pitch-black silence fades out<br/>• Player wakes in warm, nostalgic vintage room<br/>• WASD movement prompt appears in top-left corner"] --> B["<b>Scene 2: The Denial (Tape 1)</b><br/>• Objective: Find and listen to cassette tape<br/>• Pick up Tape 1 from coffee table<br/>• Play on tape deck: Calm rationalization monologue"]

    B --> C["<b>Scene 3: The Search for Escape</b><br/>• Objective: Search desk drawers for Tape 2<br/>• Open vintage study desk drawer<br/>• Retrieve Tape 2: 'VHSTape_WifeLore'"]

    C --> D["<b>Scene 4: The Confrontation (Tape 2)</b><br/>• Insert Tape 2 into player<br/>• Television flashes with violent static<br/>• Accusatory voice: <i>'Your time is officially up...'</i>"]

    D --> E["<b>Scene 5: The Collapse of Reality</b><br/>• Wall clock hands spin wildly out of control<br/>• Ceiling chandelier strobes and flickers<br/>• Procedural wave dissolve: Furniture, books, TV disintegrate into void"]

    E --> F["<b>Scene 6: The Unforgivable Act</b><br/>• Room transforms into cold, pitch-black crime scene<br/>• Confront victim body and murder weapon on floorboards<br/>• Retrieve front door brass key lying beside victim"]

    F --> G["<b>Scene 7: The Final Revelation &amp; Escape</b><br/>• Unlock front door with retrieved brass key<br/>• Door opens into blinding white light<br/>• Final Message: <i>'You locked the door from the inside.'</i>"]

    style A fill:#1e1e24,stroke:#4a4a5a,stroke-width:2px,color:#fff
    style B fill:#2b261f,stroke:#d4a373,stroke-width:2px,color:#fff
    style C fill:#2b261f,stroke:#d4a373,stroke-width:2px,color:#fff
    style D fill:#4a1e1e,stroke:#e63946,stroke-width:2px,color:#fff
    style E fill:#4a1e1e,stroke:#e63946,stroke-width:2px,color:#fff
    style F fill:#16161a,stroke:#7209b7,stroke-width:2px,color:#fff
    style G fill:#0d1b2a,stroke:#48cae4,stroke-width:2px,color:#fff
```

<br/>

## 🎨 Key Features

### 🌑 Atmospheric Intro
The game opens from total blackness, holding in suspense before smoothly fading out to reveal the dimly lit room and giving full control to the player.

### 🔎 Evidence System
Each piece of evidence has:
- **Proximity detection** — "Press E to Interact" appears when you approach
- **Caption overlay** — Detailed narration text appears on screen
- **Progressive storytelling** — Each item reveals a new fragment of the truth

### 🕯️ Atmospheric Lighting
- Warm antique chandelier with subtle flicker creates a horror ambience
- Balanced directional light keeps the room visible while maintaining shadows
- Emissive materials on chandelier arms simulate glowing filament bulbs

### 🔐 Locked Room Progression
1. Find the **cassette tape** → play it on the **gramophone**
2. Examine **evidence items** scattered around the room
3. Find the **hidden brass key** → unlock the **desk drawers**
4. Discover the final truth → attempt to **open the door**

<br/>

## 🚀 Getting Started

### ✅ Prerequisites
- **Unity 6** (6000.x or later)
- **TextMeshPro** package (included with Unity)

### 🔧 Setup
1. Clone the repository:
```bash
   git clone https://github.com/NiladriKrSahoo-dev/Buildathon-Genesis-IVWS.git
```
2. Open the project in **Unity Hub** → **Unity 6**
3. Open `Assets/Scenes1/MAIN.unity`
4. Press **Play** ▶️

<br/>

## 👥 Team

Built for the **Genesis Buildathon** hackathon.

<br/>

## 📜 Asset Credits & Third-Party Licenses

All external assets used in this project are strictly under free-to-use, commercial, or standard Unity Asset Store licenses.

<div align="center">

| Category | Asset / Resource | Author / Source | License |
|:---|:---|:---|:---|
| 🏠 3D Environment | Vintage Living Room Game Pack | ZNS3D (Unity Asset Store) | Standard Unity Asset Store License |
| 🧍 Character Model | Civilian Girl Model | Mixamo / Unity Asset Store | Free Commercial License |
| 🪑 Props | GDT Wall Clock & Furniture Prefabs | GDT / Community Assets | Standard Asset Store License |
| 🔊 Audio & SFX | Analog Stopwatch Winding, Ambience | Freesound Community (User: freesound_community) | Creative Commons 0 (Public Domain) |
| ⌨️ UI Icon | Keyboard WASD Movement Keys | Flowicon from Noun Project | Creative Commons Attribution |
| 🔤 Typography | Comic Neue Sans ID & Liberation Sans | Google Fonts / Unity TextMeshPro | SIL Open Font License |

</div>

<br/>

## ⚖️ Hackathon Compliance
