using UnityEngine;

[CreateAssetMenu(fileName = "DataBase", menuName = "Scriptable Objects/DataBase")]
public class DataBase : ScriptableObject
{
    [Header("Config")]
    public bool resetOnStart;

    public bool compleated = false;
    public string rank;
    public float timeLeft;
    public string formatedTime;
    public float topSpeed;
    public int pingwinKills;

    public void OnEnable()
    {
        if (resetOnStart)
        {
            compleated = false;
            rank = "";
            timeLeft = 0;
            topSpeed = 0;
            pingwinKills = 0;
        }
    }
    public void SetLevelData(bool compleatedV, string rankV, float timeLeftV, string formatedTimeV, float topSpeedV)
    {
        compleated = compleatedV;
        rank = rankV;
        if(timeLeft < timeLeftV)
        {
            timeLeft = timeLeftV;
            formatedTime = formatedTimeV;
        }
        if(topSpeed < topSpeedV)
        {
            topSpeed = topSpeedV;
        }
    }

    public void AddPingwinKilled(int value)
    {
        pingwinKills += value;
    }
}
