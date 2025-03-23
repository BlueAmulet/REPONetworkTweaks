using BepInEx.Configuration;

namespace REPONetworkTweaks
{
	internal static class Settings
	{
		internal static ConfigEntry<bool> DisableTimeout;
		internal static ConfigEntry<bool> Extrapolate;
		internal static ConfigEntry<float> RateSmoothing;
		internal static ConfigEntry<float> Future;

		public static void InitConfig(ConfigFile config)
		{
			DisableTimeout = config.Bind("NetworkTweaks", "DisableTimeout", true, "Remove client sided photon timeout");
			Extrapolate = config.Bind("NetworkTweaks", "Extrapolate", true, "Extrapolate position and rotations when out of data");
			RateSmoothing = config.Bind("NetworkTweaks", "RateSmoothing", 0.1f, "How much to smooth the guessed sending rate");
			Future = config.Bind("NetworkTweaks", "Future", 1f, "How much to project data into the future");
		}
	}
}
