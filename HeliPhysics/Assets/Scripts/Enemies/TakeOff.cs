﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeOff : MonoBehaviour, IState
{
    private Transform _target;
    private PlaneController _controller;
    private PlanePhysics _physics;

    [SerializeField] private LandingStrip pointA;

    [SerializeField] private int _subState = 0;
    [SerializeField] private bool _enabled = false;
    private float leaveStateTime = 0;

    [Header("Pitch")]
    [SerializeField] private float pullUpSpeed;
    [SerializeField] private float pullUpLength = 1.5f;
    [SerializeField] [Range(0, -1)] private float pitchUpSpeed = -0.5f;
    [SerializeField] [Range(0, 1)] private float pullUpTargetPitch = 0.2f;

    [Header("Yawn")]
    [SerializeField] private float parallelRotMult = 1;
    [SerializeField] private float returnToStripRotMult = 1;
    [SerializeField] private float returnToStripAngle = 20;
    [SerializeField] private float returnToStripBuffer = 1f;
    [SerializeField] private float returnToStripMaxThrust = 3;


    public void Setup(Transform pTarget, PlaneController pController, PlanePhysics pPhysics)
    {
        _target = pTarget;
        _controller = pController;
        _physics = pPhysics;
    }

    public void Enter()
    {
        _subState = 0;
        _controller.thrust = true;
        _enabled = true;
    }

    public void Exit()
    {
        _enabled = false;
    }

    public bool CanExit()
    {
        return _subState == 3;
    }

    public void Tick()
    {
        switch (_subState)
        {
            case 0:
                //Acceleration
                StayOnLandingStrip();

                if (_physics._horizontalVelocity.magnitude >= pullUpSpeed)
                {
                    _subState = 1;
                    _controller.yawn = 0;
                }

                break;

            case 1:
                //Get off the ground
                _controller.pitch = pitchUpSpeed;

                if (transform.GetChild(0).forward.y >= pullUpTargetPitch)
                {
                    leaveStateTime = Time.time + pullUpLength;
                    _controller.pitch = 0f;
                    _subState = 2;
                }

                break;

            case 2:
                if (Time.time >= leaveStateTime)
                    _subState = 3;

                break;
        }
    }

    private void StayOnLandingStrip()
    {
        //Rotate to be parallel to the landing strip
        float relRotY = transform.GetChild(0).eulerAngles.y - pointA.transform.eulerAngles.y - 180;
        //Debug.Log(transform.GetChild(0).eulerAngles.y + " r|r " + pointA.transform.eulerAngles.y);

        //If outside of runway turn onto runway
        Vector3 directionBetween = transform.GetChild(0).position - pointA.transform.position;
        float disFromPointA = Vector3.Project(directionBetween, pointA.transform.right).magnitude - (pointA.stripWidth / 2);
        disFromPointA = Mathf.Min(disFromPointA, 10);
        //Debug.Log(relRotY + " | " + disFromPointA);


        //if (disFromPointA > -2)
        //{

        //    if (dotRight > dotLeft)
        //    {
        //        if (relRotY < returnToStripMinRot)
        //            _controller.yawn = disFromPointA / 8 * returnToStripRotMult;
        //        else
        //            _controller.yawn = 0;
        //    }
        //    else
        //    {
        //        if (relRotY > -returnToStripMinRot)
        //            _controller.yawn = -disFromPointA / 8 * returnToStripRotMult;
        //        else
        //            _controller.yawn = 0;
        //    }
        //}

        float dotRight = Vector3.Dot(directionBetween.normalized, pointA.transform.right);
        float dotLeft = Vector3.Dot(directionBetween.normalized, -pointA.transform.right);

        float targetAngleMax = returnToStripAngle + returnToStripBuffer;
        float targetAngleMin = returnToStripAngle - returnToStripBuffer;

        if (disFromPointA > -2)
        {
            if (dotRight > dotLeft)
            {
                if (relRotY < targetAngleMax && relRotY > targetAngleMin)
                {
                    _controller.yawn = 0;
                }
                else
                {
                    if (relRotY > returnToStripAngle)
                        _controller.yawn = -disFromPointA / 8 * returnToStripRotMult;
                    else
                        _controller.yawn = disFromPointA / 8 * returnToStripRotMult;
                }
            }
            else
            {
                if (relRotY > -targetAngleMax && relRotY < -targetAngleMin)
                {
                    _controller.yawn = 0;
                }
                else
                {
                    if (relRotY > -returnToStripAngle)
                        _controller.yawn = -disFromPointA / 8 * returnToStripRotMult;
                    else
                        _controller.yawn = disFromPointA / 8 * returnToStripRotMult;
                }
            }

            //Slow Down
            if (_physics.thrust >= returnToStripMaxThrust)
            {
                _controller.thrust = false;
            }
            else
            {
                _controller.thrust = true;
            }
        }
        else
        {
            // Become Parallel
            if (relRotY != 0)
            {
                _controller.yawn = Mathf.Clamp(-relRotY / 15, -1, 1) * parallelRotMult;
            }

            _controller.thrust = true;
        }
    }
}
