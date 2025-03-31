using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

[DefaultExecutionOrder(-998)]
public class PersistentUpdateService : MonoBehaviour, IGlobalInitializable, IDisposable
{
    [SerializeField, TabGroup("Allocation settings")]
    private int maxRegularUpdateCount;

    [SerializeField, TabGroup("Allocation settings")]
    private int maxFixedUpdateCount;

    [SerializeField, TabGroup("Allocation settings")]
    private int maxLateUpdateCount;

    private UpdateCall[] _onUpdateDelegates;
    private UpdateCall[] _onFixedUpdateDelegates;
    private UpdateCall[] _onLateUpdateDelegates;

    [SerializeField, TabGroup("Debug")] private int _activeUpdatesCount;
    [SerializeField, TabGroup("Debug")] private int _activeFixedUpdatesCount;
    [SerializeField, TabGroup("Debug")] private int _activeLateUpdatesCount;
    private SceneLoadingService _sceneLoadingService;

    public Action OnEndOfFrame = () => { };

    #region InjectServices----------------------------------------------------------------------------------------

    // [Inject]
    // private void Construct(SceneLoadingService sceneLoadingService)
    // {
    //     //_sceneLoadingService = sceneLoadingService;
    //     //_sceneLoadingService.OnStartSceneLoading += Dispose;
    // }

    #endregion

    public void OnInitialize()
    {
        _onUpdateDelegates = new UpdateCall[maxRegularUpdateCount];
        _onFixedUpdateDelegates = new UpdateCall[maxFixedUpdateCount];
        _onLateUpdateDelegates = new UpdateCall[maxLateUpdateCount];
        
        RenderPipelineManager.endFrameRendering += CallEndOfFrame;
    }
    
    private void CallEndOfFrame(ScriptableRenderContext arg1, Camera[] arg2)
    {
        OnEndOfFrame();
    }


    public UpdateHandle CreateUpdateHandle(Object linkObject)
    {
        var handle = new UpdateHandle(linkObject);
        return handle;
    }

    public void RegisterUpdate(PersistentUpdateType type, Action method, UpdateHandle handle)
    {
        UpdateCall call;
        //Debug.Log($"[{nameof(PersistentUpdateService)}] Registering {type} update for object {handle.ObjectName}");
        switch (type)
        {
            case PersistentUpdateType.Regular:
                if (_activeUpdatesCount >= _onUpdateDelegates.Length)
                {
                    Debug.LogError(
                        $"[{nameof(PersistentUpdateService)}] Updates of type {nameof(PersistentUpdateType.Regular)} exceed maximum count");
                    return;
                }

                if (handle.UpdateIndex >= 0)
                    return;

                handle.UpdateIndex = _activeUpdatesCount;

                call = _onUpdateDelegates[handle.UpdateIndex];
                if (ReferenceEquals(call, null))
                {
                    call = new UpdateCall()
                    {
                        Method = method,
                        UpdateHandle = handle
                    };
                    _onUpdateDelegates[handle.UpdateIndex] = call;
                }
                else
                    call.Method = method;

                _activeUpdatesCount++;
                break;

            case PersistentUpdateType.Fixed:
                if (_activeFixedUpdatesCount >= _onFixedUpdateDelegates.Length)
                {
                    Debug.LogError(
                        $"[{nameof(PersistentUpdateService)}] Updates of type {nameof(PersistentUpdateType.Fixed)} exceed maximum count");
                    return;
                }

                if (handle.FixedUpdateIndex >= 0)
                    return;

                handle.FixedUpdateIndex = _activeFixedUpdatesCount;

                call = _onFixedUpdateDelegates[handle.FixedUpdateIndex];
                if (ReferenceEquals(call, null))
                {
                    call = new UpdateCall()
                    {
                        Method = method,
                        UpdateHandle = handle
                    };
                    _onFixedUpdateDelegates[handle.FixedUpdateIndex] = call;
                }
                else
                    call.Method = method;

                _activeFixedUpdatesCount++;
                break;

            case PersistentUpdateType.Late:
                if (_activeLateUpdatesCount >= _onLateUpdateDelegates.Length)
                {
                    Debug.LogError(
                        $"[{nameof(PersistentUpdateService)}] Updates of type {nameof(PersistentUpdateType.Late)} exceed maximum count");
                    return;
                }

                if (handle.LateUpdateIndex >= 0)
                    return;

                handle.LateUpdateIndex = _activeLateUpdatesCount;

                call = _onLateUpdateDelegates[handle.LateUpdateIndex];
                if (ReferenceEquals(call, null))
                {
                    call = new UpdateCall()
                    {
                        Method = method,
                        UpdateHandle = handle
                    };
                    _onLateUpdateDelegates[handle.LateUpdateIndex] = call;
                }
                else
                    call.Method = method;

                _activeLateUpdatesCount++;
                break;
        }
    }

