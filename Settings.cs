namespace iRStandings
{
	public class Settings
	{
		public string SimRacerHubSeasonNumber { get; set; }
		public int MaxDriversToShow { get; set; }
		public bool EnablePlayoffMode { get; set; }
		public int CurrentPlayoffDrivers { get; set; }
		public int CutoffDrivers { get; set; }
		public int FontSize { get; set; }
		public int Spacing { get; set; }
		public int DropWeeks { get; set; }
		public int WeeksBeforeDropping { get; set; }
		public bool LeagueRemovesNumberSuffix { get; set; }

		public string Notes { get; set; }


		public IEnumerable<string> ScoringTypes => new List<string>{ "Bonfire", "NTRL", "BPC43Plus1", "BPC43Plus1WinAndIn" };


        public string LeagueScoring { get; set; }
		public int LedLapPoints { get; set; }
		public int LedMostLapsPoints { get; set; }
		public int FastestLapPoints { get; set; }
		public int NoIncidentPoints { get; set; }
		public int PolePoints { get; set; }
	}
}
