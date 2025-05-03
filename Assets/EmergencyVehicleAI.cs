using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class EmergencyVehicleAI : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public GameObject waypointPrefab;
    public int numberOfWaypoints = 5;
    public Vector3 spawnAreaSize = new Vector3(50, 0, 50);

    [Header("Agent Settings")]
    public float reachThreshold = 1.5f;
    public float waitTimeAtWaypoint = 2f;

    [Header("UI")]
    public TextMeshProUGUI statusText;

    private Transform[] waypoints;
    private NavMeshAgent agent;
    private int currentWaypointIndex = -1;
    private bool isWaiting = false;

    private string[] dispatchPhrases = {
        "🚨 Urgent Dispatch: Heading to {0}!",
        "🚑 Emergency Call: Navigating to {0}!",
        "🚒 Rapid Response: Moving toward {0}!",
        "🛻 Quick Dispatch: Approaching {0}!"
    };

    private string[] arrivalPhrases = {
        "✅ Target {0} Reached. Awaiting further orders...",
        "🛬 Arrived at {0}. Standing by for next dispatch.",
        "📍 Successfully reached {0}. Holding position.",
        "🛑 {0} secured. Preparing for next operation..."
    };

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GenerateWaypoints();
        GoToNextWaypoint();
    }

    void Update()
    {
        if (!isWaiting && !agent.pathPending && agent.remainingDistance <= reachThreshold)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    void GenerateWaypoints()
    {
        waypoints = new Transform[numberOfWaypoints];

        for (int i = 0; i < numberOfWaypoints; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                0f,
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );

            GameObject wp = Instantiate(waypointPrefab, randomPos, Quaternion.identity);
            wp.name = "Waypoint_" + i;
            waypoints[i] = wp.transform;
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        currentWaypointIndex = Random.Range(0, waypoints.Length);
        agent.SetDestination(waypoints[currentWaypointIndex].position);

        if (statusText != null)
        {
            string dispatchMessage = string.Format(GetRandomPhrase(dispatchPhrases), waypoints[currentWaypointIndex].name);
            statusText.text = dispatchMessage;
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;

        if (statusText != null)
        {
            string arrivalMessage = string.Format(GetRandomPhrase(arrivalPhrases), waypoints[currentWaypointIndex].name);
            statusText.text = arrivalMessage;
        }

        yield return new WaitForSeconds(waitTimeAtWaypoint);
        GoToNextWaypoint();
        isWaiting = false;
    }

    private string GetRandomPhrase(string[] phrases)
    {
        int index = Random.Range(0, phrases.Length);
        return phrases[index];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, spawnAreaSize);
    }
}
