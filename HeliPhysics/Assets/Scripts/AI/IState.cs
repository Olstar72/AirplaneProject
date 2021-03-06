﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IState
{
    void Setup(PlaneController pController, PlanePhysics pPhysics); // Replacement for Start
    void Enter(); // When entering state
    void Exit(); // When Exiting state
    bool CanExit(); // Check if exiting is possible
    void Tick(); // Replacement for Update (Called every frame when it is the current state)
}
