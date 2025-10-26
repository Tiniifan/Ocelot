# Ocelot

Ocelot is a comprehensive map event editor designed for **Inazuma Eleven Go Light & Shadow**. Currently, it supports **IEGO (Inazuma Eleven Go) only**.

The tool allows you to read and modify various map components:
- **Map Information**: Map properties, camera settings, lighting configuration
- **Map Events**: Triggers, map transitions (map jump)
- **NPCs**: Non-playable characters positioned on the map
- **Heal Points**: Healing locations within the map

With Ocelot, you can easily visualize and edit these values through an intuitive interface.

Ocelot is built using **WPF** and it uses two main libraries:
- **ImaginationUI.dll (also known as StudioElevenGUI)**: Handles all UI generation and components
- **StudioElevenLib.dll**: Facilitates reading and understanding Level-5 file formats

<img width="1920" height="1032" alt="Ocelot Interface" src="https://github.com/user-attachments/assets/8d1a3b69-e3e3-4f79-957f-a7ade7dc6d5c" />

## Interface Layout

- **Top Bar**: File operations (Open and Save)
- **Left Panel**: TreeView displaying all map elements in a hierarchical structure
- **Center Panel**: Minimap preview showing the positions of NPCs, heal points, and triggers
- **Right Panel**: Properties panel displaying values for the selected element
- **Bottom Bar**: Application footer with theme and color customization options

## Getting Started

### Prerequisites

To work with Ocelot, you'll need:
- A Level-5 archive tool (**[Kuriimu2](https://github.com/FanTranslatorsInternational/Kuriimu2)** or **[Pingouin](https://github.com/Tiniifan/Pingouin)** - Pingouin is recommended!)
- The `ie_a.fa` archive file  (you can obtain this file by extracting the romfs data from the game. I won't explain how here, if you don't know: google it to find out how to do it.)

### Tutorial: How to Use Ocelot

#### Step 1: Extract a Map

1. Open the `ie_a.fa` file using **Kuriimu2** or **Pingouin**
2. Navigate to `data/map` within the archive
3. Extract the map folder you wish to edit

#### Step 2: Open the Map in Ocelot

1. Launch Ocelot
2. Click **Open** in the top menu
3. Select the extracted map folder
4. The map will load, displaying all its components in the TreeView

#### Step 3: Edit Map Data

1. Navigate through the TreeView on the left to find the element you want to modify
2. Click on any element (NPC, heal point, trigger, etc.)
3. View and edit its properties in the right panel
4. Use the minimap in the center to visualize positions

#### Step 4: Save Your Changes

1. Once you've finished editing, click **Save** in the top menu
2. Replace the modified files in your `ie_a.fa` archive using your archive tool
3. Your changes are now ready to be tested in-game!

## Tips

- Always backup your original files before making modifications
