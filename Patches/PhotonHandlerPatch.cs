using HarmonyLib;
using Photon.Pun;

namespace REPONetworkTweaks.Patches
{
	[HarmonyPatch(typeof(PhotonHandler))]
	internal static class PhotonHandlerPatch
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(PhotonHandler.Awake))]
		public static void PrefixAwake()
		{
			// Cannot touch PhotonNetwork early or else it breaks, so do it here in PhotonHandler.Awake instead
			if (Settings.PhotonLateUpdate.Value)
			{
				REPONetworkTweaks.Log.LogInfo("Changing PhotonHandler to run in LateUpdate");
				PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = float.PositiveInfinity;
			}
		}
	}
}
