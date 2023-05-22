using Microsoft.AspNetCore.SignalR;

namespace iRStandings.Hubs
{
	public class StandingsHub : Hub<StandingsHub.IStandingsHubClient>
	{
		public async Task SendMessage(IEnumerable<StandingsRow> rows)
		{
			await Clients.All.UpdateStandings(rows);
		}

		public interface IStandingsHubClient
		{
			Task UpdateStandings(IEnumerable<StandingsRow> rows);
		}
	}
}
