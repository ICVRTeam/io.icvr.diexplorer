using TestEnvironment.Interfaces;
using TestEnvironment.Services;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace TestEnvironment.Views
{
    public class SwitchSceneButton : MonoBehaviour
    {
        private enum SwitchMode
        {
            Next,
            Prev
        }

        [SerializeField] private Button _button; 
        [SerializeField] private SwitchMode _switchMode;

        private DebugLogService _logService;
        private LoadSceneService _loadSceneService;
        private IInformant _informant;

        [Inject]
        private void Construct(DebugLogService logService, LoadSceneService loadSceneService, IInformant informant)
        {
            _logService = logService;
            _loadSceneService = loadSceneService;
            _informant = informant;
        }
    
        private void Awake()
        {
            if (_button != null && SceneManager.sceneCountInBuildSettings > 1)
            {
                _button.OnClickAsObservable().Subscribe(_ =>
                {
                    var nextSceneIndex = 0;
                    var activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
                    var countScene = SceneManager.sceneCountInBuildSettings;
                
                    switch (_switchMode)
                    {
                        case SwitchMode.Next:
                            nextSceneIndex = activeSceneIndex + 1;
                            if (nextSceneIndex == countScene)
                                nextSceneIndex = 0;
                            break;
                        case SwitchMode.Prev:
                            nextSceneIndex = activeSceneIndex - 1;
                            if (nextSceneIndex < 0)
                                nextSceneIndex = countScene - 1;
                            break;
                    }
                
                    _loadSceneService.LoadSceneByIndex(nextSceneIndex);
                    _logService.ShowLog($"[{nameof(SwitchSceneButton)}] Switch to scene by index: {nextSceneIndex}!");
                }).AddTo(gameObject);
            }
            else
            {
                Debug.LogWarning($"[{nameof(SwitchSceneButton)}] Add scenes to the build settings or no reference to button!");
            }
        }

        private void Start()
        {
            _informant.ShowInfoScene();
        }
    }
}
