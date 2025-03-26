using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable RCS1146 // Use conditional access

namespace REPONetworkTweaks
{
	public class PhotonTransformViewTweak : MonoBehaviour
	{
		private Rigidbody rigidBody;
		private PhotonTransformView photonTransformView;
		private PhysGrabHinge hinge;

		// Owner variables
		private Vector3 m_StoredPosition;
		private bool teleport;

		// Receiver variables
		private bool isSleeping;
		private bool isKinematic;
		private Vector3 interpVelocity;
		private Vector3 interpAngularVelocity;

		private readonly LinkedList<TransformSnapshot> snapshots = new LinkedList<TransformSnapshot>();
		private TransformSnapshot prevSnapshot; // Used for extrapolation

		private float interpolationStartTime = -1f;
		private float interpFactor => (smoothUpdateFrequency <= 0 || interpolationStartTime < 0) ? 1 : (Time.timeSinceLevelLoad - interpolationStartTime) / smoothUpdateFrequency;

		// Dynamic sending rate
		private float smoothUpdateFrequency = PhotonNetwork.serializationFrequency / 1000f;
		private float lastSerializationFrequency = -1f;
		private int lastSendTimestamp = 0;
		private bool haveFirstSend = false;

		public void Awake()
		{
			rigidBody = GetComponent<Rigidbody>();
			photonTransformView = GetComponent<PhotonTransformView>();
			hinge = GetComponent<PhysGrabHinge>();
			m_StoredPosition = transform.position; // Changed from localPosition
		}

		public void OnEnable()
		{
			snapshots.Clear();
			prevSnapshot = null;
		}

		internal void Teleport(Vector3 _position, Quaternion _rotation)
		{
			teleport = true;
			transform.position = _position;
			transform.rotation = _rotation;
			rigidBody.position = _position;
			rigidBody.rotation = _rotation;
			m_StoredPosition = _position; // Added due to m_Direction
			isSleeping = false;
			rigidBody.WakeUp();
		}

		private Vector3 HermiteInterpolatePosition(Vector3 startPos, Vector3 startVel, Vector3 endPos, Vector3 endVel, float interpolation)
		{
			Vector3 posControl1 = startPos + (startVel * (smoothUpdateFrequency * interpolation) / 3f);
			Vector3 posControl2 = endPos - (endVel * (smoothUpdateFrequency * (1.0f - interpolation)) / 3f);
			return Vector3.Lerp(posControl1, posControl2, interpolation);
		}

		private Quaternion HermiteInterpolateRotation(Quaternion startRot, Vector3 startSpin, Quaternion endRot, Vector3 endSpin, float interpolation)
		{
			Quaternion rotControl1 = startRot * Quaternion.Euler(startSpin * (smoothUpdateFrequency * interpolation) / 3f);
			Quaternion rotControl2 = endRot * Quaternion.Euler(endSpin * (-1.0f * smoothUpdateFrequency * (1.0f - interpolation)) / 3f);
			return Quaternion.Slerp(rotControl1, rotControl2, interpolation);
		}

		public void Update()
		{
			Transform transform = base.transform;
			if (!PhotonNetwork.IsMasterClient)
			{
				// Unity's NetworkRigidbody forces kinematic and no interpolation on non owner
				// This makes the movement smooth
				rigidBody.isKinematic = true;
				rigidBody.interpolation = RigidbodyInterpolation.None;

				if (snapshots.Count >= 1)
				{
					// Advance to next snapshot if we've finished interpolating
					while (interpFactor >= 1 && snapshots.Count > 1)
					{
						prevSnapshot = snapshots.First.Value;
						snapshots.RemoveFirst();
						interpolationStartTime += smoothUpdateFrequency;
					}
					if (snapshots.Count == 1)
					{
						// First, ran out of data, or teleport, just apply
						TransformSnapshot snapshot = snapshots.First.Value;
						if (prevSnapshot != null && Settings.Extrapolate.Value)
						{
							// Clamp interpolation factor to prevent drift
							//float clampInterp = Mathf.Min(interpFactor + 1, 2f);
							float clampInterp = 2f - Mathf.Pow((float)Math.E, -interpFactor);
							//transform.position = Vector3.LerpUnclamped(prevSnapshot.position, snapshot.position, clampInterp);
							transform.position = snapshot.position + (Vector3.SlerpUnclamped(prevSnapshot.velocity, snapshot.velocity, clampInterp) * ((clampInterp - 1f) * smoothUpdateFrequency));
							transform.rotation = Quaternion.SlerpUnclamped(prevSnapshot.rotation, snapshot.rotation, clampInterp);
						}
						else
						{
							transform.position = snapshot.position;
							transform.rotation = snapshot.rotation;
						}
						interpVelocity = snapshot.velocity;
						interpAngularVelocity = snapshot.angularVelocity;
					}
					else
					{
						// Interpolate between first and next
						LinkedListNode<TransformSnapshot> node = snapshots.First;
						TransformSnapshot now = node.Value;
						TransformSnapshot next = node.Next.Value;
						transform.position = HermiteInterpolatePosition(now.position, now.velocity, next.position, next.velocity, interpFactor);
						transform.rotation = HermiteInterpolateRotation(now.rotation, now.angularVelocity, next.rotation, next.angularVelocity, interpFactor);
						interpVelocity = Vector3.Slerp(now.velocity, next.velocity, interpFactor);
						interpAngularVelocity = Vector3.Lerp(now.angularVelocity, next.angularVelocity, interpFactor);
					}
				}
			}
		}

