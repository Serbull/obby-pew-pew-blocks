using System;
using UnityEngine;

namespace Humanoid_Basics.Core.Helpers
{
	public class WaterPhysics : MonoBehaviour
	{
		public LayerMask waterLayer;
		public bool debug;
		
		public float floatHeight;
		public Vector3 buoyancyCentreOffset;
		public float bounceDamp;

		private Rigidbody rb;
		private Vector3 anchorPoint;
		private Vector3 startPosition;
		private const float HeightFromGroundRaycast = 5f;
		private const float RaycastDownDistance = 0.35f;
		private bool hasDetectableHit;
		private RaycastHit detectableHit;
		
		private void Start()
		{
			rb = GetComponent<Rigidbody>();
		}

		private void FixedUpdate() 
		{
			
			// Cache Transform Position
			var objectPosition = transform.position;
			
			// Store Origin + Height
			startPosition = objectPosition;
			startPosition.y += HeightFromGroundRaycast;
			// startPosition.y += 0.5f;
			
			// Draw Debug Line
			if (debug)
				Debug.DrawLine(startPosition, startPosition + Vector3.down * (RaycastDownDistance +  HeightFromGroundRaycast), Color.red);

			// Calculate Anchor Point
			anchorPoint = objectPosition + transform.TransformDirection(buoyancyCentreOffset);
			
			// Store our hit (if any)
			hasDetectableHit = Physics.Raycast(startPosition, Vector3.down, out detectableHit, RaycastDownDistance + HeightFromGroundRaycast, waterLayer);
			//hasDetectableHit = Physics.SphereCast(startPosition, 0.2f, -transform.up, out _, 3f, waterLayer);
			
			// Only run if we have a hit
			if (!hasDetectableHit) return;
			
			// Get Water Level from Detectable Hit
			var waterLevel = detectableHit.transform.position.y;

			// Calculate Force
			var forceFactor = 1f - (anchorPoint.y - waterLevel) / floatHeight;
			if (!(forceFactor > 0f)) return;
			var force = -Physics.gravity * (forceFactor - GetComponent<Rigidbody>().linearVelocity.y * bounceDamp);
			rb.AddForceAtPosition(force, anchorPoint);
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawSphere(anchorPoint, 0.2f);
		}
	}
}
