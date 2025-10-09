SAVE SYSTEM HANDBOOK

OVERVIEW

The save system has three parts:

SaveAgentBase.cs
Defines the structure (interface) that all save agents must follow.

InventorySaveAgent.cs
Example agent: handles saving and loading inventory data.

SaveSystemManager.cs
Central manager that calls all agents and handles file saving/loading.

HOW THE SYSTEM WORKS (STEP BY STEP)

A. SaveAgentBase.cs

Abstract base class.

All systems that want to be saved must inherit it.

Requires three members:

public abstract string SectionName { get; }
public abstract object CaptureData();
public abstract void RestoreData(object data);

Any script that inherits this class must have those three functions.

B. InventorySaveAgent.cs

Connects the saving system to InventoryManager.

Defines what data will be saved:

[Serializable]
public class InventorySaveData
{
[Serializable]
public class SlotData
{
public string itemName;
public int quantity;
}

  public List<SlotData> slots = new();


}

CaptureData()

Reads all inventory slots from InventoryManager.

Creates InventorySaveData.

Stores each slot’s item name and quantity.

Returns the filled data.

RestoreData()

Clears InventoryManager.slots.

Rebuilds them using data from InventorySaveData.

Looks up items in ItemLibrary by name.

You do not modify InventoryManager.
All saving logic lives in this agent.

C. SaveSystemManager.cs

Central component that coordinates everything.

Main public functions:

SaveGame(); // Save all agents’ data to file
LoadGame(); // Load from file and restore data
ClearSave(); // Delete the save file

SaveGame():

Finds all objects in the scene that inherit from SaveAgentBase.

Calls CaptureData() on each one.

Stores each agent’s data into SaveContainer.

Converts to JSON and writes to file.

LoadGame():

Reads saved JSON file.

Recreates SaveContainer.

Tells each agent to RestoreData() using its saved section.

D. SaveContainer

Holds all collected data from agents.

Uses a list of SectionEntry (key + JSON string) for serialization.

PrepareForSerialization(): converts runtime data to JSON strings.

RestoreAfterDeserialization(): converts JSON strings back for agents.

DATA FLOW SUMMARY

CaptureData() ........... Game -> Save File
PrepareForSerialization() Object -> JSON
WriteAllText() .......... JSON -> Disk
LoadGame() .............. Disk -> JSON -> Object -> Game

FILE PATH

All saves are stored at:
Application.persistentDataPath + "/save.json"

Example (Windows):
C:\Users<User>\AppData\LocalLow<Company><Game>\save.json

ADDING A NEW SYSTEM TO SAVE

Create a new class inheriting from SaveAgentBase.

Define your own save data structure (like PlayerSaveData).

Implement:
SectionName (unique name)
CaptureData()
RestoreData()

Attach the new agent to your scene and assign references.

SaveSystemManager will automatically find it.

FAQ - COMMON FUTURE CASES

Q1. I added a new variable to save (e.g. durability). What do I do?
A. Add the field to your SaveData class.
Update CaptureData() and RestoreData().
Done. Old save files ignore missing fields and use defaults.

Q2. I renamed or deleted a variable. Will old saves break?
A. No. JsonUtility ignores missing fields.
Renamed fields are treated as new fields (defaults used).

Q3. I changed the structure (e.g. categories in inventory).
A. Update the SaveAgent to match new structure.
Old saves load but may miss new information.

Q4. Where should I put new save agents?
A. Folder suggestion:
/Scripts/SaveSystem/Agents/

Q5. How do I handle major format changes?perf
A. Use the version field in SaveContainer.
Increment version and handle older ones manually in LoadGame().

Q6. Can I have multiple save slots?
A. Yes. Change the fileName dynamically:
fileName = $"save_slot_{slotIndex}.json";
Then call SaveGame() or LoadGame() normally.

Q7. Can I encrypt or compress saves?
A. Yes. After ToJson(), compress or encrypt the text.
Reverse the process before loading.

Q8. How to test in Unity?
A. Add SaveSystemManager to the scene.
Right-click component -> Save Game or Load Game.
Or call from code:
FindObjectOfType<SaveSystemManager>().SaveGame();

Q9. What if an item in ItemLibrary was deleted?
A. The system logs a warning:
"Could not find item 'itemName'"
It skips the missing item safely.

MAINTENANCE CHECKLIST

[ ] Keep each SaveAgent handling only one system.
[ ] When adding gameplay variables, check if they need saving.
[ ] Increase version number when changing format.
[ ] Keep all save scripts in /SaveSystem/.
[ ] Test Save/Load after modifying data structures.

END OF HANDBOOK