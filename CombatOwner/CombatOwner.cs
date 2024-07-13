using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace CombatOwner;

[BepInPlugin(ModGUID, ModName, ModVersion)]
[BepInIncompatibility("org.bepinex.plugins.valheim_plus")]
public class CombatOwner : BaseUnityPlugin
{
	private const string ModName = "Combat Owner";
	private const string ModVersion = "1.0.0";
	private const string ModGUID = "org.bepinex.plugins.combatowner";

	public void Awake()
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		Harmony harmony = new(ModGUID);
		harmony.PatchAll(assembly);
	}

	[HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.SetTarget))]
	private static class SwitchOwnerToTarget
	{
		private static void Postfix(MonsterAI __instance, Character attacker)
		{
			if (attacker is Player player && player != Player.m_localPlayer)
			{
				__instance.m_nview.GetZDO().SetOwner(attacker.GetZDOID().UserID);
				__instance.m_nview.InvokeRPC("CombatOwner Transfer Owner", __instance);
			}
		}
	}

	[HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.Awake))]
	private static class AddRPCs
	{
		private static void Postfix(MonsterAI __instance)
		{
			__instance.m_nview.Register("CombatOwner Transfer Owner", _ => GetOwnership(__instance));
		}

		private static void GetOwnership(MonsterAI creature)
		{
			creature.m_nview.ClaimOwnership();
			creature.SetTarget(Player.m_localPlayer);
		}
	}
}
