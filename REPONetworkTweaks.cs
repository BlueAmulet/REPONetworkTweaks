using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

namespace REPONetworkTweaks
{
	[BepInPlugin(ID, Name, Version)]
	public class REPONetworkTweaks : BaseUnityPlugin
	{
		internal const string Name = "REPONetworkTweaks";
		internal const string Author = "BlueAmulet";
		internal const string ID = Author + "." + Name;
		internal const string Version = "1.0.3";

		internal static ManualLogSource Log;

		public void Awake()
		{
			Log = Logger;
			Settings.InitConfig(Config);
			Harmony harmony = new Harmony(ID);
			Logger.LogInfo("Applying Harmony patches");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			int patchedMethods = 0;
			foreach (MethodBase method in harmony.GetPatchedMethods())
			{
				//Logger.LogInfo("Patched " + method.DeclaringType.Name + "." + method.Name);
				patchedMethods++;
			}
			Logger.LogInfo(patchedMethods + " patches applied");
		}
	}
}