    public void RemoveUpdate(PersistentUpdateType type, UpdateHandle handle)
    {
        UpdateHandle lastElementHandle;
        switch (type)
        {
            case PersistentUpdateType.Regular:
                if (handle.UpdateIndex < 0 || _activeUpdatesCount == 0)
                    return;
                
                lastElementHandle = _onUpdateDelegates[_activeUpdatesCount - 1].UpdateHandle;
                
                if (handle.UpdateIndex != _activeUpdatesCount - 1)
                {
                    _onUpdateDelegates[handle.UpdateIndex] = _onUpdateDelegates[_activeUpdatesCount - 1];
                    lastElementHandle.UpdateIndex = handle.UpdateIndex;
                }
                
                _onUpdateDelegates[_activeUpdatesCount - 1] = null;
                _activeUpdatesCount--;

                handle.UpdateIndex = -1;
                break;

            case PersistentUpdateType.Fixed:
                if (handle.FixedUpdateIndex < 0 || _activeFixedUpdatesCount == 0)
                    return;

                lastElementHandle = _onFixedUpdateDelegates[_activeFixedUpdatesCount - 1].UpdateHandle;

                if (handle.FixedUpdateIndex != _activeFixedUpdatesCount - 1)
                {
                    _onFixedUpdateDelegates[handle.FixedUpdateIndex] =
                        _onFixedUpdateDelegates[_activeFixedUpdatesCount - 1];
                    lastElementHandle.FixedUpdateIndex = handle.FixedUpdateIndex;
                }

                _onFixedUpdateDelegates[_activeFixedUpdatesCount - 1] = null;
                _activeFixedUpdatesCount--;

                handle.FixedUpdateIndex = -1;
                break;

            case PersistentUpdateType.Late:
                if (handle.LateUpdateIndex < 0 || _activeLateUpdatesCount == 0)
                    return;

                lastElementHandle = _onLateUpdateDelegates[_activeLateUpdatesCount - 1].UpdateHandle;

                if (handle.LateUpdateIndex != _activeLateUpdatesCount - 1)
                {
                    _onLateUpdateDelegates[handle.LateUpdateIndex] =
                        _onLateUpdateDelegates[_activeLateUpdatesCount - 1];
                    lastElementHandle.LateUpdateIndex = handle.LateUpdateIndex;
                }

                _onLateUpdateDelegates[_activeLateUpdatesCount - 1] = null;
                _activeLateUpdatesCount--;

                handle.LateUpdateIndex = -1;
                break;
        }
    }

    private void Update()
    {
        for (int i = 0; i < _activeUpdatesCount; i++)
        {
            var call = _onUpdateDelegates[i];
            call.UpdateHandle.Marker.Begin(call.UpdateHandle.LinkedObject);
            call.Method.Invoke();
            call.UpdateHandle.Marker.End();
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < _activeFixedUpdatesCount; i++)
        {
            var call = _onFixedUpdateDelegates[i];
            call.UpdateHandle.Marker.Begin(call.UpdateHandle.LinkedObject);
            call.Method.Invoke();
            call.UpdateHandle.Marker.End();
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < _activeLateUpdatesCount; i++)
        {
            var call = _onLateUpdateDelegates[i];
            call.UpdateHandle.Marker.Begin(call.UpdateHandle.LinkedObject);
            call.Method.Invoke();
            call.UpdateHandle.Marker.End();
        }
    }
    
    public void Dispose()
    {
        Array.Clear(_onUpdateDelegates, 0, _activeUpdatesCount);
        Array.Clear(_onFixedUpdateDelegates, 0, _activeFixedUpdatesCount);
        Array.Clear(_onLateUpdateDelegates, 0, _activeLateUpdatesCount);

        _activeUpdatesCount = 0;
        _activeFixedUpdatesCount = 0;
        _activeLateUpdatesCount = 0;

        OnEndOfFrame = () => { };
    }

    private void OnDestroy()
    {
        //_sceneLoadingService.OnStartSceneLoading -= Dispose;
    }
}