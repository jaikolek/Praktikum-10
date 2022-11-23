using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TakeCover : MonoBehaviour
{
    NavMeshAgent nav;
    public int frameInterval = 10;
    public int facePlayerFactor = 50;

    Vector3 randomPos;
    Vector3 coverPoint;
    public float rangeRandPoint = 6f;
    public bool isHiding = false;

    public LayerMask coverLayer;
    Vector3 coverObj;
    public LayerMask visibleLayer;

    public float maxCovDist = 30;
    public bool coverIsClose;
    public bool coverNotReached = true;

    public float distToCoverPos = 1f;
    public float distToCoverObj = 20f;

    public float rangeDist = 15;
    private bool playerInRange = false;

    private int testCoverPos = 10;

    public GameObject player;

    bool RandomPoint(Vector3 center, float rangeRandPoint, out Vector3 resultCover)
    {
        for(int i = 0; i < testCoverPos; i++)
        {
            randomPos = center + Random.insideUnitSphere * rangeRandPoint;
            Vector3 direction = player.transform.position - randomPos;
            RaycastHit hitTestCov;
            if(Physics.Raycast(randomPos, direction.normalized, out hitTestCov, rangeRandPoint, visibleLayer))
            {
                if(hitTestCov.collider.gameObject.layer == 18)
                {
                    resultCover = randomPos;
                    return true;
                }
            }
        }
        resultCover = Vector3.zero;
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(nav.isActiveAndEnabled)
        {
            if(Time.frameCount % frameInterval == 0)
            {
                float distance = ((player.transform.position - transform.position).sqrMagnitude);

                if (distance < rangeDist * rangeDist)
                {
                    playerInRange = true;
                }
                else playerInRange = false;
            }

            if (playerInRange == true)
            {
                CheckOverDist();

                if(coverIsClose == true)
                {
                    if(coverNotReached == true)
                    {
                        nav.SetDestination(coverObj);
                    }
                    if(coverNotReached == false)
                    {
                        Cover();

                        FacePlayer();
                    }
                }
                if (coverIsClose == false)
                {

                }
            }
        }
    }

    void CheckOverDist()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, maxCovDist, coverLayer);
        Collider nearestCollider = null;
        float minSqrDistance = Mathf.Infinity;

        Vector3 AIPos = transform.position;

        for (int i = 0; i < colliders.Length; i++)
        {
            float sqrDistanceToCenter = (AIPos - colliders[i].transform.position).sqrMagnitude;
            if(sqrDistanceToCenter < minSqrDistance)
            {
                minSqrDistance = sqrDistanceToCenter;
                nearestCollider = colliders[i];

                float coverDistance = (nearestCollider.transform.position - AIPos).sqrMagnitude;

                if(coverDistance <= maxCovDist * maxCovDist)
                {
                    coverIsClose = true;
                    coverObj = nearestCollider.transform.position;
                    if (coverDistance <= distToCoverObj * distToCoverObj)
                    {
                        coverNotReached = false;
                    }
                    else if (coverDistance > distToCoverObj * distToCoverObj)
                    {
                        coverNotReached = true;
                    }
                }

                if(coverDistance >= maxCovDist*maxCovDist)
                {
                    coverIsClose = false;
                }
            }
        }
        if(colliders.Length < 1)
        {
            coverIsClose = false;
        }
    }

    void Cover()
    {
        if(RandomPoint(transform.position, rangeRandPoint, out coverPoint))
        {
            if(nav.isActiveAndEnabled)
            {
                nav.SetDestination(coverPoint);
                if((coverPoint - transform.position).sqrMagnitude <= distToCoverObj*distToCoverObj)
                {
                    isHiding = true;
                }
            }
        }
    }

    void FacePlayer()
    {
        Vector3 dir = (player.transform.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * facePlayerFactor);
    }
}
