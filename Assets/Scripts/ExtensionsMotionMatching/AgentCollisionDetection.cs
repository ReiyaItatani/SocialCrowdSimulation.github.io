using System.Collections;
using UnityEngine;
using MotionMatching;

/// <summary>
/// Handles collision detection and responses for an agent in a virtual environment.
/// This class is responsible for detecting collisions with other agents and walls, 
/// and managing the agent's behavior in response to these collisions. 
/// It utilizes a CapsuleCollider for collision detection and interacts with 
/// both the PathController and SocialBehaviour components to adjust the agent's 
/// movement and social interactions based on collisions.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(SocialBehaviour))]
public class AgentCollisionDetection : MonoBehaviour
{
    [Header("Collision Handling Parameters")]
    private PathController pathController;
    private CapsuleCollider capsuleCollider;
    private bool isColliding = false;
    private bool isMoving = false;
    public SocialBehaviour socialBehaviour;

    [Header("Repulsion Force Parameters")]
    private GameObject currentWallTarget;

    // For debugging collision detection.
    [HideInInspector]
    public Camera collisionDetectionCamera;

    private const float cameraPositionOffsetX = -0.4f;
    private const float cameraPositionOffsetY = 1.7f;
    private const float cameraPositionOffsetZ = -2.5f;
    private const float cameraAdjustmentDuration = 5.0f;
    private const float minReactionTime = 3f;
    private const float maxReactionTime = 7f;

    void Awake()
    {
        socialBehaviour = GetComponent<SocialBehaviour>();
        if (socialBehaviour == null)
        {
            Debug.LogError("SocialBehaviour component not found on this GameObject.");
        }
    }

    void Update()
    {
        if (pathController != null)
        {
            Vector3 currentPosition = pathController.GetCurrentPosition();
            Vector3 offset = new Vector3(currentPosition.x - transform.position.x, capsuleCollider.center.y, currentPosition.z - transform.position.z);
            capsuleCollider.center = offset;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Agent") && pathController != null)
        {
            HandleAgentCollision(other);
        }
        else if (other.CompareTag("Wall"))
        {
            currentWallTarget = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            currentWallTarget = null;
        }
    }

    /// <summary>
    /// Initializes required parameters for collision detection.
    /// </summary>
    /// <param name="_pathController">PathController component reference.</param>
    /// <param name="_capsuleCollider">CapsuleCollider component reference.</param>
    public void InitParameter(PathController _pathController, CapsuleCollider _capsuleCollider)
    {
        pathController = _pathController;
        capsuleCollider = _capsuleCollider;
    }

    /// <summary>
    /// Coroutine that handles the agent's reaction when a collision occurs.
    /// </summary>
    /// <param name="time">Time duration of the reaction.</param>
    /// <param name="collidedAgent">The agent that was collided with.</param>
    /// <returns></returns>
    public IEnumerator ReactionToCollision(float time, GameObject collidedAgent)
    {
        isColliding = true;
        pathController.SetOnCollide(isColliding);

        socialBehaviour.SetCollidedTarget(collidedAgent);
        socialBehaviour.TryPlayAudio();
        socialBehaviour.TriggerUnityAnimation(UpperBodyAnimationState.Talk);
        yield return new WaitForSeconds(time / 2.0f);

        socialBehaviour.DeleteCollidedTarget();
        socialBehaviour.FollowMotionMatching();

        isMoving = true;
        pathController.SetOnMoving(isMoving);
        yield return new WaitForSeconds(time / 2.0f);

        ResetCollisionStates();
    }

    /// <summary>
    /// Returns the current wall target the agent is interacting with.
    /// </summary>
    /// <returns>Current wall target GameObject.</returns>
    public GameObject GetCurrentWallTarget()
    {
        return currentWallTarget;
    }

    /// <summary>
    /// Returns the current look-at position of the social behaviour.
    /// </summary>
    /// <returns>Current look-at position as a Vector3.</returns>
    public Vector3 GetCurrentLookAt()
    {
        return socialBehaviour.GetCurrentLookAt();
    }

    /// <summary>
    /// Resets collision states back to their default values.
    /// </summary>
    private void ResetCollisionStates()
    {
        isColliding = false;
        isMoving = false;
        pathController.SetOnCollide(isColliding);
        pathController.SetOnMoving(isMoving);
    }

    /// <summary>
    /// Handles collision with another agent.
    /// </summary>
    /// <param name="collidingAgent">The agent that was collided with.</param>
    private void HandleAgentCollision(Collider collidingAgent)
    {
        ResetCollisionStates();
        pathController.SetCollidedAgent(collidingAgent.gameObject);

        if (socialBehaviour != null)
        {
            StartCoroutine(ReactionToCollision(Random.Range(minReactionTime, maxReactionTime), collidingAgent.gameObject));
        }

        if (collisionDetectionCamera != null)
        {
            AdjustCameraPosition(collidingAgent.transform.position);
        }
    }

    /// <summary>
    /// Adjusts the camera's position for a better view of the collision.
    /// </summary>
    /// <param name="targetPosition">The target position to adjust the camera towards.</param>
    private void AdjustCameraPosition(Vector3 targetPosition)
    {
        ChangeCamPosChecker camPosChecker = collisionDetectionCamera.GetComponent<ChangeCamPosChecker>();

        if (camPosChecker != null && !camPosChecker.ChangeCamPos)
        {
            camPosChecker.ChangeCamPos = true;
            collisionDetectionCamera.transform.position = targetPosition + new Vector3(cameraPositionOffsetX, cameraPositionOffsetY, cameraPositionOffsetZ);
            StartCoroutine(DurationAfterCameraPositionChange(camPosChecker, cameraAdjustmentDuration));
        }
        else
        {
            Debug.LogError("ChangeCamPosChecker component not found on collisionDetectionCamera or camera is already adjusted.");
        }
    }

    /// <summary>
    /// Coroutine to reset camera position adjustment after a duration.
    /// </summary>
    /// <param name="changeCamPosChecker">The ChangeCamPosChecker component reference.</param>
    /// <param name="duration">Duration to wait before resetting the camera position adjustment.</param>
    /// <returns></returns>
    private IEnumerator DurationAfterCameraPositionChange(ChangeCamPosChecker changeCamPosChecker, float duration)
    {
        yield return new WaitForSeconds(duration);
        changeCamPosChecker.ChangeCamPos = false;
    }
}
