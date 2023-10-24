using System.Collections.Generic;
using UnityEngine;
using MotionMatching;
using System.Security.Cryptography;

// AgentManager is a class that manages various parameters and settings for agents in a simulation.
public class AgentManager : MonoBehaviour
{
    // Basic Collision Avoidance Parameters: Parameters that define how agents avoid collisions.
    [Header("Basic Collision Avoidance Parameters")]
    [Tooltip("Size of the avoidance collider.")]
    public Vector3 avoidanceColliderSize = new Vector3(1.5f, 1.5f, 2.0f);
    [Tooltip("Radius to consider as the goal.")]
    [Range(0.1f, 5.0f)]
    public float goalRadius = 0.5f;
    [Tooltip("Radius to start slowing down.")]
    [Range(0.1f, 5.0f)]
    public float slowingRadius = 2.0f;

    // Parameters For Unaligned Collision Avoidance: Parameters that define how agents avoid collisions when they are not aligned.
    [Header("Parameters For Unaligned Collision Avoidance")]
    [Tooltip("Size of the unaligned avoidance collider.")]
    public Vector3 unalignedAvoidanceColliderSize = new Vector3(4.5f, 1.5f, 6.0f); 

    // Weights for various forces influencing agent movement.
    [Space]
    [Tooltip("Weight for moving towards the goal.")]
    [Range(0.0f, 5.0f)]
    public float toGoalWeight = 2.0f;
    [Tooltip("Weight to avoid neighbors.")]
    [Range(0.0f, 5.0f)]
    public float avoidNeighborWeight = 2.0f;
    [Tooltip("Weight for general avoidance.")]
    [Range(0.0f, 5.0f)]
    public float avoidanceWeight = 3.0f;
    [Tooltip("Weight for group force.")]
    [Range(0.0f, 5.0f)]
    public float groupForceWeight = 1.0f;
    [Tooltip("Weight for wall force.")]
    [Range(0.0f, 5.0f)]
    public float wallRepForceWeight = 1.0f;
    [Tooltip("Weight for synthetic vision force.")]
    [Range(0.0f, 5.0f)]
    public float syntheticVisionForceWeight = 1.0f;

    // Parameters related to the adjustment of the position of the SimulationBone and SimulationObject.
    [Space]
    [Range(0.0f, 2.0f), Tooltip("Max distance between SimulationBone and SimulationObject")] 
    public float MaxDistanceMMAndCharacterController = 0.1f;
    [Range(0.0f, 2.0f), Tooltip("Time needed to move half of the distance between SimulationBone and SimulationObject")] 
    public float PositionAdjustmentHalflife = 0.1f;
    [Range(0.0f, 2.0f), Tooltip("Ratio between the adjustment and the character's velocity to clamp the adjustment")] 
    public float PosMaximumAdjustmentRatio = 0.1f;

    // Parameters defining the size of the agent's capsule collider.
    [Header("Agent Capsule Collider Size")]
    [Range(0.0f, 1.0f)]
    public float CapsuleColliderRadius = 0.25f; 

    // Parameters to control the display of various debug gizmos in the Unity Editor.
    [Header("Controll Gizmos Parameters")]
    public bool showAgentSphere = false;
    public bool ShowAvoidanceForce = false;
    public bool ShowUnalignedCollisionAvoidance = false;
    public bool ShowGoalDirection = false;
    public bool ShowCurrentDirection = false;
    public bool ShowGroupForce = false;
    public bool ShowWallForce = false;

    // Parameters for debugging the Motion Matching Controller.
    [Header("Motion Matching Controller Debug")]
    public bool DebugSkeleton = false;
    public bool DebugCurrent = false;
    public bool DebugPose = false;
    public bool DebugTrajectory = false;
    public bool DebugContacts = false;

    // OCEAN Personality Model Parameters: Parameters that define the personality of the agent according to the OCEAN model.
    [Header("OCEAN Parameters")]
    [Range(-1f, 1f),HideInInspector] public float openness = 0f;
    [Range(-1f, 1f),HideInInspector] public float conscientiousness = 0f;
    [Range(-1f, 1f)] public float Negative_Positive = 0f;
    [Range(-1f, 1f),HideInInspector] public float agreeableness = 0f;
    [Range(-1f, 1f),HideInInspector] public float neuroticism = 0f;

    // Emotion Parameters: Parameters that define the emotional state of the agent.
    [Header("Emotion Parameters")]
    [Range(0f, 1f)] public float e_happy = 0f;
    [Range(0f, 1f)] public float e_sad = 0f;
    [Range(0f, 1f)] public float e_angry = 0f;
    [Range(0f, 1f)] public float e_disgust = 0f;
    [Range(0f, 1f)] public float e_fear = 0f;
    [Range(0f, 1f)] public float e_shock = 0f;

