namespace iRStandings
{
	public class Driver
	{
		public string Name { get; set; }

		public int? CarIdx { get; set; }

		public int StartingPoints { get; set; }

		public int LivePoints { get; set; }

		public int StartingRank { get; set; }

		public List<int> RacePoints { get; set; }

		public int CurrentRacePoints { get; set; }

		public string BonusPointReasons { get; set; }

	}
}