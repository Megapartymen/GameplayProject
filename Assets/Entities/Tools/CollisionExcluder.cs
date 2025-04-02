using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using System.Linq;

public class CollisionExcluder : MonoBehaviour
{
    [TabGroup("Links"), SerializeField] private List<Collider> _collidersForIgnore;
    [TabGroup("Links"), SerializeField] private List<Collider> _myColliders;


    [Space, Header("Settings")]
    [SerializeField] private bool _isAutoSelfSeparation;
    [SerializeField, ShowIf("_isAutoSelfSeparation")] private int _parentDepth;
    
    private Transform _parent;
    private List<Collider> _dynamicIgnoreColliders = new List<Collider>();

    public void UpdateDynamicCollidersIgnore(Collider[] colliders, bool ignore)
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            if (_dynamicIgnoreColliders.Contains(colliders[i]) == false)
            {
                _dynamicIgnoreColliders.Add(colliders[i]);
            }
        }

        UpdateDynamicCollidersCollision(ignore);
    }

    private void UpdateDynamicCollidersCollision(bool state)
    {
        for (int i = 0; i < _dynamicIgnoreColliders.Count; i++)
        {
            foreach (var myCollider in _myColliders)
            {
                Physics.IgnoreCollision(_dynamicIgnoreColliders[i], myCollider, state);
            }
        }
    }

    private void Start()
    {
        GetParent();
        TryAutoSeparate();
        ExcludeColliders();
    }

    private void TryAutoSeparate()
    {
        if (_isAutoSelfSeparation)
        {
            _myColliders.AddRange(GetComponents<Collider>());
            _collidersForIgnore.AddRange(_parent.GetComponentsInChildren<Collider>());
            // var childrenColliders = GetComponentsInChildren<Collider>();
            // var parentColliders = GetComponentsInParent<Collider>();
            //
            // foreach (var collider in childrenColliders)
            // {
            //     _collidersForIgnore.Add(collider);
            // }
            //
            // foreach (var collider in parentColliders)
            // {
            //     if (!_collidersForIgnore.Contains(collider))
            //     {
            //         _collidersForIgnore.Add(collider);
            //     }
            // }

            foreach (var collider in _myColliders)
            {
                if (_collidersForIgnore.Contains(collider))
                {
                    _collidersForIgnore.Remove(collider);
                }
            }
        }
    }
    
    private void ExcludeColliders()
    {
        foreach (var collider in _collidersForIgnore)
        {
            foreach (var myCollider in _myColliders)
            {
                Physics.IgnoreCollision(collider, myCollider);
            }
        }
    }

    private void GetParent()
    {
        _parent = transform;
        if (_parentDepth == 0) return;
        
        for (int i = 0; i < _parentDepth; i++)
        {
            _parent = _parent.parent;
        }
    }

    [Button]
    private void DetectColliders(string layerName = "Decorations", float sphereRadius = -1)
    {
        if (sphereRadius<0)
        {
            var bounds = GetComponentInChildren<Renderer>().bounds;
            sphereRadius = (bounds.size.x + bounds.size.y + bounds.size.z) / 3;
        }

        var colliders = Physics.OverlapSphere(transform.position, sphereRadius, 1<<LayerMask.NameToLayer(layerName));
        _collidersForIgnore = colliders.ToList();
    }
    
    [Button]
    private void DetectCollidersBoxColliders(string layerName = "Decorations")
    {
        var bounds = new Bounds(transform.position, Vector3.zero);
        foreach (var boxCollider in GetComponentsInChildren<BoxCollider>())
        {
            bounds.Encapsulate(boxCollider.bounds);
        }
        var colliders = Physics.OverlapBox(transform.position, bounds.extents, transform.rotation, 1<<LayerMask.NameToLayer(layerName));
        _collidersForIgnore = colliders.ToList();
    }
    
}
