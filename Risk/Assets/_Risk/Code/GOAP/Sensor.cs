using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;

[RequireComponent(typeof(SphereCollider))]
public class Sensor : MonoBehaviour
{
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float evaluationInterval = 1f;

    SphereCollider detectionSphere;

    /// <summary>
    /// Event triggered when there is new target or the old one is moved.
    /// </summary>
    public event Action TargetChanged = delegate { };
    public Vector3 TargetPosition => target ? target.transform.position : Vector3.zero;
    public bool IsTargetInRange => TargetPosition != Vector3.zero;

    GameObject target;
    Vector3 lastKnownPosition;
    CountdownTimer timer;

    void Awake() {
        detectionSphere = GetComponent<SphereCollider>();
        detectionSphere.isTrigger = true;
        detectionSphere.radius = detectionRange;
    }

    void Start() {
        timer = new CountdownTimer(evaluationInterval);
        timer.OnTimerStop += () => {
            UpdateTargetPosition(target.OrNull());
            timer.Start();
        };
        timer.Start();
    }

    void Update() {
        timer.Tick(Time.deltaTime);
    }

    // TODO: Add a way to check not only the player, but also other agents.
    void OnTriggerEnter(Collider other) {
        if(!other.CompareTag("Player")) return;
        UpdateTargetPosition(other.gameObject);
    }

    void OnTriggerExit(Collider other) {
        if(!other.CompareTag("Player")) return;
        UpdateTargetPosition();
    }

    void UpdateTargetPosition(GameObject target = null) {
        this.target = target;
        if(IsTargetInRange && (lastKnownPosition != TargetPosition || lastKnownPosition != Vector3.zero)) {
            lastKnownPosition = TargetPosition;
            TargetChanged.Invoke();
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = IsTargetInRange ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
