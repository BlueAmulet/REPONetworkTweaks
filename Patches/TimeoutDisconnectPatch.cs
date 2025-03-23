using ExitGames.Client.Photon;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace REPONetworkTweaks.Patches
{
	[HarmonyPatch]
	internal static class TimeoutDisconnectPatch
	{
		private static readonly MethodInfo debugOut = AccessTools.PropertyGetter(typeof(PeerBase), nameof(PeerBase.debugOut));
		private static readonly MethodInfo EnqueueStatusCallback = AccessTools.Method(typeof(PeerBase), nameof(PeerBase.EnqueueStatusCallback));

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(EnetPeer), nameof(EnetPeer.SendOutgoingCommands))]
		internal static IEnumerable<CodeInstruction> TranspilerEnetPeer(IEnumerable<CodeInstruction> instructions)
		{
			if (!Settings.DisableTimeout.Value)
			{
				return instructions;
			}
			List<CodeInstruction> instrs = new List<CodeInstruction>(instructions);
			bool patched = false;
			for (int i = 1; i < instrs.Count; i++)
			{
				CodeInstruction instr = instrs[i];
				if (instr.opcode == OpCodes.Call && (MethodInfo)instr.operand == debugOut && instrs[i+1].opcode == OpCodes.Ldc_I4_2)
				{
					if (instrs[i - 3].opcode == OpCodes.Brfalse)
					{
						instrs.Insert(i - 2, new CodeInstruction(OpCodes.Br, instrs[i - 3].operand));
						instrs[i - 3].opcode = OpCodes.Pop;
						instrs[i - 3].operand = null;
						patched = true;
						break;
					}
				}
			}
			if (patched)
			{
				REPONetworkTweaks.Log.LogInfo("Removed TimeoutDisconnect from EnetPeer");
			}
			else
			{
				REPONetworkTweaks.Log.LogWarning("Failed to locate patch to remove EnetPeer triggering TimeoutDisconnect");
			}
			return instrs;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(TPeer), nameof(TPeer.DispatchIncomingCommands))]
		internal static IEnumerable<CodeInstruction> TranspilerTPeer(IEnumerable<CodeInstruction> instructions)
		{
			if (!Settings.DisableTimeout.Value)
			{
				return instructions;
			}
			List<CodeInstruction> instrs = new List<CodeInstruction>(instructions);
			bool patched = false;
			for (int i = 1; i < instrs.Count; i++)
			{
				CodeInstruction instr = instrs[i];
				if (instr.opcode == OpCodes.Ldc_I4 && (int)instr.operand == (int)StatusCode.TimeoutDisconnect && instrs[i + 1].opcode == OpCodes.Call && (MethodInfo)instrs[i + 1].operand == EnqueueStatusCallback)
				{
					if (instrs[i - 3].opcode == OpCodes.Brfalse)
					{
						instrs.Insert(i - 2, new CodeInstruction(OpCodes.Br, instrs[i - 3].operand));
						instrs[i - 3].opcode = OpCodes.Pop;
						instrs[i - 3].operand = null;
						patched = true;
						break;
					}
				}
			}
			if (patched)
			{
				REPONetworkTweaks.Log.LogInfo("Removed TimeoutDisconnect from TPeer");
			}
			else
			{
				REPONetworkTweaks.Log.LogWarning("Failed to locate patch to remove TPeer triggering TimeoutDisconnect");
			}
			return instrs;
		}
	}
}
