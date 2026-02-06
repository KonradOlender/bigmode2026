using UnityEngine;

[CreateAssetMenu(fileName = "DataBase", menuName = "Scriptable Objects/DataBase")]
public class DataBase : ScriptableObject
{
    public string rank;
    public float topSpeed;
    public int pingwinKills;

    public void OnEnable()
    {
        rank = "";
        topSpeed = 0;
        pingwinKills = 0;
    }
    public void SetLevelData(string rankV, float topSpeedV, int pingwinKillsV)
    {
        rank = rankV;
        topSpeed = topSpeedV;
        pingwinKills = pingwinKillsV;
    }
}
