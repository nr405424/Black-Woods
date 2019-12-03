﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Strix : Player
{

    [SerializeField]
    private float _flairRadius = 5f;

    private int _layerMaskFlair = 1 << 9;

    [HideInInspector]
    public bool isCoop;

    private GameObject nearestObj;
    private GameObject _holeToDig;

    [SerializeField]
    private Collider _coopCollider;

    private void Awake() {
        Init();

        //_controls.Player.StrixMovement.performed += ctx => _move = new Vector2(ctx.ReadValue<float>(), 0);
        //_controls.Player.StrixMovement.canceled += ctx => _move = Vector2.zero;

        //_controls.Player.StrixJump.performed += ctx => Jump();
    }

    private void Update() {
        PlayerUpdate();

        if (isCoop) {
            _coopCollider.enabled = true;
        }
        else {
            _coopCollider.enabled = false;
        }
    }

    private void OnStrixMovement(InputValue value) {
        _move = new Vector2(value.Get<Vector2>().x, 0);
        animator.SetBool("isRunning", true);
        if (value.Get<Vector2>().x == 0) {
            animator.SetBool("isRunning", false);
        }
    }

    private void OnStrixJump() {
        if (_isGrounded) {
            //make the jump
            _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            _rb.velocity += Vector3.up * _jumpStrength;
        }
    }

    private void OnStrixFlair() {
        Collider[] nearObjects = Physics.OverlapSphere(transform.position, _flairRadius, _layerMaskFlair);

        if (nearObjects.Length == 1) {
            nearestObj = nearObjects[0].gameObject;
        }
        else if (nearObjects.Length > 1) {  //trouver l'objet le plus proche qui n'est pas deja detecté
            float minDistance = 100f;
            foreach(Collider obj in nearObjects) {
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                if (distance <= minDistance && !obj.GetComponent<Usable>().isDetected) {
                    minDistance = distance;
                    nearestObj = obj.gameObject;
                }
            }
        }

        if (nearestObj != null) {
            nearestObj.GetComponent<Usable>().OnDetected();
        }

        StartCoroutine(ActivateFlairAnimation());
    }

    IEnumerator ActivateFlairAnimation() {
        animator.SetBool("isFlairing", true);
        yield return new WaitForSeconds(2.25f);
        animator.SetBool("isFlairing", false);
    }

    IEnumerator ActivateDigAnimation() {
        animator.SetBool("isDigging", true);
        yield return new WaitForSeconds(2.15f);
        animator.SetBool("isDigging", false);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position, _flairRadius);
    }

    private void OnStrixCreuse() {
        StartCoroutine(ActivateDigAnimation());

        if (_isNextToHole && _holeToDig != null) {
            _holeToDig.GetComponent<EnablePathObject>().EnablePath();
        }
    }

    private void OnStrixCoop(InputValue value) {
        isCoop = value.Get<float>() > 0;
        Debug.Log("(Strix) : _isCoop = " + isCoop);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "creuse_object") {
            _isNextToHole = true;
            _holeToDig = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "creuse_object") {
            _isNextToHole = false;
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.tag == "creuse_object") {
            _isNextToHole = true;
        }
    }
}
