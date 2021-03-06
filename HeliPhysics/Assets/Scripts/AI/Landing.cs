﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landing : MonoBehaviour, IState
{
    private PlaneController _controller;
    private PlanePhysics _physics;

    public LandingStrip pointB;
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

    public void Setup(PlaneController pController, PlanePhysics pPhysics)
    {
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
        TipDown();
        MakeParallel();
        LevelOut();

        if (_enabled == true && _physics._velocity.magnitude <= 5f)
        {
            transform.GetComponentInChildren<PlaneCrash>().Crashed();
            _enabled = false;
        }
    }

    private void TipDown()
    {
        float relY = transform.GetChild(0).position.y - pointB.transform.position.y;

        if (relY < tipDownTriggerDistance)
        {
            targetPitch += Time.deltaTime * tipDownSpeed;
            targetPitch = Mathf.Min(targetPitch, tipDownMax);

            //if (targetPitch == tipDownMax && _physics._horizontalVelocity.magnitude > 0.5f)
            //    _physics._horizontalVelocity -= _physics._horizontalVelocity.normalized * 0.01f;
        }
    }

    private void MakeParallel()
    {
        float relRotY = Vector3.SignedAngle(transform.GetChild(0).forward, -pointB.transform.forward, Vector3.up);

        //Rotate to be parallel to the landing strip
        if (relRotY != 0)
        {
            _controller.yawn = Mathf.Clamp(relRotY / 15, -1, 1) * yawnSpeed;
        }
    }

    private void LevelOut()
    {
        _controller.pitch = (_physics._verticalVelocity.y + targetPitch) * pitchSpeed;
        _controller.roll = -_physics._angluarVelocity.y * rollSpeed;
    }
}
