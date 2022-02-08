using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.Loader;
using MEC;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SCPSwap
{
	public sealed class EventHandlers
	{
		internal Dictionary<Player, Player> ongoingReqs = new Dictionary<Player, Player>();

		internal List<CoroutineHandle> coroutines = new List<CoroutineHandle>();
		internal Dictionary<Player, CoroutineHandle> reqCoroutines = new Dictionary<Player, CoroutineHandle>();

		internal bool allowSwaps = false;
		internal bool isRoundStarted = false;

		internal Dictionary<string, RoleType> valid = new Dictionary<string, RoleType>()
		{
			{"173", RoleType.Scp173},
			{"peanut", RoleType.Scp173},
			{"939", RoleType.Scp93953},
			{"dog", RoleType.Scp93953},
			{"079", RoleType.Scp079},
			{"computer", RoleType.Scp079},
			{"106", RoleType.Scp106},
			{"larry", RoleType.Scp106},
			{"096", RoleType.Scp096},
			{"shyguy", RoleType.Scp096},
			{"049", RoleType.Scp049},
			{"doctor", RoleType.Scp049},
			{"0492", RoleType.Scp0492},
			{"zombie", RoleType.Scp0492},
			{"966", RoleType.None }
		};

		public ScpSwap plugin;

		public EventHandlers(ScpSwap plugin) => this.plugin = plugin;

		internal IEnumerator<float> SendRequest(Player source, Player dest)
		{
			ongoingReqs.Add(source, dest);
			dest.Broadcast(5, "<i>You have an SCP Swap request!\nCheck your console by pressing [`] or [~]</i>");
			dest.ReferenceHub.characterClassManager.TargetConsolePrint(dest.ReferenceHub.scp079PlayerScript.connectionToClient, $"You have received a swap request from {source.ReferenceHub.nicknameSync.Network_myNickSync} who is SCP-{valid.FirstOrDefault(x => x.Value == source.Role).Key}. Would you like to swap with them? Type \".scpswap yes\" to accept or \".scpswap no\" to decline.", "yellow");
			yield return Timing.WaitForSeconds(plugin.Config.SwapRequestTimeout);
			TimeoutRequest(source);
		}

		internal void TimeoutRequest(Player source)
		{
			if (ongoingReqs.ContainsKey(source))
			{
				Player dest = ongoingReqs[source];
				source.ReferenceHub.characterClassManager.TargetConsolePrint(source.ReferenceHub.scp079PlayerScript.connectionToClient, "The player did not respond to your request.", "red");
				dest.ReferenceHub.characterClassManager.TargetConsolePrint(dest.ReferenceHub.scp079PlayerScript.connectionToClient, "Your swap request has timed out.", "red");
				ongoingReqs.Remove(source);
			}
		}

		internal void PerformSwap(Player source, Player dest)
		{
			bool source966 = source.SessionVariables.ContainsKey("is966") && (bool)source.SessionVariables["is966"];
			bool dest966 = dest.SessionVariables.ContainsKey("is966") && (bool)dest.SessionVariables["is966"];
			source.ReferenceHub.characterClassManager.TargetConsolePrint(source.ReferenceHub.scp079PlayerScript.connectionToClient, "Swap successful!", "green");

			RoleType sRole = source.Role;
			RoleType dRole = dest.Role;

			Vector3 sPos = source.Position;
			Vector3 dPos = dest.Position;

			float sHealth = source.Health;
			float dHealth = dest.Health;

			if (dest966) Swap966(source);
			else
			{
				source.Role = dRole;
				source.Position = dPos;
				source.Health = dHealth;
			}

			if (source966) Swap966(dest);
			else
			{
				dest.Role = sRole;
				dest.Position = sPos;
				dest.Health = sHealth;
			}

			ongoingReqs.Remove(source);
		}

		public void OnRoundStart()
		{
			allowSwaps = true;
			isRoundStarted = true;
			Timing.CallDelayed(plugin.Config.SwapTimeout, () => allowSwaps = false);
		}

		public void OnRoundRestart()
		{
			// fail safe
			isRoundStarted = false;
			Timing.KillCoroutines(coroutines.ToArray());
			Timing.KillCoroutines(reqCoroutines.Values.ToArray());
			coroutines.Clear();
			reqCoroutines.Clear();
		}

		public void OnRoundEnd(RoundEndedEventArgs ev)
		{
			isRoundStarted = false;
			Timing.KillCoroutines(coroutines.ToArray());
			Timing.KillCoroutines(reqCoroutines.Values.ToArray());
			coroutines.Clear();
			reqCoroutines.Clear();
		}

		public void OnWaitingForPlayers()
		{
			allowSwaps = false;
		}

		public void Swap966(Player newPlayer)
		{
			Assembly assembly = Loader.Plugins.First(pl => pl.Name == "scp966")?.Assembly;
			if (assembly == null) return;
			assembly.GetType("scp966.API.Scp966API")?.GetMethod("Swap", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new[] { newPlayer });
		}
	}
}
