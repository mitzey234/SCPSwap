using Exiled.API.Features;

namespace SCPSwap
{
	public class ScpSwap : Plugin<Config>
	{
		public static ScpSwap Instance;
		public EventHandlers Handler { get; private set; }
		public override string Name => nameof(ScpSwap);
		public override string Author => "Cyanox";

		public ScpSwap() { }

		public override void OnEnabled()
		{
			Instance = this;
			Handler = new EventHandlers(this);
			Exiled.Events.Handlers.Server.WaitingForPlayers += Handler.OnWaitingForPlayers;
			Exiled.Events.Handlers.Server.RoundStarted += Handler.OnRoundStart;
			Exiled.Events.Handlers.Server.RoundEnded += Handler.OnRoundEnd;
			Exiled.Events.Handlers.Server.RestartingRound += Handler.OnRoundRestart;
		}

		public override void OnDisabled()
		{
			Exiled.Events.Handlers.Server.WaitingForPlayers -= Handler.OnWaitingForPlayers;
			Exiled.Events.Handlers.Server.RoundStarted -= Handler.OnRoundStart;
			Exiled.Events.Handlers.Server.RoundEnded -= Handler.OnRoundEnd;
			Exiled.Events.Handlers.Server.RestartingRound -= Handler.OnRoundRestart;
			Handler = null;
		}

		public override void OnReloaded() { }
	}
}
