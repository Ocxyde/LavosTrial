// Copyright (C) 2026 Ocxyde
// GPL-3.0 license - see COPYING
// WallFacade.cs - Wall segment with binary sides and edges for proper connection

using UnityEngine;

namespace Code.Lavos.Core
{
    /// <summary>
    /// WallFacade - Wall segment with binary sides and edges.
    /// Connection Rules:
    /// - Side 0 = Inward (inside face), Side 1 = Outward
    /// - Edge 0 = Start, Edge 1 = End
    /// - 1 can connect to 0 (compatible)
    /// - 0 cannot connect to 0 (incompatible)
    /// - 1 cannot connect to 1 (incompatible)
    /// </summary>
    public class WallFacade : MonoBehaviour
    {
        #region Binary Side/Edge System

        /// <summary>
        /// Binary side: 0 = Inward, 1 = Outward
        /// Inward side (0) always faces the corridor interior
        /// </summary>
        public enum WallSide : byte
        {
            Inward = 0,
            Outward = 1
        }

        /// <summary>
        /// Binary edge: 0 = Start, 1 = End
        /// Edge 0 is the start of the wall segment
        /// Edge 1 is the end of the wall segment
        /// </summary>
        public enum WallEdge : byte
        {
            Start = 0,
            End = 1
        }

        #endregion

        #region Inspector Fields

        [Header("Wall Identification")]
        [Tooltip("Unique identifier for this wall segment")]
        [SerializeField] private string wallId;

        [Tooltip("Inward side marker (Side 0) - faces corridor")]
        [SerializeField] private Transform inwardMarker;

        [Tooltip("Outward side marker (Side 1) - faces exterior")]
        [SerializeField] private Transform outwardMarker;

        [Tooltip("Edge 0 marker (start of wall)")]
        [SerializeField] private Transform edge0Marker;

        [Tooltip("Edge 1 marker (end of wall)")]
        [SerializeField] private Transform edge1Marker;

        [Header("Wall Dimensions")]
        [SerializeField] private float length = 6f;
        [SerializeField] private float height = 3f;
        [SerializeField] private float thickness = 0.3f;

        [Header("Connection State")]
        [Tooltip("Is Edge 0 (start) connected to another wall?")]
        [SerializeField] private bool edge0Connected = false;

        [Tooltip("Is Edge 1 (end) connected to another wall?")]
        [SerializeField] private bool edge1Connected = false;

        #endregion

        #region Public Accessors

        public string WallId => wallId;
        public float Length => length;
        public float Height => height;
        public float Thickness => thickness;
        public bool IsEdge0Connected => edge0Connected;
        public bool IsEdge1Connected => edge1Connected;

        #endregion

        #region Connection Compatibility Rules

        /// <summary>
        /// Check if two edges are compatible for connection.
        /// Rule: 1 can connect to 0, but 0 cannot connect to 0, and 1 cannot connect to 1.
        /// </summary>
        public static bool AreEdgesCompatible(WallEdge myEdge, WallEdge otherEdge)
        {
            // Convert to byte values (0 or 1)
            byte myValue = (byte)myEdge;
            byte otherValue = (byte)otherEdge;

            // Compatible: 0-1 or 1-0 (different values)
            // Incompatible: 0-0 or 1-1 (same values)
            return myValue != otherValue;
        }

        /// <summary>
        /// Check if this wall's edge can connect to another wall's edge.
        /// </summary>
        public bool CanConnectTo(WallEdge myEdge, WallFacade otherWall, WallEdge otherEdge)
        {
            if (otherWall == null) return false;

            // Check if my edge is already connected
            bool myEdgeIsConnected = (myEdge == WallEdge.Start) ? edge0Connected : edge1Connected;
            if (myEdgeIsConnected) return false;

            // Check if other edge is already connected
            bool otherEdgeIsConnected = (otherEdge == WallEdge.Start) ? otherWall.edge0Connected : otherWall.edge1Connected;
            if (otherEdgeIsConnected) return false;

            // Check compatibility (0-1 or 1-0 only)
            return AreEdgesCompatible(myEdge, otherEdge);
        }

        #endregion

        #region Side Methods

        /// <summary>
        /// Get the transform for a specific side (Inward=0 or Outward=1)
        /// </summary>
        public Transform GetSide(WallSide side)
        {
            return side switch
            {
                WallSide.Inward => inwardMarker,
                WallSide.Outward => outwardMarker,
                _ => null
            };
        }

        /// <summary>
        /// Get the world position of a specific side
        /// </summary>
        public Vector3 GetSidePosition(WallSide side)
        {
            Transform sideTransform = GetSide(side);
            return sideTransform != null ? sideTransform.position : transform.position;
        }

        /// <summary>
        /// Get the forward direction of a specific side (which way it faces)
        /// </summary>
        public Vector3 GetSideDirection(WallSide side)
        {
            Transform sideTransform = GetSide(side);
            return sideTransform != null ? sideTransform.forward : transform.forward;
        }

        #endregion

        #region Edge Methods

        /// <summary>
        /// Get the transform for a specific edge (Start=0 or End=1)
        /// </summary>
        public Transform GetEdge(WallEdge edge)
        {
            return edge switch
            {
                WallEdge.Start => edge0Marker,
                WallEdge.End => edge1Marker,
                _ => null
            };
        }

