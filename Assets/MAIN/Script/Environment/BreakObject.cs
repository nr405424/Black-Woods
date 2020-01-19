﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakObject : MonoBehaviour
{
    [HideInInspector]
    public bool _isBroken;

    private Rigidbody _rb;
    private BoxCollider _coll;
    [SerializeField]
    private GameObject _meshGO;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<BoxCollider>();
    }

    public void OnBecHit() {
        if (!_isBroken) {
            Debug.Log("hit by Dots");
            _isBroken = true;

            _rb.useGravity = true;
            //_coll.isTrigger = false;

            MeshRenderer[] meshs = _meshGO.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mesh in meshs) {
                mesh.enabled = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "ground") {
            Debug.Log("hit ground");
            _rb.velocity = Vector3.zero;
            _rb.useGravity = false;
        }
    }
}
