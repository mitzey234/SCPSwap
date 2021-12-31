using Exiled.API.Features;

namespace SCPSwap
{
	public class ScpSwap : Plugin<Config>
	{
		public static ScpSwap Instance;
		public EventHandlers Handler { get; private set; }
		public override string Name => nameof(ScpSwap);
		public override string Author => "Cyanox";
		private bool state = false;

		public ScpSwap() { }

		public override void OnEnabled()
		{
			if (state) return;

			Instance = this;
			Handler = new EventHandlers(this);
			Exiled.Events.Handlers.Server.WaitingForPlayers += Handler.OnWaitingForPlayers;
			Exiled.Events.Handlers.Server.RoundStarted += Handler.OnRoundStart;
			Exiled.Events.Handlers.Server.RoundEnded += Handler.OnRoundEnd;
			Exiled.Events.Handlers.Server.RestartingRound += Handler.OnRoundRestart;

			state = true;
			base.OnEnabled();
		}

		public override void OnDisabled()
		{
			if (!state) return;

			Exiled.Events.Handlers.Server.WaitingForPlayers -= Handler.OnWaitingForPlayers;
			Exiled.Events.Handlers.Server.RoundStarted -= Handler.OnRoundStart;
			Exiled.Events.Handlers.Server.RoundEnded -= Handler.OnRoundEnd;
			Exiled.Events.Handlers.Server.RestartingRound -= Handler.OnRoundRestart;
			Handler = null;

			state = false;
			base.OnEnabled();
		}

		public override void OnReloaded() { }
	}
}
