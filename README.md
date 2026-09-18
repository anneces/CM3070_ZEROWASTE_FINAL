# CM3070_ZEROWASTE_FINAL

# Zero-Waste VR Simulation

A Virtual Reality simulation built in Unity designed to teach and evaluate household zero-waste habits, food storage preservation, and culinary efficiency.

---

## 🚀 Quick Start: Playing via APK (Android / Meta Quest)

You do **not** need to build the project from the source code to evaluate the application. A pre-compiled, fully functional executable is provided as an **APK file**.

### **Prerequisites**
* Meta Quest (2, 3, or Pro) or compatible Android VR Headset.
* Developer Mode enabled on your headset (for sideloading).
* [SideQuest](https://sidequestvr.com/) or Android Debug Bridge (ADB) installed on your PC.

### **Installation Steps**
1. **Download the APK**: Download `ZeroWasteVR.apk` from the submission root folder (or the Releases section of this repository).
2. **Connect Headset**: Connect your VR headset to your computer via a USB-C cable.
3. **Sideload via SideQuest**:
   * Open **SideQuest**.
   * Click on the **"Install APK file from folder on computer"** icon (top toolbar).
   * Select `ZeroWasteVR.apk`.
   * Wait for the "Success" notification at the bottom of the SideQuest window.
4. **Launch the Application**:
   * Put on your VR headset.
   * Navigate to **App Library** $\rightarrow$ Dropdown menu (top right) $\rightarrow$ Select **Unknown Sources**.
   * Click on **ZeroWasteVR** to launch the simulation.

---

## 🎮 Controls & Gameplay Overview

* **Locomotion**: Smooth movement or Teleportation via thumbstick.
* **Interaction**: Use the **Grip Button** to pick up ingredients, open doors (Fridge/Freezer), and interact with appliances. Use the **Trigger Button** to select options on UI Canvases (Shopping Tablet, Recipe Book, Chalkboard).
* **Objective**: Complete cooking tasks across a 5-day cycle while minimizing food spoilage ($CO_2$ emissions and financial cost) through proper cold storage management.

---

## 📁 Repository Structure

```text
├── Assets/
│   ├── Audio/               # Centralized SFX and BGM assets
│   ├── DishItems/           # Cooked dish prefabs and Recipe ScriptableObjects
│   ├── FoodItems/           # Raw ingredient prefabs
│   ├── Shaders/             # Custom food decay shaders and materials
│   └── ZW SCRIPTS/          # Domain-driven C# codebase
│       ├── CookingManagement/
│       ├── DayPhaseTransitionManagement/
│       ├── FoodItemMangement/
│       ├── GameSummaryManagement/
│       ├── InstructionManager/
│       ├── PhaseInteractionManagement/
│       └── StorageManagement/
└── README.md