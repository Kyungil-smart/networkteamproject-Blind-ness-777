using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipInitScene : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("AJYTestScene");
    }
}
