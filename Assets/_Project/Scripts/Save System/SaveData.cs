using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class SaveData
{
    public string version = "v0.38";
    public long timestamp;
    public bool hasStartedGame = false;

    public SerializableVector3 playerPosition;
    public int currentHealth;
    public int maxHealth;

    public float torchCurrentTime;
    public float torchMaxTime;

    public int currentXP;
    public int currentEcho;
    public int currentSkillPoints;
    public int currentLevel;

    public Dictionary<string, int> skillProgress = new Dictionary<string, int>();
    public List<string> activeCheckpoints = new List<string>();
    public Dictionary<string, SerializableVector3> checkpointPositions = new Dictionary<string, SerializableVector3>();
    public List<string> killedEnemy = new List<string>();
    public List<string> activatedObjects = new List<string>();
    public List<string> discoveredZones = new List<string>();
    public string currentCheckpointID;

    public SaveData()
    {
        timestamp = DateTime.Now.ToBinary();
    }
}