    // Lists to store references to various controller game objects.
    private List<GameObject> PathControllers = new List<GameObject>();
    private List<GameObject> MotionMatchingControllers = new List<GameObject>();
    private List<GameObject> CollisionAvoidanceControllers = new List<GameObject>();
    private List<GameObject> MotionMatchingSkinnedMeshRendererWithOCEANs = new List<GameObject>();
    private List<GameObject> Avatars = new List<GameObject>();
    private AvatarCreatorBase avatarCreator;

    // Parameters related to social behavior of the agent.
    [Header("Social Behaviour")]
    public bool onTalk = false;
    public bool onAnimation = true;

    // Parameters related to collision detection.
    [Header("Collision Detection")]
    public Camera collisionDetectionCam;

    // Awake is called when the script instance is being loaded.
    void Awake(){
        // Get a reference to the AvatarCreatorBase component.
        avatarCreator = this.GetComponent<AvatarCreatorBase>();
        // Get a list of instantiated avatars from the AvatarCreatorBase.
        Avatars = avatarCreator.instantiatedAvatars; 
    }

    // Start is called before the first frame update.
    void Start()
    {
        // Loop through all avatars and set their parameters.
        for (int i = 0; i < Avatars.Count; i++)
        {
            // Get and set PathController parameters.
            PathController pathController = Avatars[i].GetComponentInChildren<PathController>();
            if(pathController != null) {
                SetPathControllerParams(pathController);
                PathControllers.Add(pathController.gameObject);
            }

            // Get and set MotionMatchingController parameters.
            MotionMatchingController motionMatchingController = Avatars[i].GetComponentInChildren<MotionMatchingController>();
            if(motionMatchingController != null) {
                SetMotionMatchingControllerParams(motionMatchingController);
                MotionMatchingControllers.Add(motionMatchingController.gameObject);
            }

            // Get and set CollisionAvoidance parameters.
            CollisionAvoidanceController collisionAvoidanceController = Avatars[i].GetComponentInChildren<CollisionAvoidanceController>();
            if(collisionAvoidanceController != null) {
                SetCollisionAvoidanceControllerParams(collisionAvoidanceController);
                CollisionAvoidanceControllers.Add(collisionAvoidanceController.gameObject);
            }

            // Get and set MotionMatchingSkinnedMeshRendererWithOCEAN parameters.
            MotionMatchingSkinnedMeshRendererWithOCEAN mmSMRWithOCEAN = Avatars[i].GetComponentInChildren<MotionMatchingSkinnedMeshRendererWithOCEAN>();
            if(mmSMRWithOCEAN != null) {
                SetMotionMatchingSkinnedMeshRendererWithOCEANParams(mmSMRWithOCEAN);
                MotionMatchingSkinnedMeshRendererWithOCEANs.Add(mmSMRWithOCEAN.gameObject);
            }

            // Get and set SocialBehaviour parameters.
            SocialBehaviour socialBehaviour = Avatars[i].GetComponentInChildren<SocialBehaviour>();
            if(socialBehaviour != null) {
                SetSocialBehaviourParams(socialBehaviour);
            }     

            // Get and set AgentCollisionDetection parameters.
            AgentCollisionDetection agentCollisionDetection = Avatars[i].GetComponentInChildren<AgentCollisionDetection>();
            if(agentCollisionDetection != null){
                SetCollisionDetectionParams(agentCollisionDetection);
            }
        }
    }

