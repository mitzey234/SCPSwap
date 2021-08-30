using System;
using System.IO;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using MEC;
using RemoteAdmin;
using SCPSwap;

namespace BrightPlugin_EXILED.Commands
{
	[CommandHandler(typeof(ClientCommandHandler))]
	class ScpSwapCommand : ICommand
	{
		public string[] Aliases { get; set; } = Array.Empty<string>();

		public string Description { get; set; } = "Swap your scp with another player";

		string ICommand.Command { get; } = "scpswap";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			EventHandlers handlers = ScpSwap.Instance.Handler;
			if (sender is PlayerCommandSender playerSender)
			{
				Player player = Player.Get(playerSender);
				if (!handlers.isRoundStarted)
				{
					response = "<color=\"red\">The round hasn't started yet!</color>";
					return true;
				}

				if (player.Team != Team.SCP)
				{
					response = "<color=\"red\">You're not an SCP, why did you think that would work.</color>";
					return true;
				}

				if (player.SessionVariables.ContainsKey("is966") && (bool)player.SessionVariables["is966"])
				{
					response = "<color=\"red\">Due to issues regarding 966, SCP 966 is not allowed to swap.</color>";
					return true;
				}

				if (!handlers.allowSwaps)
				{
					response = "<color=\"red\">SCP swap period has expired.</color>";
					return true;
				}

				switch (arguments.Count)
				{
					case 1:
						switch (arguments.ElementAt(0).ToLower())
						{
							case "yes":
								Player swap = handlers.ongoingReqs.FirstOrDefault(x => x.Value == player).Key;
								if (swap != null)
								{
									handlers.PerformSwap(swap, player);
									response = "<color=\"green\">Swap successful!</color>";
									Timing.KillCoroutines(handlers.reqCoroutines[swap]);
									handlers.reqCoroutines.Remove(swap);
									return true;
								}
								response = "You do not have a swap request.";
								return true;
							case "no":
								swap = handlers.ongoingReqs.FirstOrDefault(x => x.Value == player).Key;
								if (swap != null)
								{
									response = "Swap request denied.";
									swap.ReferenceHub.characterClassManager.TargetConsolePrint(swap.ReferenceHub.scp079PlayerScript.connectionToClient, "Your swap request has been denied.", "red");
									Timing.KillCoroutines(handlers.reqCoroutines[swap]);
									handlers.reqCoroutines.Remove(swap);
									handlers.ongoingReqs.Remove(swap);
									return true;
								}
								response = "You do not have a swap reqest.";
								break;
							case "cancel":
								if (handlers.ongoingReqs.ContainsKey(player))
								{
									Player dest = handlers.ongoingReqs[player];
									dest.ReferenceHub.characterClassManager.TargetConsolePrint(dest.ReferenceHub.scp079PlayerScript.connectionToClient, "Your swap request has been cancelled.", "red");
									Timing.KillCoroutines(handlers.reqCoroutines[player]);
									handlers.reqCoroutines.Remove(player);
									handlers.ongoingReqs.Remove(player);
									response = "You have cancelled your swap request.";
									return true;
								}
								response = "You do not have an outgoing swap request.";
								break;
							default:
								if (!handlers.valid.ContainsKey(arguments.ElementAt(0)))
								{
									response = "<color=\"red\">Invalid SCP.</color>";
									return true;
								}

								if (handlers.ongoingReqs.ContainsKey(player))
								{
									response = "<color=\"red\">You already have a request pending!</color>";
									return true;
								}

								RoleType role = handlers.valid[arguments.ElementAt(0)];
								bool is966 = false;
								if (player.SessionVariables.ContainsKey("is966"))
								{
									is966 = (bool)player.SessionVariables["is966"];
								}
								bool req966 = arguments.ElementAt(0) == "966";
								if (ScpSwap.Instance.Config.SwapBlacklist.Contains((int)role))
								{
									response = "<color=\"red\">That SCP is blacklisted.</color>";
									return true;
								}

								if (player.Role == role || (is966 && req966))
								{
									response = "<color=\"red\">You cannot swap with your own role.</color>";
									return true;
								}

								if (!req966) swap = Player.List.FirstOrDefault(x => role == RoleType.Scp93953 ? x.Role == role || x.Role == RoleType.Scp93989 : x.Role == role);
								else swap = Player.List.FirstOrDefault(x => x.SessionVariables.ContainsKey("is966") && (bool)x.SessionVariables["is966"]);

								if (swap.SessionVariables.ContainsKey("is966") && (bool)swap.SessionVariables["is966"])
								{
									response = "<color=\"red\">Due to issues regarding 966, SCP 966 is not allowed to swap.</color>";
									return true;
								}

								if (swap != null)
								{
									handlers.reqCoroutines.Add(player, Timing.RunCoroutine(handlers.SendRequest(player, swap)));
									response = "<color=\"green\">Swap request sent!</color>";
									return true;
								}
								if (ScpSwap.Instance.Config.SwapAllowNewScps)
								{
									//if (!req966) ev.Player.ReferenceHub.characterClassManager.SetPlayersClass(role, ev.Player.ReferenceHub.gameObject);
									//else Swap966(ev.Player);
									player.ReferenceHub.characterClassManager.SetPlayersClass(role, player.ReferenceHub.gameObject, CharacterClassManager.SpawnReason.ForceClass);
									response = "<color=\"green\">Could not find a player to swap with, you have been made the specified SCP.</color>";
									return true;
								}
								response = "<color=\"red\">No players found to swap with.</color>";
								return true;
						}
						break;
					default:
						response = "<color=\"red\">USAGE: SCPSWAP [SCP NUMBER]</color>";
						return true;
				}
			}
			response = "";
			return true;
		}
	}
}
