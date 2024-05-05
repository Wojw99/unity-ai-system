using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBrain : MonoBehaviour
{
    private GoalName _goal;

    public event Action<GoalName> GoalChanged;
    public event Action DepositExtracted;
    public event Action<float> DamageTaken;

    private AgentStatus agentStatus;
    private AgentVisionSensor agentVisionSensor;
    private AgentInteractionSensor agentInteractionSensor;
    
    private void Start()
    {
        agentStatus = GetComponent<AgentStatus>();
        agentVisionSensor = GetComponent<AgentVisionSensor>();
        agentInteractionSensor = GetComponent<AgentInteractionSensor>();

        agentVisionSensor.EnemySpotted += OnEnemySpotted;
        agentVisionSensor.DepositSpotted += OnDepositSpotted;
        agentVisionSensor.HealSpotted += OnHealSpotted;
        agentVisionSensor.RestSpotted += OnRestSpotted;

        agentInteractionSensor.InteractionStarted += OnInteractionStarted;
        agentInteractionSensor.InteractionEnded += OnInteractionEnded; 
        agentInteractionSensor.InteractionExited += OnInteractionExited;

        agentInteractionSensor.ModifierStarted += OnModifierStarted;

        // Give other components time to subscribe to the GoalChanged event
        StartCoroutine(ConsiderGoalChanging(1f));
    }

    private IEnumerator ConsiderGoalChanging(float delay) {
        yield return new WaitForSeconds(delay);
        ConsiderGoalChanging();
    }

    private void OnInteractionStarted(InteractionType interactionType) {
        if(interactionType == InteractionType.DEPOSIT 
            && (Goal == GoalName.SEARCH_FOR_DEPOSIT || Goal == GoalName.GO_TO_NEAREST_DEPOSIT)) 
        {
            Goal = GoalName.MINE_DEPOSIT;
        }
        if(interactionType == InteractionType.REST 
            && (Goal == GoalName.SEARCH_FOR_REST || Goal == GoalName.GO_TO_NEAREST_REST)) 
        {
            Goal = GoalName.TAKE_REST;
        }
        if(interactionType == InteractionType.DAMAGE) 
        {
            Goal = GoalName.TAKE_DAMAGE;
            // DamageTaken?.Invoke();
        }
    }

    private void OnModifierStarted(ModifierInfo modifierInfo) {
        if(modifierInfo.modifierType == ModifierType.DAMAGE) 
        {
            // Goal = GoalName.TAKE_DAMAGE;
            DamageTaken?.Invoke(modifierInfo.modifierValue);
            // StartCoroutine(ConsiderGoalChanging(0.5f));
        } 
        else if (modifierInfo.modifierType == ModifierType.HEAL) 
        {
            DamageTaken?.Invoke(modifierInfo.modifierValue * -1f);
        }
    }

    private void OnInteractionEnded(InteractionType interactionType) {
        if(interactionType == InteractionType.DEPOSIT) {
            DepositExtracted?.Invoke();
        }
        ConsiderGoalChanging();
    }

    private void OnInteractionExited(InteractionType interactionType) {
        ConsiderGoalChanging();
    }

    private void OnHealSpotted() {
        if(Goal == GoalName.SEARCH_FOR_HEALING)
        {
            Goal = GoalName.GO_TO_NEAREST_HEALING;
        }
    }

    private void OnDepositSpotted() {
        if(Goal == GoalName.SEARCH_FOR_DEPOSIT)
        {
            Goal = GoalName.GO_TO_NEAREST_DEPOSIT;
        }
    }

    private void OnRestSpotted() {
        if(Goal == GoalName.SEARCH_FOR_REST)
        {
            Goal = GoalName.GO_TO_NEAREST_REST;
        }
    }

    private void OnEnemySpotted() {
        Goal = GoalName.RUN_FOR_YOUR_LIFE;
    }

    private void Update() {
        
    }

    private void ConsiderGoalChanging() {
        var calculatedGoal = CalculateGoal();
        
        if (Goal != calculatedGoal) {
            Goal = calculatedGoal;
        }

        Debug.Log($"Consider goal changing. Goal: {Goal}");
    }

    private GoalName CalculateGoal() {
        var goal = GoalName.SEARCH_FOR_DEPOSIT;

        if(agentStatus.Stamina <= agentStatus.MaxStamina / 3f) {
            goal = GoalName.SEARCH_FOR_REST;
        }

        if(agentStatus.Health <= agentStatus.MaxHealth / 2f) {
            goal = GoalName.SEARCH_FOR_HEALING;
        }

        return goal;
    }

    public GoalName Goal {
        get => _goal;
        set {
            _goal = value;
            GoalChanged?.Invoke(_goal);
        }
    }

    public enum GoalName
    {
        FREEZE,
        RUN_FOR_YOUR_LIFE,
        LEAVE_THE_AREA,
        TAKE_DAMAGE,
        
        SEARCH_FOR_DEPOSIT,
        GO_TO_NEAREST_DEPOSIT,
        MINE_DEPOSIT,

        SEARCH_FOR_REST,
        GO_TO_NEAREST_REST,
        TAKE_REST,

        SEARCH_FOR_HEALING,
        GO_TO_NEAREST_HEALING,
        TAKE_HEALING,
    }
}
