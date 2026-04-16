#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class EditorHotkeys : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod]
    private static void Init()
    {
        GameObject go = new GameObject("Editor Hot Keys");
        go.AddComponent<EditorHotkeys>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if(Input.GetKeyDown(KeyCode.C))
        {
            EditorApplication.isPaused = true;
        }
        if(Input.GetKeyDown(KeyCode.Z))
        {
            Time.timeScale += 1;
        }
        if(Input.GetKeyDown(KeyCode.X))
        {
            Time.timeScale = 1;
        }
    }
}

#endif