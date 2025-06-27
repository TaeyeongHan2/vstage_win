
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AkilliMum
{
    public class LoadScene : MonoBehaviour
    {
        public void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}