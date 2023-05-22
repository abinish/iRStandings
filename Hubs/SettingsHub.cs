using Microsoft.AspNetCore.SignalR;

namespace iRStandings.Hubs
{
	public class SettingsHub : Hub<SettingsHub.ISettingsHubClient>
	{

		private StandingsHelper _standingsHelper { get; set; }
		public SettingsHub(StandingsHelper standingsHelper)
		{
            _standingsHelper = standingsHelper;
		}
		public async Task SendMessage(Settings settings)
		{
			await Clients.All.UpdateSettings(settings);
		}

		public async Task SaveSettings(Settings settings)
		{
			_standingsHelper.SaveSettings(settings);
			await Task.CompletedTask;
        }

		public interface ISettingsHubClient
		{
			Task UpdateSettings(Settings settings);
		}
	}
}
