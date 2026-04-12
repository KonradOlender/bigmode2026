using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance;

    private string playerId;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Init()
    {
        // Tworzenie / pobieranie lokalnego ID (jak localStorage w przeglądarce)
        if (PlayerPrefs.HasKey("player_id"))
        {
            playerId = PlayerPrefs.GetString("player_id");
        }
        else
        {
            playerId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("player_id", playerId);
            PlayerPrefs.Save();
        }
        Debug.Log(playerId);
        Login();
    }

    void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = playerId,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("✅ Zalogowano do PlayFab!");
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError("❌ PlayFab error: " + error.GenerateErrorReport());
    }
}
