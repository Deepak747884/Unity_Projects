using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gaurd : MonoBehaviour {

    public static event System.Action OnGaurdHasSpottedPlayer;

    public float speed = 3;
    public float waitTime = .3f;
    public float turnSpeed = 90;

    public Light spotlight;
    public float viewDistance;
    float viewAngle;
    Color originalSpotLightColor;

    public Transform pathHolder;

    Transform player;
    public LayerMask viewMask;

    public float timeToSpotPlayer = .5f;
    float playerVisibleTimer;

    private void Start()
    {
        originalSpotLightColor = spotlight.color;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        viewAngle = spotlight.spotAngle;

        Vector3[] waypoints = new Vector3[pathHolder.childCount];

        for (int i= 0;i< waypoints.Length;i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        StartCoroutine(FollowPath(waypoints));
    }

    bool CanSeePlayer()
    {
        if (Vector3.Distance(transform.position,player.position) < viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGaurdAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);

            if (angleBetweenGaurdAndPlayer < viewAngle/2f)
            {
                if (!Physics.Linecast(transform.position, player.position, viewMask)) //mask for obstacles
                {
                    return true;
                }
            }
        }

        return false;
    }

    void Update()
    {        
        if (CanSeePlayer())
        {
            playerVisibleTimer += Time.deltaTime;
        }
        else
        {
            playerVisibleTimer -= Time.deltaTime;
        }

        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, timeToSpotPlayer);
        spotlight.color = Color.Lerp(originalSpotLightColor, Color.red, playerVisibleTimer / timeToSpotPlayer);

        if (playerVisibleTimer >= timeToSpotPlayer)
        {
            if (OnGaurdHasSpottedPlayer != null)
            {
                OnGaurdHasSpottedPlayer();
            }
        }
    }

    IEnumerator FollowPath(Vector3[] waypoints)
    {
        transform.position = waypoints[0];

        int targetWaypointIndex = 1;
        Vector3 targetwaypoint = waypoints[targetWaypointIndex];

        transform.LookAt(targetwaypoint);

        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetwaypoint, speed * Time.deltaTime);

            if (transform.position == targetwaypoint)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length; //loop back to 1 
                targetwaypoint = waypoints[targetWaypointIndex];

                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetwaypoint));
            }

            yield return null; //for 1 frame wait
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y,targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    } 

    void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward*viewDistance);
    }
}
