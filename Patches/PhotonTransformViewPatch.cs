using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace REPONetworkTweaks.Patches
{
	[HarmonyPatch(typeof(PhotonTransformView))]
	internal static class PhotonTransformViewPatch
	{
		// Lobotomize PhotonTransformView and redirect to our version
		[HarmonyPostfix]
		[HarmonyPatch(nameof(PhotonTransformView.Awake))]
		public static void PostfixAwake(ref PhotonTransformView __instance)
		{
			__instance.gameObject.AddComponent<PhotonTransformViewTweak>();
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(PhotonTransformView.OnPhotonSerializeView))]
		public static bool PrefixOnPhotonSerializeView(ref PhotonTransformView __instance, PhotonStream stream, PhotonMessageInfo info)
		{
			__instance.GetComponent<PhotonTransformViewTweak>().OnPhotonSerializeView(stream, info);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(PhotonTransformView.Teleport))]
		public static bool PrefixTeleport(ref PhotonTransformView __instance, Vector3 _position, Quaternion _rotation)
		{
			__instance.GetComponent<PhotonTransformViewTweak>().Teleport(_position, _rotation);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(PhotonTransformView.Update))]
		public static bool PrefixUpdate()
		{
			return false;
		}
	}
}
