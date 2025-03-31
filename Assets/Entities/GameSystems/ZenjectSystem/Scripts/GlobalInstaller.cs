using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class GlobalInstaller : MonoInstaller
{
    // public GameObject OculusPlatformServicePrefab;
    public GameObject SceneLoadingServicePrefab;
    // public GameObject NetworkDatabaseServicePrefab;
    // public GameObject SaveLoadProgressServicePrefab;
    // public GameObject RewardsCounterServicePrefab;
    // public GameObject EnvironmentEventCollectorPrefab;
    // public GameObject DistanceCheckSystemPrefab;
    // public GameObject QuestSystemPrefab;
    // public GameObject AudioFMODSystemPrefab;
    // public GameObject TransactionRepoServicePrefab;
    public GameObject VRInputSystemPrefab;
    // public GameObject TransitionServicePrefab;
    // public GameObject PauseServicePrefab;
    public GameObject HapticServicePrefab;
    public GameObject SpeedometerSystemPrefab;
    // public GameObject LiquidSystemPrefab;
    public GameObject PersistentUpdaterPrefab;
    // public GameObject PhysicsRunnerPrefab;
    // public GameObject CoroutinesRunnerPrefab;
    // public GameObject DiagnosticPrefab;
    // public GameObject GlobalRenderingManagerPrefab;
    // public GameObject SkinnedBakerPrefab;
    // public GameObject NetworkServicePrefab;
    
    public override void InstallBindings()
    {
        // OculusPlatformService oculusPlatformService = Container.InstantiatePrefabForComponent<OculusPlatformService>(OculusPlatformServicePrefab);
        // Container.BindInterfacesAndSelfTo<OculusPlatformService>().FromInstance(oculusPlatformService).AsSingle().NonLazy();
        
        SceneLoadingService sceneLoadingService = Container.InstantiatePrefabForComponent<SceneLoadingService>(SceneLoadingServicePrefab);
        Container.BindInterfacesAndSelfTo<SceneLoadingService>().FromInstance(sceneLoadingService).AsSingle().NonLazy();
        
        // NetworkDatabaseService networkDatabaseService = Container.InstantiatePrefabForComponent<NetworkDatabaseService>(NetworkDatabaseServicePrefab);
        // Container.BindInterfacesAndSelfTo<NetworkDatabaseService>().FromInstance(networkDatabaseService).AsSingle().NonLazy();
        
        // SaveLoadProgressService saveLoadProgressService = Container.InstantiatePrefabForComponent<SaveLoadProgressService>(SaveLoadProgressServicePrefab);
        // Container.BindInterfacesAndSelfTo<SaveLoadProgressService>().FromInstance(saveLoadProgressService).AsSingle().NonLazy();
        
        // RewardsCounterService rewardsCounterService = Container.InstantiatePrefabForComponent<RewardsCounterService>(RewardsCounterServicePrefab);
        // Container.BindInterfacesAndSelfTo<RewardsCounterService>().FromInstance(rewardsCounterService).AsSingle().NonLazy();
        
        // EnvironmentEventCollector environmentEventCollector = Container.InstantiatePrefabForComponent<EnvironmentEventCollector>(EnvironmentEventCollectorPrefab);
        // Container.BindInterfacesAndSelfTo<EnvironmentEventCollector>().FromInstance(environmentEventCollector).AsSingle().NonLazy();
        
        // DistanceCheckSystem distanceCheckSystem = Container.InstantiatePrefabForComponent<DistanceCheckSystem>(DistanceCheckSystemPrefab);
        // Container.BindInterfacesAndSelfTo<DistanceCheckSystem>().FromInstance(distanceCheckSystem).AsSingle().NonLazy();

        // QuestSystem questSystem = Container.InstantiatePrefabForComponent<QuestSystem>(QuestSystemPrefab);
        // Container.BindInterfacesAndSelfTo<QuestSystem>().FromInstance(questSystem).AsSingle().NonLazy();

        // AudioFMODSystem audioFMODSystem = Container.InstantiatePrefabForComponent<AudioFMODSystem>(AudioFMODSystemPrefab);
        // Container.BindInterfacesAndSelfTo<AudioFMODSystem>().FromInstance(audioFMODSystem).AsSingle().NonLazy();

        // TransactionRepoService transactionRepoService = Container.InstantiatePrefabForComponent<TransactionRepoService>(TransactionRepoServicePrefab);
        // Container.BindInterfacesAndSelfTo<TransactionRepoService>().FromInstance(transactionRepoService).AsSingle().NonLazy();

        VRInputSystem vrInputSystem = Container.InstantiatePrefabForComponent<VRInputSystem>(VRInputSystemPrefab);
        Container.BindInterfacesAndSelfTo<VRInputSystem>().FromInstance(vrInputSystem).AsSingle().NonLazy();
        
        HapticService hapticService = Container.InstantiatePrefabForComponent<HapticService>(HapticServicePrefab);
        Container.BindInterfacesAndSelfTo<HapticService>().FromInstance(hapticService).AsSingle().NonLazy();
        
        SpeedometerSystem speedometerSystem = Container.InstantiatePrefabForComponent<SpeedometerSystem>(SpeedometerSystemPrefab);
        Container.BindInterfacesAndSelfTo<SpeedometerSystem>().FromInstance(speedometerSystem).AsSingle().NonLazy();
        
        // LiquidSystem liquidSystem = Container.InstantiatePrefabForComponent<LiquidSystem>(LiquidSystemPrefab);
        // Container.BindInterfacesAndSelfTo<LiquidSystem>().FromInstance(liquidSystem).AsSingle().NonLazy();
        
        PersistentUpdateService persistentUpdateService = Container.InstantiatePrefabForComponent<PersistentUpdateService>(PersistentUpdaterPrefab);
        Container.BindInterfacesAndSelfTo<PersistentUpdateService>().FromInstance(persistentUpdateService).AsSingle().NonLazy();
        
        // NetworkService networkService = Container.InstantiatePrefabForComponent<NetworkService>(NetworkServicePrefab);
        // Container.BindInterfacesAndSelfTo<NetworkService>().FromInstance(networkService).AsSingle().NonLazy();
        
        // TransitionService transitionService = Container.InstantiatePrefabForComponent<TransitionService>(TransitionServicePrefab);
        // ontainer.BindInterfacesAndSelfTo<TransitionService>().FromInstance(transitionService).AsSingle().NonLazy();
        
        // PauseService pauseService = Container.InstantiatePrefabForComponent<PauseService>(PauseServicePrefab);
        // Container.BindInterfacesAndSelfTo<PauseService>().FromInstance(pauseService).AsSingle().NonLazy();
        
        // PhysicsRunner physicsRunner = Container.InstantiatePrefabForComponent<PhysicsRunner>(PhysicsRunnerPrefab);
        // Container.BindInterfacesAndSelfTo<PhysicsRunner>().FromInstance(physicsRunner).AsSingle().NonLazy();

        // CoroutinesRunner coroutinesRunner = Container.InstantiatePrefabForComponent<CoroutinesRunner>(CoroutinesRunnerPrefab);
        // Container.BindInterfacesAndSelfTo<CoroutinesRunner>().FromInstance(coroutinesRunner).AsSingle().NonLazy();

        // Diagnostic diagnostic = Container.InstantiatePrefabForComponent<Diagnostic>(DiagnosticPrefab);
        // Container.BindInterfacesAndSelfTo<Diagnostic>().FromInstance(diagnostic).AsSingle().NonLazy();
        
        // GlobalRenderingManager globalRenderingManager = Container.InstantiatePrefabForComponent<GlobalRenderingManager>(GlobalRenderingManagerPrefab);
        // Container.BindInterfacesAndSelfTo<GlobalRenderingManager>().FromInstance(globalRenderingManager).AsSingle().NonLazy();
        
        // SkinnedBaker skinnedBaker = Container.InstantiatePrefabForComponent<SkinnedBaker>(SkinnedBakerPrefab);
        // Container.BindInterfacesAndSelfTo<SkinnedBaker>().FromInstance(skinnedBaker).AsSingle().NonLazy();

        /*var initializables = Container.ResolveAll<IGlobalInitializable>();
        foreach (var globalInitializable in initializables)
        {
            Debug.Log($"[{nameof(GlobalInstaller)}] Initializing {globalInitializable.GetType().Name}");
            globalInitializable.OnInitialize();
        }*/
        
        // oculusPlatformService.OnInitialize();
        sceneLoadingService.OnInitialize();
        // networkDatabaseService.OnInitialize();
        // networkService.OnInitialize();
        
        //WTF
        // saveLoadProgressService.questSystem = questSystem;
        // saveLoadProgressService.OnInitialize();
        
        // rewardsCounterService.OnInitialize();
        // environmentEventCollector.OnInitialize();
        // distanceCheckSystem.OnInitialize();
        
        //transactionRepoService must be initialized before QuestSystem
        // transactionRepoService.OnInitialize();
        // questSystem.OnInitialize();
        // audioFMODSystem.OnInitialize();
        
        vrInputSystem.OnInitialize();
        // transitionService.OnInitialize();
        // pauseService.OnInitialize();
        hapticService.OnInitialize();
        speedometerSystem.OnInitialize();
        // liquidSystem.OnInitialize();
        persistentUpdateService.OnInitialize();
        // physicsRunner.OnInitialize();
        //coroutinesRunner.OnInitialize();
        //diagnostic.OnInitialize();
        // globalRenderingManager.OnInitialize();
        // skinnedBaker.OnInitialize();

    }
}
