using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestEnvironmen.Views
{
    public class SceneNameComponent : MonoBehaviour
    {
        [SerializeField] private Text _text;

        private void Start()
        {
            if (_text != null)
            {
                _text.text = SceneManager.GetActiveScene().name;
            }
        }
    }
}
