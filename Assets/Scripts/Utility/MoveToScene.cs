using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utility
{
    public class MoveToScene: MonoBehaviour
    {
        private void Start()
        {
            MoveToNextScene();
        }

        private static void MoveToNextScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}