		internal void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
		{
			Transform transform = base.transform;
			if (stream.IsWriting)
			{
				stream.SendNext(rigidBody.IsSleeping());
				stream.SendNext(teleport);
				teleport = false;
				isKinematic = rigidBody.isKinematic;
				stream.SendNext(isKinematic);
				stream.SendNext(rigidBody.velocity);
				stream.SendNext(rigidBody.angularVelocity);
				Vector3 m_Direction = transform.position - m_StoredPosition;
				m_StoredPosition = transform.position;
				stream.SendNext(transform.position);
				stream.SendNext(m_Direction);
				stream.SendNext(transform.rotation);
			}
			else
			{
				if (!rigidBody)
				{
					rigidBody = GetComponent<Rigidbody>();
				}
				isSleeping = (bool)stream.ReceiveNext();
				if (isSleeping)
				{
					rigidBody.Sleep();
				}
				else
				{
					rigidBody.WakeUp();
				}
				teleport = (bool)stream.ReceiveNext();
				isKinematic = (bool)stream.ReceiveNext();
				Vector3 receivedVelocity = (Vector3)stream.ReceiveNext();
				Vector3 receivedAngularVelocity = (Vector3)stream.ReceiveNext();
				Vector3 receivedPosition = (Vector3)stream.ReceiveNext();
				Vector3 m_Direction = (Vector3)stream.ReceiveNext();
				Quaternion receivedRotation = (Quaternion)stream.ReceiveNext();

				// Velocity data on rigidbodies with hinges is just incorrect
				if (hinge != null && !hinge.broken)
				{
					if (haveFirstSend)
					{
						receivedVelocity = m_Direction / (info.SentServerTimestamp - lastSendTimestamp) * 1000f;
					}
					else
					{
						receivedVelocity = m_Direction / smoothUpdateFrequency;
					}
				}

				if (teleport)
				{
					snapshots.Clear();
					prevSnapshot = null;
				}
				float future = Mathf.Max(Settings.Future.Value * smoothUpdateFrequency, 0f);
				TransformSnapshot currentState = new TransformSnapshot(receivedPosition, receivedRotation, receivedVelocity, receivedAngularVelocity);
				if (snapshots.Count > 0 && (info.SentServerTimestamp - lastSendTimestamp) / 1000f < Settings.TimingThreshold.Value)
				{
					TransformSnapshot previousState = snapshots.Last.Value;
					// Advance position forward in time
					Vector3 acceleration = (currentState.velocity - previousState.velocity) / smoothUpdateFrequency;
					currentState.position += (currentState.velocity * future) + (0.5f * acceleration * future * future);
					// Advance rotation forward in time
					Vector3 angularAcceleration = (currentState.angularVelocity - previousState.angularVelocity) / smoothUpdateFrequency;
					Vector3 futureAngularVelocity = currentState.angularVelocity + (angularAcceleration * future);
					Quaternion deltaRotation = Quaternion.Euler(futureAngularVelocity * future);
					currentState.rotation *= deltaRotation;
				}
				else
				{
					// No previous data or previous data is too old
					currentState.position += currentState.velocity * future;
					currentState.rotation *= Quaternion.Euler(currentState.angularVelocity * future);
				}
				snapshots.AddLast(currentState);
				if (snapshots.Count == 2)
				{
					// Have enough to start interpolating now
					interpolationStartTime = Time.timeSinceLevelLoad;
				}
				else if (snapshots.Count >= 4)
				{
					// Receiving too much data, discard current and restart interpolation
					//REPONetworkTweaks.Log.LogWarning($"[{name}] Too many snapshots, discarding current");
					snapshots.RemoveFirst();
					TransformSnapshot snapshot = snapshots.First.Value;
					// Modify snapshot to start from current transform to hide discontinuity
					snapshot.position = transform.position;
					snapshot.rotation = transform.rotation;
					snapshot.velocity = interpVelocity;
					snapshot.angularVelocity = interpAngularVelocity;
					interpolationStartTime = Time.timeSinceLevelLoad;
				}

				// Debugging
				if (haveFirstSend)
				{
					// Send timestamps are more reliable than Recv
					lastSerializationFrequency = (info.SentServerTimestamp - lastSendTimestamp) / 1000f;
					if (lastSerializationFrequency >= Settings.TimingThreshold.Value)
					{
						// Assuming the body stopped moving, reset frequencies
						lastSerializationFrequency = PhotonNetwork.serializationFrequency / 1000f;
						smoothUpdateFrequency = lastSerializationFrequency;
					}
					else
					{
						smoothUpdateFrequency = Mathf.Lerp(smoothUpdateFrequency, lastSerializationFrequency, Settings.RateSmoothing.Value);
					}
				}
				else
				{
					haveFirstSend = true;
				}
				lastSendTimestamp = info.SentServerTimestamp;
			}
		}

		private class TransformSnapshot
		{
			internal Vector3 position;
			internal Quaternion rotation;
			internal Vector3 velocity;
			internal Vector3 angularVelocity;

			internal TransformSnapshot(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
			{
				this.position = position;
				this.rotation = rotation;
				this.velocity = velocity;
				this.angularVelocity = angularVelocity;
			}
		}
	}
}
