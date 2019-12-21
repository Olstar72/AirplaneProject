﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landing : MonoBehaviour, IState
{
    private Transform _target;
    private PlaneController _controller;
    private PlanePhysics _physics;

    [SerializeField] private LandingStrip pointB;
    [SerializeField] private int _subState = 0;
    [SerializeField] private bool _enabled = false;

    [Header("Speeds")]
    [SerializeField] private float yawnSpeed;
    [SerializeField] private float rollSpeed = 1;
    [SerializeField] private float pitchSpeed = 0.01f;

    [Header("Pitch Down")]
    [SerializeField] private float tipDownTriggerDistance = 10;
    [SerializeField] private float tipDownSpeed = 5;
    [SerializeField] private float tipDownMax = 5;

    [Space]
    [SerializeField] private float targetPitch = 0;


    public void Setup(Transform pTarget, PlaneController pController, PlanePhysics pPhysics)
    {
        _target = pTarget;
        _controller = pController;
        _physics = pPhysics;
    }

    public void Enter()
    {
        _controller.thrust = false;
        _enabled = true;
    }

    public void Exit()
    {
        _enabled = false;
    }

    public bool CanExit()
    {
        return false;
    }

    public void Tick()
    {
        float relY = transform.GetChild(0).position.y - pointB.transform.position.y;
        Debug.Log("Landing: " + relY);

        if (relY < tipDownTriggerDistance)
        {
            targetPitch += Time.deltaTime * tipDownSpeed;
            targetPitch = Mathf.Min(targetPitch, tipDownMax);
        }

        LevelOut();
        MakeParallel();
    }

    private void MakeParallel()
    {
        float relRotY = transform.GetChild(0).eulerAngles.y - pointB.transform.eulerAngles.y + 180;

        //Rotate to be parallel to the landing strip
        if (relRotY < 0)
        {
            _controller.yawn = -Vector3.Dot(transform.GetChild(0).forward, pointB.transform.forward) * yawnSpeed;
        }
        else if (relRotY != 0)
        {
            _controller.yawn = Vector3.Dot(transform.GetChild(0).forward, pointB.transform.forward) * yawnSpeed;
        }
    }

    private void LevelOut()
    {
        _controller.pitch = (_physics._verticalVelocity.y + targetPitch) * pitchSpeed;
        _controller.roll = -_physics._angluarVelocity.y * rollSpeed;
    }
}