    // OnValidate is called when the script is loaded or a value is changed in the Inspector.
    private void OnValidate() {
        // Loop through all PathControllers and set their parameters.
        foreach(GameObject controllerObject in PathControllers) 
        {
            PathController pathController = controllerObject.GetComponent<PathController>();
            if(pathController != null) 
            {
                SetPathControllerParams(pathController);
            }
        }

        // Loop through all MotionMatchingControllers and set their parameters.
        foreach(GameObject controllerObject in MotionMatchingControllers) 
        {
            MotionMatchingController motionMatchingController = controllerObject.GetComponent<MotionMatchingController>();
            if(motionMatchingController != null) 
            {
                SetMotionMatchingControllerParams(motionMatchingController);
            }
        }

        // Loop through all CollisionAvoidanceControllers and set their parameters.
        foreach(GameObject controllerObject in CollisionAvoidanceControllers) 
        {
            CollisionAvoidanceController collisionAvoidanceController = controllerObject.GetComponent<CollisionAvoidanceController>();
            if(collisionAvoidanceController != null) 
            {
                SetCollisionAvoidanceControllerParams(collisionAvoidanceController);
            }
        }

        // Loop through all MotionMatchingSkinnedMeshRendererWithOCEANs and set their parameters.
        foreach(GameObject controllerObject in MotionMatchingSkinnedMeshRendererWithOCEANs) 
        {
            MotionMatchingSkinnedMeshRendererWithOCEAN mmSMRWithOCEAN = controllerObject.GetComponent<MotionMatchingSkinnedMeshRendererWithOCEAN>();
            if(mmSMRWithOCEAN != null) 
            {
                SetMotionMatchingSkinnedMeshRendererWithOCEANParams(mmSMRWithOCEAN);
            }
            SocialBehaviour socialBehaviour = controllerObject.GetComponent<SocialBehaviour>();
            if(socialBehaviour != null) {
                SetSocialBehaviourParams(socialBehaviour);
            }    
        }
    }

    // Method to set parameters for PathController.
    private void SetPathControllerParams(PathController pathController){
        pathController.goalRadius = goalRadius;
        pathController.slowingRadius = slowingRadius;

        pathController.toGoalWeight = toGoalWeight;
        pathController.avoidanceWeight = avoidanceWeight;
        pathController.avoidNeighborWeight = avoidNeighborWeight;
        pathController.groupForceWeight = groupForceWeight;
        pathController.wallRepForceWeight = wallRepForceWeight;
        pathController.syntheticVisionForceWeight = syntheticVisionForceWeight;

        pathController.MaxDistanceMMAndCharacterController = MaxDistanceMMAndCharacterController;
        pathController.PositionAdjustmentHalflife = PositionAdjustmentHalflife;
        pathController.PosMaximumAdjustmentRatio = PosMaximumAdjustmentRatio;

        pathController.showAvoidanceForce = ShowAvoidanceForce;
        pathController.showUnalignedCollisionAvoidance = ShowUnalignedCollisionAvoidance;
        pathController.showGoalDirection = ShowGoalDirection;
        pathController.showCurrentDirection = ShowCurrentDirection;
        pathController.showGroupForce = ShowGroupForce;
        pathController.showWallForce = ShowWallForce;
    }

    private void SetMotionMatchingControllerParams(MotionMatchingController motionMatchingController){
        // motionMatchingController.SpheresRadius = SphereRadius;
        motionMatchingController.DebugSkeleton = DebugSkeleton;
        motionMatchingController.DebugCurrent = DebugCurrent;
        motionMatchingController.DebugPose = DebugPose;
        motionMatchingController.DebugTrajectory = DebugTrajectory;
        motionMatchingController.DebugContacts = DebugContacts;
    }

    private void SetCollisionAvoidanceControllerParams(CollisionAvoidanceController collisionAvoidanceController){
        collisionAvoidanceController.avoidanceColliderSize = avoidanceColliderSize;
        collisionAvoidanceController.unalignedAvoidanceColliderSize = unalignedAvoidanceColliderSize;
        collisionAvoidanceController.agentCollider.radius = CapsuleColliderRadius;
        collisionAvoidanceController.showAgentSphere = showAgentSphere;
    }

    private void SetMotionMatchingSkinnedMeshRendererWithOCEANParams(MotionMatchingSkinnedMeshRendererWithOCEAN mmSMRWithOCEAN){
        mmSMRWithOCEAN.openness = openness;
        mmSMRWithOCEAN.conscientiousness = conscientiousness;
        mmSMRWithOCEAN.extraversion = Negative_Positive;
        mmSMRWithOCEAN.agreeableness = agreeableness;
        mmSMRWithOCEAN.neuroticism = neuroticism;
        mmSMRWithOCEAN.e_happy = e_happy;
        mmSMRWithOCEAN.e_sad = e_sad;
        mmSMRWithOCEAN.e_angry = e_angry;
        mmSMRWithOCEAN.e_disgust = e_disgust;
        mmSMRWithOCEAN.e_fear = e_fear;
        mmSMRWithOCEAN.e_shock = e_shock;      
    }

    private void SetCollisionDetectionParams(AgentCollisionDetection agentCollisionDetection){
        agentCollisionDetection.collisionDetectionCam = collisionDetectionCam;
    }

    private void SetSocialBehaviourParams(SocialBehaviour socialBehaviour){
        socialBehaviour.onTalk = onTalk;
        socialBehaviour.onAnimation = onAnimation;
    }

}
