using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

namespace CollisionAvoidance{

public class GroupColliderManager : MonoBehaviour
{
    public bool OnGroupCollider = true;

    public SocialRelations socialRelations;
    public AvatarCreatorBase avatarCreator;
    public GameObject groupColliderGameObject;
    private CapsuleCollider groupCollider;
    private List<CollisionAvoidanceController> collisionAvoidanceControllers = new List<CollisionAvoidanceController>();
    private HashSet<GameObject> agentsInFOV = new HashSet<GameObject>();
    [ReadOnly]
    public List<GameObject> debug = new List<GameObject>();

    public bool onGroupCollider = false;

    private List<GameObject> agentsInCategory = new List<GameObject>();

    void Start()
    {
        agentsInCategory = avatarCreator.GetAgentsInCategory(socialRelations);
        foreach(GameObject agent in agentsInCategory){
            collisionAvoidanceControllers.Add(agent.GetComponent<ParameterManager>().GetCollisionAvoidanceController());
        }
        StartCoroutine(UpdateAgentsInGroupFOV(0.1f));
        groupCollider = groupColliderGameObject.GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        UpdateCenterOfMass();
        DistanceChecker();
    }

    void UpdateCenterOfMass()
    {
        Vector3 combinedPosition = Vector3.zero;
        foreach (GameObject agent in agentsInCategory)
        {
            combinedPosition += agent.transform.position;
        }
        this.transform.position = combinedPosition / agentsInCategory.Count;
    }

    private void DistanceChecker(){
        float maxDistance = 0f;
        foreach (GameObject agent in agentsInCategory)
        {
            float distance = Vector3.Distance(this.transform.position, agent.transform.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
            }
        }
        if(maxDistance <= (agentsInCategory.Count)/2 && OnGroupCollider){
            groupCollider.enabled = true;
            //groupColliderGameObject.SetActive(true);
            onGroupCollider = true;
        }else{
            groupCollider.enabled = false;
            //groupColliderGameObject.SetActive(false);
            onGroupCollider = false;
            agentsInFOV.Clear();
        }
    }

    public bool GetOnGroupCollider(){
        return onGroupCollider;
    }

    public List<GameObject> GetAgentsInSharedFOV(){
        return agentsInFOV.ToList();
    }

    private IEnumerator UpdateAgentsInGroupFOV(float updateTime){
        while(true){
            agentsInFOV.Clear();
            foreach(CollisionAvoidanceController collisionAvoidanceController in collisionAvoidanceControllers){
                agentsInFOV.UnionWith(collisionAvoidanceController.GetOthersInFOV());
            }
            //remove agents in same category
            agentsInFOV.ExceptWith(agentsInCategory); 
            debug = agentsInFOV.ToList();
            yield return new WaitForSeconds(updateTime);
        }
    }
}
}