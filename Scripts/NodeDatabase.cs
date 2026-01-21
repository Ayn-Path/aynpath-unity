using System.Collections.Generic;
using UnityEngine;

public class NodeDatabase : MonoBehaviour
{
    private Node[] nodes;

    // Fast lookup
    private readonly Dictionary<string, Node> idMap = new();
    private readonly Dictionary<string, Node> nameMap = new();

    // -------------------------
    // Unity lifecycle
    // -------------------------

    void Start()
    {
        // Ensure all Node.Awake() have run
        Refresh();
    }

    // -------------------------
    // Public API
    // -------------------------

    public void Refresh()
    {
#if UNITY_2023_1_OR_NEWER
        nodes = FindObjectsByType<Node>(FindObjectsSortMode.None);
#else
        nodes = FindObjectsOfType<Node>();
#endif
        BuildDictionaries();
    }

    // -------------------------
    // Internal helpers
    // -------------------------

    private void BuildDictionaries()
    {
        idMap.Clear();
        nameMap.Clear();

        if (nodes == null)
            return;

        foreach (var n in nodes)
        {
            if (!n)
                continue;

            if (!string.IsNullOrWhiteSpace(n.nodeId))
            {
                string idKey = n.nodeId.ToLowerInvariant().Trim();
                if (idMap.ContainsKey(idKey))
                {
                    Debug.LogWarning(
                        $"[NodeDatabase] Duplicate nodeId '{n.nodeId}'. Ignoring '{n.name}'.");
                }
                else
                {
                    idMap[idKey] = n;
                }
            }

            if (!string.IsNullOrWhiteSpace(n.displayName))
            {
                string nameKey = n.displayName.ToLowerInvariant().Trim();
                if (nameMap.ContainsKey(nameKey))
                {
                    Debug.LogWarning(
                        $"[NodeDatabase] Duplicate displayName '{n.displayName}'. Ignoring '{n.name}'.");
                }
                else
                {
                    nameMap[nameKey] = n;
                }
            }
        }

        Debug.Log($"[NodeDatabase] Loaded {nodes.Length} nodes.");
    }

    // -------------------------
    // Lookup methods
    // -------------------------

    public Node FindNodeById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        idMap.TryGetValue(id.ToLowerInvariant().Trim(), out var n);
        return n;
    }

    public Node FindByNameOrId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        string key = value.ToLowerInvariant().Trim();

        if (idMap.TryGetValue(key, out var idMatch))
            return idMatch;

        if (nameMap.TryGetValue(key, out var nameMatch))
            return nameMatch;

        return null;
    }

    public Node NearestNode(Vector3 pos)
    {
        if (nodes == null || nodes.Length == 0)
            Refresh();

        Node best = null;
        float bestDist = float.MaxValue;

        foreach (var n in nodes)
        {
            if (!n)
                continue;

            float d = Vector3.SqrMagnitude(pos - n.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = n;
            }
        }

        return best;
    }
}
