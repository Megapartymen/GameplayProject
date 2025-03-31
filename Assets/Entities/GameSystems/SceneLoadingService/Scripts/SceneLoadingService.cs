using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneType
{
    None = 0,
    Gameplay_Scene = 1,
    // SecretRoomScene = 2,
    // TutorialScene = 3,
    // GrannyHouseScene = 4,
    // GrannyGarageScene = 6,
    // OutsideScene = 7,
    // ButcherShopScene = 8,
    // WardrobeScene = 10,
    // HubScene = 12,
    // VoidScene = 9,
    // MultiplayerScene = 11,
    //
    // MainGrannyScene = 5,
    //
    // Art_Butchery = 100
}

public class SceneLoadingService : MonoBehaviour, IGlobalInitializable
{
    public event Action OnStartSceneLoading;
    public event Action OnEndSceneLoading;
    
    public const string Gameplay_ScenePath = "Assets/Scenes/Gameplay_Scene.unity";
    // public const string SecretRoomScenePath = "Assets/_Sources/Scenes/BuildScenes/SecretRoomScene.unity";
    // public const string TutorialScenePath = "Assets/_Sources/Scenes/BuildScenes/TutorialScene.unity";
    // public const string GrannyHouseScenePath = "Assets/_Sources/Scenes/BuildScenes/GrannyHouseScene.unity";
    // public const string MainGrannyHouseScenePath = "Assets/_Sources/Scenes/LegacyScenes/MainScene-Granny.unity";
    // public const string GrannyGarageScenePath = "Assets/_Sources/Scenes/BuildScenes/GrannyGarageScene.unity";
    // public const string OutsideScenePath = "Assets/_Sources/Scenes/BuildScenes/OutsideScene.unity";
    // public const string ButcherShopScenePath = "Assets/_Sources/Scenes/BuildScenes/ButcherShopScene.unity";
    // public const string WardrobeScenePath = "Assets/_Sources/Scenes/BuildScenes/WardrobeScene.unity";
    // public const string HubScenePath = "Assets/_Sources/Scenes/BuildScenes/HubScene.unity";
    // public const string MultiplayerScenePath = "Assets/_Sources/Scenes/TestScenes/MultiplayerScene.unity";
    
    public SceneType CurrentSceneType = SceneType.Gameplay_Scene;
    public SceneType PreviousSceneType = SceneType.Gameplay_Scene;

    private bool _isLoadingInProgress;
    private Dictionary<int, SceneType> _sceneIndexesToSceneTypes = new Dictionary<int, SceneType>();
    private Dictionary<SceneType, string> _scenesMap;

    public Dictionary<int, SceneType> SceneIndexesToSceneTypes => _sceneIndexesToSceneTypes;
    public bool IsLoadingInProgress => _isLoadingInProgress;

    public void OnInitialize()
    {
        CurrentSceneType = GetCurrentSceneType();
        SceneManager.sceneLoaded += OnSceneLoaded;

        _scenesMap = new Dictionary<SceneType, string>()
        {
            { SceneType.Gameplay_Scene, Gameplay_ScenePath},
            // { SceneType.SecretRoomScene, SecretRoomScenePath},
            // { SceneType.TutorialScene, TutorialScenePath},
            // { SceneType.GrannyHouseScene, GrannyHouseScenePath},
            // { SceneType.MainGrannyScene, MainGrannyHouseScenePath},
            // { SceneType.GrannyGarageScene , GrannyGarageScenePath},
            // { SceneType.OutsideScene , OutsideScenePath},
            // { SceneType.ButcherShopScene, ButcherShopScenePath},
            // { SceneType.WardrobeScene , WardrobeScenePath},
            // { SceneType.HubScene , HubScenePath},
            // { SceneType.MultiplayerScene, MultiplayerScenePath}
        };

        foreach (KeyValuePair<SceneType, string> pair in _scenesMap)
        {
            var index = SceneUtility.GetBuildIndexByScenePath(pair.Value);
            if(index >= 0)
                SceneIndexesToSceneTypes.Add(index, pair.Key);
        } 
    }

    public void LoadScene(SceneType sceneType)
    {
        if(TryGetSceneIndexBySceneType(sceneType, out int index))
            StartCoroutine(LoadSceneAsyncCoroutine(index));
    }

    public bool TryGetSceneIndexBySceneType(SceneType sceneType, out int index)
    {
        index = 0;
        // if (sceneType == SceneType.VoidScene)
        // {
        //     index = SceneUtility.GetBuildIndexByScenePath(SceneManager.GetActiveScene().name);
        //     return true;
        // }
        if (!_scenesMap.TryGetValue(sceneType, out string path))
            return false;

        index = SceneUtility.GetBuildIndexByScenePath(path);
        return true;
    }
    
    private SceneType GetCurrentSceneType()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"Current scene: SceneType {sceneName}");
        return (SceneType) Enum.Parse(typeof(SceneType), sceneName);
        
    }
    
    public IEnumerator LoadSceneAsyncCoroutine(int sceneIndex, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        PreviousSceneType = CurrentSceneType;
        CurrentSceneType = SceneIndexesToSceneTypes[sceneIndex];

        _isLoadingInProgress = true;
        OnStartSceneLoading?.Invoke();
        SceneManager.LoadScene(sceneIndex);
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isLoadingInProgress = false;
        OnEndSceneLoading?.Invoke();
    }

    private void OnDestroy() => 
        SceneManager.sceneLoaded -= OnSceneLoaded;
}
