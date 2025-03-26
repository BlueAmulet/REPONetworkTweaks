using BepInEx.Configuration;

namespace REPONetworkTweaks
{
	internal static class Settings
	{
		internal static ConfigEntry<bool> DisableTimeout;
		internal static ConfigEntry<bool> PhotonLateUpdate;
		internal static ConfigEntry<bool> Extrapolate;
		internal static ConfigEntry<float> RateSmoothing;
		internal static ConfigEntry<float> Future;
		internal static ConfigEntry<float> TimingThreshold;

		public static void InitConfig(ConfigFile config)
		{
			DisableTimeout = config.Bind("NetworkTweaks", "DisableTimeout", true, "Remove client sided Photon timeout");
			PhotonLateUpdate = config.Bind("NetworkTweaks", "PhotonLateUpdate", true, "Run Photon networking in LateUpdate instead of FixedUpdate");
			Extrapolate = config.Bind("NetworkTweaks", "Extrapolate", true, "Extrapolate position and rotations when out of data");
			RateSmoothing = config.Bind("NetworkTweaks", "RateSmoothing", 0.1f, new ConfigDescription("How much to smooth the guessed sending rate", new AcceptableValueRange<float>(0f, 1f)));
			Future = config.Bind("NetworkTweaks", "Future", 1f, new ConfigDescription("How much to project data into the future", new AcceptableValueRange<float>(0f, 10f)));
			TimingThreshold = config.Bind("NetworkTweaks", "TimingThreshold", 1f, "Threshold in seconds for when data is considered discontinuous");
		}
	}
}
