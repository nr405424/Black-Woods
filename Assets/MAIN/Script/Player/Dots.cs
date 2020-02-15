﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class Dots : Player {
    [SerializeField]
    private float _speedOnTree = 1f;
    private float _speedOnWeb = 0.85f;
    private float _baseSpeed;
    [SerializeField]
    private float _chanteRadius = 5f;

    private int _layerMaskFlair = 1 << 9;
    private GameObject nearestObj;

    private bool _isUsingBec;
    private bool _isNearHeadStrix;
    [HideInInspector]
    public bool isOnStrixHead;

    private GameObject _objectToHit;

    [SerializeField]
    private Transform _strix;
    [SerializeField]
    private Transform _strixHead;

    [SerializeField]
    private GameObject _mainCamera;

    //[HideInInspector]
    public bool becIsUnlock, coopIsUnlock, jumpOnStrixIsUnlock, chanteIsUnlock;

    [SerializeField]
    private float _dotsZPosition = 7.5f;

    public bool IsNearHeadStrix {
        get { return _isNearHeadStrix; }
        set { _isNearHeadStrix = value; }
    }

    private void Awake() {
        Init();
        _baseSpeed = _playerSpeed;
        //_controls.Player.DotsMovement.performed += ctx => _move = new Vector2(ctx.ReadValue<float>(), 0);
        //_controls.Player.DotsMovement.canceled += ctx => _move = Vector2.zero;

        //_controls.Player.DotsJump.performed += ctx => Jump();
    }

    private void Update() {
        PlayerUpdate();

        if (_onTree && _isUsingBec) {    //si dots touche l'arbre et il appuie sur la touche "bec"
            _wallClimb = true;
            _playerSpeed = _speedOnTree;
            _rb.useGravity = false;
        } else if (isOnStrixHead) {      //si dots est sur Strix
            _rb.useGravity = false;
        } else if (_isOnSpiderWeb) {
            _playerSpeed = _speedOnWeb;
        } else {
            _wallClimb = false;
            _playerSpeed = _baseSpeed;
            _rb.useGravity = true;
        }
    }

    private void FixedUpdate() {
        PlayerFixedUpdate();
    }

    private void OnDotsMovement(InputValue value) {
        Move(value);
    }

    private void OnDotsJump() {
        if (_isGrounded) Jump();
        else if (isOnStrixHead) {
            if (jumpOnStrixIsUnlock) {
                GetDownFromStrix();
                Jump();
            }
        }
    }

    private void OnDotsBec(InputValue value) {
        if (becIsUnlock && (_isGrounded || isOnStrixHead)) {            DisableHorizontalMovement();
            _isUsingBec = value.Get<float>() > 0;
            if (_objectToHit != null) {
                if (_objectToHit.tag == "branchDoMove") {
                    _objectToHit.GetComponent<DoMoveOnBec>().OnBecHit();
                }
                else if (_objectToHit.tag == "brambleDoMove") {
                    _objectToHit.GetComponent<SeparateBrambles>().OnBecHit();
                }
                else {
                    _objectToHit.GetComponent<BreakObject>().OnBecHit();
                }
            }
            animator.SetTrigger("isPecking");
        }
    }

    private void OnDotsCoop() {
        if (coopIsUnlock) {
            if (_isNearHeadStrix && _isGrounded /*&& _strix.gameObject.GetComponent<Strix>().isCoop*/) {
                MoveToStrixHead();
            } else if (isOnStrixHead && _strix.gameObject.GetComponent<Strix>().isCoop) {                Debug.Log("TRYING");
                GetDownFromStrix();
            }
        }
    }

    private void OnDotsChante() {
        if (!_stopMoving) {
            if (chanteIsUnlock && _isGrounded) {
                DisableHorizontalMovement();

                Collider[] nearObjects = Physics.OverlapSphere(transform.position, _chanteRadius, _layerMaskFlair);

                if (nearObjects.Length == 1) {
                    nearestObj = nearObjects[0].gameObject;
                } else if (nearObjects.Length > 1) {  //trouver l'objet le plus proche qui n'est pas deja detecté
                    float minDistance = 100f;
                    foreach (Collider obj in nearObjects) {
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

                animator.SetTrigger("isSinging");
            }
        }
    }

    private void MoveToStrixHead() {
        //animator.SetBool("isJumping", true);
        animator.SetTrigger("triggerIsJumping");
        isOnStrixHead = true;

        CameraControl cameraControl = _mainCamera.GetComponent<CameraControl>();    //to not move cam when coop and anchor is strix
        cameraControl.isMoving = false;
        cameraControl.offSetStrixLastUpdatePos = new Vector3(
            cameraControl.UpdatePosition().x - _strix.position.x,
            cameraControl.UpdatePosition().y - _strix.position.y,
            0f);
        cameraControl.offSetCoop = _mainCamera.transform.position - new Vector3(cameraControl.UpdatePosition().x, cameraControl.UpdatePosition().y, _strix.position.z);

        transform.SetParent(_strixHead);

        transform.DOJump(_strixHead.position, 0.5f, 1, 0.5f).OnComplete(() => EndMoveToStrixHead());

        _rb.constraints = RigidbodyConstraints.FreezeAll;
        _col.isTrigger = true;
    }

    private void EndMoveToStrixHead() {
        //animator.SetBool("isJumping", false);
        transform.position = _strixHead.position;
        transform.rotation = _strixHead.rotation;
    }

    private void GetDownFromStrix() {
        Debug.Log("DOWN");
        isOnStrixHead = false;

        CameraControl cameraControl = _mainCamera.GetComponent<CameraControl>();
        cameraControl.isMoving = true;

        transform.parent = null;
        transform.position = new Vector3(transform.position.x, transform.position.y, _dotsZPosition);
        _rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        _col.isTrigger = false;
    }

    public void SetBoolAnim(string animatorParameter, bool boolJump) {
        animator.SetBool(animatorParameter, boolJump);
    }

    public void SetTriggerAnim(string animatorParameter) {
        animator.SetTrigger(animatorParameter);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "break_object" || other.tag == "caveBranch" || other.tag == "branchDoMove" || other.tag == "brambleDoMove") {
            _objectToHit = other.gameObject;
        }
        if (other.tag == "head_strix") {
            _isNearHeadStrix = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "break_object" || other.tag == "caveBranch" || other.tag == "branchDoMove" || other.tag == "brambleDoMove") {
            _objectToHit = null;
        }
        if (other.tag == "head_strix") {
            _isNearHeadStrix = false;
        }
    }

    private void OnTriggerStay(Collider other) {
    }

    private void OnCollisionEnter(Collision collision) {
        CollisionEnter(collision);
    }

    private void OnCollisionExit(Collision collision) {
        CollisionExit(collision);
    }

    private void OnCollisionStay(Collision collision) {
        CollisionStay(collision);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position, _chanteRadius);
        DrawGizmos();
    }
}
