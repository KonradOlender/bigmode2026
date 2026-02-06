using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenemanager : MonoBehaviour
{
    public GameObject loadingTutorialScreen;
    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
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

}