        /// <summary>
        /// Get the world position of a specific edge
        /// </summary>
        public Vector3 GetEdgePosition(WallEdge edge)
        {
            Transform edgeTransform = GetEdge(edge);
            return edgeTransform != null ? edgeTransform.position : transform.position;
        }

        /// <summary>
        /// Connect this wall's edge to another wall's edge.
        /// Only works if edges are compatible (0-1 or 1-0).
        /// Positions this wall so that the specified edges meet.
        /// </summary>
        public bool ConnectEdgeToEdge(WallFacade otherWall, WallEdge myEdge, WallEdge otherEdge)
        {
            if (otherWall == null)
            {
                Debug.LogError("[WallFacade] Cannot connect to null wall!");
                return false;
            }

            // Check if connection is allowed
            if (!CanConnectTo(myEdge, otherWall, otherEdge))
            {
                Debug.LogWarning($"[WallFacade] Cannot connect Edge {(byte)myEdge} to Edge {(byte)otherEdge} - incompatible!");
                return false;
            }

            // Get target edge position from other wall
            Vector3 targetPosition = otherWall.GetEdgePosition(otherEdge);

            // Get my edge local position
            Vector3 myEdgeLocalPos = GetEdgeLocalPosition(myEdge);

            // Calculate offset needed to align edges
            Vector3 offset = targetPosition - transform.TransformPoint(myEdgeLocalPos);

            // Move this wall to connect edges
            transform.position += offset;

            // Mark edges as connected
            if (myEdge == WallEdge.Start) edge0Connected = true;
            else edge1Connected = true;

            if (otherEdge == WallEdge.Start) otherWall.edge0Connected = true;
            else otherWall.edge1Connected = true;

            Debug.Log($"[WallFacade]  Connected Edge {(byte)myEdge} to Edge {(byte)otherEdge}");
            return true;
        }

        /// <summary>
        /// Get the local position of an edge relative to wall center.
        /// Edge 0 (Start) is at -length/2, Edge 1 (End) is at +length/2 (along local X axis)
        /// </summary>
        private Vector3 GetEdgeLocalPosition(WallEdge edge)
        {
            float xPos = (edge == WallEdge.Start) ? -length / 2f : length / 2f;
            return new Vector3(xPos, height / 2f, 0f);
        }

        #endregion

        #region Connection Helpers

        /// <summary>
        /// Connect this wall's Edge 1 (End) to another wall's Edge 0 (Start).
        /// This is the standard connection pattern (10).
        /// </summary>
        public bool ConnectEndToStart(WallFacade otherWall)
        {
            return ConnectEdgeToEdge(otherWall, WallEdge.End, WallEdge.Start);
        }

        /// <summary>
        /// Connect this wall's Edge 0 (Start) to another wall's Edge 1 (End).
        /// This is the reverse connection pattern (01).
        /// </summary>
        public bool ConnectStartToEnd(WallFacade otherWall)
        {
            return ConnectEdgeToEdge(otherWall, WallEdge.Start, WallEdge.End);
        }

        /// <summary>
        /// Reset connection state (mark all edges as unconnected).
        /// </summary>
        public void ResetConnections()
        {
            edge0Connected = false;
            edge1Connected = false;
        }

        #endregion

        #region Editor Helpers

#if UNITY_EDITOR
        [ContextMenu("Auto-Setup Markers")]
        private void AutoSetupMarkers()
        {
            // Create markers if they don't exist
            if (inwardMarker == null)
                inwardMarker = CreateMarker("InwardMarker_S0", Vector3.forward);

            if (outwardMarker == null)
                outwardMarker = CreateMarker("OutwardMarker_S1", -Vector3.forward);

            if (edge0Marker == null)
                edge0Marker = CreateMarker("Edge0_Start", new Vector3(-length / 2f, 0f, 0f));

            if (edge1Marker == null)
                edge1Marker = CreateMarker("Edge1_End", new Vector3(length / 2f, 0f, 0f));

            Debug.Log($"[WallFacade] Auto-setup complete for {gameObject.name}");
            Debug.Log($"[WallFacade] Connection Rules: 0-1 | 0-0 | 1-1");
        }

        private Transform CreateMarker(string name, Vector3 offset)
        {
            GameObject marker = new GameObject(name);
            marker.transform.SetParent(transform, false);
            marker.transform.localPosition = offset;
            return marker.transform;
        }
#endif

        #endregion

        #region Debug Visualization

        private void OnDrawGizmos()
        {
            // Draw wall bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(length, height, thickness));

            // Draw side markers
            if (inwardMarker != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(inwardMarker.position, 0.1f);
                Gizmos.DrawLine(inwardMarker.position, inwardMarker.position + inwardMarker.forward * 0.5f);
                // Label: "S0"
            }

            if (outwardMarker != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(outwardMarker.position, 0.1f);
                Gizmos.DrawLine(outwardMarker.position, outwardMarker.position + outwardMarker.forward * 0.5f);
                // Label: "S1"
            }

            // Draw edge markers with connection state
            if (edge0Marker != null)
            {
                Gizmos.color = edge0Connected ? Color.gray : Color.blue;
                Gizmos.DrawSphere(edge0Marker.position, 0.15f);
                // Label: "E0" + (connected ? "" : "")
            }

            if (edge1Marker != null)
            {
                Gizmos.color = edge1Connected ? Color.gray : Color.magenta;
                Gizmos.DrawSphere(edge1Marker.position, 0.15f);
                // Label: "E1" + (connected ? "" : "")
            }
        }

        #endregion
    }
}
