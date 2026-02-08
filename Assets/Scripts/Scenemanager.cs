using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenemanager : MonoBehaviour
{
    public GameObject tutorialScreen;
    public GameObject titleScreen;
    public GameObject levelSelect;
    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void ToLevelSelect(bool value)
    {
        if(levelSelect != null && titleScreen != null)
        {
            levelSelect.SetActive(value);
            titleScreen.SetActive(!value);
        }
    }

    public void ToTutorial(bool value)
    {
        Debug.Log("Tutorial");
        if (tutorialScreen != null)
        {
            tutorialScreen.SetActive(value);
        }
    }

    public IEnumerator LoadSceneAsync(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!operation.isDone)
        {
            Debug.Log(operation.progress); // 0.0 – 0.9 (0.9 = ready)
            yield return null;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

}
