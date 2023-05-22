using HtmlAgilityPack;
using irsdkSharp;
using irsdkSharp.Serialization;
using iRStandings.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Reflection.Emit;

namespace iRStandings
{
	public class StandingsHelper
	{
		public StandingsHelper(IOptions<Settings> settings, IHubContext<StandingsHub, StandingsHub.IStandingsHubClient> standingsHubContext, IHubContext<SettingsHub, SettingsHub.ISettingsHubClient> settingsHubContext)
		{
			_settings = settings.Value;
			_standingsHubContext = standingsHubContext;
			_settingsHubContext = settingsHubContext;

        }

		Settings _settings;
		private static IRacingSDK sdk;
		private bool running = false;
		private bool _requiresReload = false;
		private IHubContext<StandingsHub, StandingsHub.IStandingsHubClient> _standingsHubContext;
        private IHubContext<SettingsHub, SettingsHub.ISettingsHubClient> _settingsHubContext;
        private bool carIdInitialized = false;
		private List<Driver> Drivers = new List<Driver>();
		private Dictionary<int, int> BTTRacePoints = new Dictionary<int, int>
	{
		{1, 30 },
		{2, 29 },
		{3, 28 },
		{4, 27 },
		{5, 26 },
		{6, 25 },
		{7, 24 },
		{8, 23 },
		{9, 22 },
		{10, 21 },
		{11, 20 },
		{12, 19 },
		{13, 18 },
		{14, 17 },
		{15, 16 },
		{16, 15 },
		{17, 14 },
		{18, 13 },
		{19, 12 },
		{20, 11 },
		{21, 10 },
		{22, 9 },
		{23, 8 },
		{24, 7 },
		{25, 6 },
		{26, 5 },
		{27, 4 },
		{28, 3 },
		{29, 2 },
		{30, 1 }
	};
		private Dictionary<int, int> NTRLPoints = new Dictionary<int, int>
	{
		{1, 40 },
		{2, 35 },
		{3, 34 },
		{4, 33 },
		{5, 32 },
		{6, 31 },
		{7, 30 },
		{8, 29 },
		{9, 28 },
		{10, 27 },
		{11, 26 },
		{12, 25 },
		{13, 24 },
		{14, 23 },
		{15, 22 },
		{16, 21 },
		{17, 20 },
		{18, 19 },
		{19, 18 },
		{20, 17 },
		{21, 16 },
		{22, 15 },
		{23, 14 },
		{24, 13 },
		{25, 12 },
		{26, 11 },
		{27, 10 },
		{28, 9 },
		{29, 8 },
		{30, 7 },
		{31, 6 },
		{32, 5 },
		{33, 4 },
		{34, 3 },
		{35, 2 },
		{36, 1 }
	};

        private Dictionary<int, int> BPC43Plus1 = new Dictionary<int, int>
    {
        {1, 44 },
        {2, 42 },
        {3, 41 },
        {4, 40 },
        {5, 39 },
        {6, 38 },
        {7, 37 },
        {8, 36 },
        {9, 35 },
        {10, 34 },
        {11, 33 },
        {12, 32 },
        {13, 31 },
        {14, 30 },
        {15, 29 },
        {16, 28 },
        {17, 27 },
        {18, 26 },
        {19, 25 },
        {20, 24 },
        {21, 23 },
        {22, 22 },
        {23, 21 },
        {24, 20 },
        {25, 19 },
        {26, 18 },
        {27, 17 },
        {28, 16 },
        {29, 15 },
        {30, 14 },
        {31, 13 },
        {32, 12 },
        {33, 11 },
        {34, 10 },
        {35, 9 },
        {36, 8 },
        {37, 7 },
        {38, 6 },
        {39, 5 },
        {40, 4 },
        {41, 3 },
        {42, 2 },
        {43, 1 }
    };
        private Dictionary<int, int> BPC43Plus1WinAndIn = new Dictionary<int, int>
    {
        {1, 440 },
        {2, 42 },
        {3, 41 },
        {4, 40 },
        {5, 39 },
        {6, 38 },
        {7, 37 },
        {8, 36 },
        {9, 35 },
        {10, 34 },
        {11, 33 },
        {12, 32 },
        {13, 31 },
        {14, 30 },
        {15, 29 },
        {16, 28 },
        {17, 27 },
        {18, 26 },
        {19, 25 },
        {20, 24 },
        {21, 23 },
        {22, 22 },
        {23, 21 },
        {24, 20 },
        {25, 19 },
        {26, 18 },
        {27, 17 },
        {28, 16 },
        {29, 15 },
        {30, 14 },
        {31, 13 },
        {32, 12 },
        {33, 11 },
        {34, 10 },
        {35, 9 },
        {36, 8 },
        {37, 7 },
        {38, 6 },
        {39, 5 },
        {40, 4 },
        {41, 3 },
        {42, 2 },
        {43, 1 }
    };

        private Dictionary<int, int> PointsSystem;

		private Dictionary<int, Label> DriversUIList = new Dictionary<int, Label>();

		public void SendSettings()
		{
            _settingsHubContext.Clients.All.UpdateSettings(_settings);
        }

		public void SaveSettings(Settings settings)
		{
			if(_settings.LeagueScoring != settings.LeagueScoring)
                _requiresReload = true;
            
			if(_settings.SimRacerHubSeasonNumber != settings.SimRacerHubSeasonNumber)
                _requiresReload = true;
			
			if(_settings.DropWeeks != settings.DropWeeks)
                _requiresReload = true;

			_settings = settings;
			SettingsHelpers.AddOrUpdateAppSetting("Settings", _settings);
		}


		public void RunStandingsHelper()
		{
			if (running)
			{
				_settingsHubContext.Clients.All.UpdateSettings(_settings);
				return;
			}

			running = true;

			SetPointsSystem();
			LoadStandingsFromSettings();

			sdk = new IRacingSDK();
			Task.Run(() => Loop());
		}

		private void TestLoop()
		{

			var test = new List<StandingsRow>();
			int counter = 1;
			while (true)
			{
				var x = new StandingsRow();
				x.Row = "Test" + counter;
				x.Size = 32;
				x.Color = "green";
				test.Add(x);

				_standingsHubContext.Clients.All.UpdateStandings(test);
				Thread.Sleep(2000);
			}
		}

		private void LoadStandingsFromSettings()
		{
            if (_settings.DropWeeks > 0)
                LoadRaceByRaceStandings();
            else
                LoadStandings();
        }

		private void SetPointsSystem()
		{
            if (_settings.LeagueScoring.ToLower() == "Bonfire".ToLower())
                PointsSystem = BTTRacePoints;
            else if (_settings.LeagueScoring.ToLower() == "NTRL".ToLower())
                PointsSystem = NTRLPoints;
            else if (_settings.LeagueScoring.ToLower() == "BPC43Plus1".ToLower())
                PointsSystem = BPC43Plus1;
            else if (_settings.LeagueScoring.ToLower() == "BPC43Plus1WinAndIn".ToLower())
                PointsSystem = BPC43Plus1WinAndIn;
            else
                PointsSystem = new Dictionary<int, int>();
        }


		private void Loop()
		{
			while (true)
			{
				if (_requiresReload)
				{

                    SetPointsSystem();
                    LoadStandingsFromSettings();
					_requiresReload = false;
                }

				if (sdk.IsConnected())
				{
					var data = sdk.GetSerializedData();
					if (data.Data.SessionState > 2)
					{
						var sessionData = sdk.GetSerializedSessionInfo();
						if (!carIdInitialized)
						{
							//Set Drivers CardIdx
							foreach (var driver in sessionData.DriverInfo.Drivers)
							{
								var matchedDriver = Drivers.FirstOrDefault(_ => _.Name == driver.UserName);

								if (matchedDriver == null)
								{
									matchedDriver = Drivers.FirstOrDefault(_ => FuzzySharp.Fuzz.Ratio(_.Name, driver.UserName) > 75);
								}
								if (matchedDriver != null)
								{
									matchedDriver.CarIdx = driver.CarIdx;
								}
								else
								{
									Console.WriteLine("Driver not found: " + driver.UserName);
									//Driver hasnt raced yet
									//Drivers.Add(new Driver
									//{
									//	Name = driver.UserName,
									//	CarIdx = driver.CarIdx,
									//	StartingPoints = 0,
									//	StartingRank = int.MaxValue,
									//	LivePoints = 0,
									//	RacePoints = new List<int>()
									//});
								}
							}
							var sessionCheck = sessionData?.SessionInfo.Sessions.FirstOrDefault(_ => _.SessionName == "RACE");

							//This should only stop checking drivers after the race starts
							if (sessionCheck?.ResultsPositions != null)
								carIdInitialized = true;

							foreach (var driver in Drivers)
							{
								driver.LivePoints = driver.StartingPoints;
							}
						}


						var mostLapsLed = 0;
						var carIdsWithMostLapsLed = new List<int>();

						var fastestLap = double.MaxValue;
						var carIdsWithFastestLap = new List<int>();

						var session = sessionData?.SessionInfo.Sessions.FirstOrDefault(_ => _.SessionName == "RACE");

						if (session?.ResultsPositions != null)
						{
							//Do race values
							foreach (var position in session.ResultsPositions)
							{
								if (Drivers.Any(_ => _.CarIdx == position.CarIdx))
								{
									//Logic specific to our drivers
									var matchedDriver = Drivers.FirstOrDefault(_ => _.CarIdx == position.CarIdx);

									matchedDriver.CurrentRacePoints = 0;

									if (PointsSystem.ContainsKey(position.Position))
									{
										matchedDriver.CurrentRacePoints += PointsSystem[position.Position];
									}

									if (position.LapsLed > 0)
									{
										matchedDriver.CurrentRacePoints += _settings.LedLapPoints;


									}

									if (position.Incidents == 0)
										matchedDriver.CurrentRacePoints += _settings.NoIncidentPoints;
								}

								//Full race driver checks
								if (position.LapsLed == mostLapsLed)
								{
									carIdsWithMostLapsLed.Add(position.CarIdx);
								}

								if (position.LapsLed > mostLapsLed)
								{
									mostLapsLed = position.LapsLed;
									carIdsWithMostLapsLed = new List<int> { position.CarIdx };
								}

								if (position.FastestLap == fastestLap)
								{
									carIdsWithFastestLap.Add(position.CarIdx);
								}

								if (position.FastestLap < fastestLap)
								{
									fastestLap = position.FastestLap;
									carIdsWithFastestLap.Add(position.CarIdx);
								}

							}



							//Add most laps led
							foreach (var carId in carIdsWithMostLapsLed)
							{
								var matchedDriver = Drivers.FirstOrDefault(_ => _.CarIdx == carId);
								if (matchedDriver == null)
									continue;
								matchedDriver.CurrentRacePoints += _settings.LedMostLapsPoints;

							}
							//Add fastest lap
							foreach (var carId in carIdsWithMostLapsLed)
							{
								var matchedDriver = Drivers.FirstOrDefault(_ => _.CarIdx == carId);
								if (matchedDriver == null)
									continue;
								matchedDriver.CurrentRacePoints += _settings.FastestLapPoints;

							}


							int? poleCarId = sessionData?.QualifyResultsInfo?.Results?.FirstOrDefault(_ => _.Position == 0)?.CarIdx;
							if (poleCarId != null && Drivers.Any(_ => _.CarIdx == poleCarId))
							{
								var poleCar = Drivers.FirstOrDefault(_ => _.CarIdx == poleCarId);
								if (poleCar == null)
									continue;
								poleCar.CurrentRacePoints += _settings.PolePoints;
							}

							if (_settings.DropWeeks > 0)
							{
								foreach (var driver in Drivers)
								{
									if (driver.RacePoints.Count() + 1 >= _settings.WeeksBeforeDropping)
									{
										var allWeeks = driver.RacePoints.Select(_ => _).ToList();
										allWeeks.Add(driver.CurrentRacePoints);
										driver.LivePoints = allWeeks.OrderByDescending(_ => _).Take(allWeeks.Count() - _settings.DropWeeks).ToList().Sum();
									}
									else
									{
										driver.LivePoints = driver.CurrentRacePoints + driver.StartingPoints;
									}
								}


							}
							else
							{
								//Normal standings or playoff cutoff
								foreach (var driver in Drivers)
								{
									driver.LivePoints = driver.CurrentRacePoints + driver.StartingPoints;
								}
							}
						}

						var standings = new List<StandingsRow>();

						if (_settings.EnablePlayoffMode)
						{
							//Cutoff line
							var drivers = Drivers.OrderByDescending(_ => _.LivePoints);

							var cutoffDriver = drivers.ElementAt(_settings.CutoffDrivers - 1);
							var firstDriverOut = drivers.ElementAt(_settings.CutoffDrivers);


							var maxCounterValue = GetMaxLoopingValue(drivers.Count());
							for (int counter = 1; counter <= maxCounterValue; counter++)
							{
								var driver = drivers.ElementAt(counter - 1);

								var rowToAdd = new StandingsRow
								{
									Size = _settings.FontSize
								};

								var pointsDiff = 0;
								if (counter <= _settings.CutoffDrivers)
								{
									//Compare to first one out
									pointsDiff = driver.LivePoints - firstDriverOut.LivePoints;

									rowToAdd.Row = driver.Name + $" (+{pointsDiff})";
									rowToAdd.Color = "green";

								}
								else
								{
									//Compare to cutoff
									pointsDiff = cutoffDriver.LivePoints - driver.LivePoints;

									rowToAdd.Row = driver.Name + $" (-{pointsDiff})";
									rowToAdd.Color = "red";

								}
								standings.Add(rowToAdd);
							}
						}
						else
						{
							//Standard standings
							var drivers = Drivers.OrderByDescending(_ => _.LivePoints);

							var maxCounterValue = GetMaxLoopingValue(drivers.Count());
							for (int counter = 1; counter <= maxCounterValue; counter++)
							{
								var driver = drivers.ElementAt(counter - 1);

								var rowToAdd = new StandingsRow
								{
									Size = _settings.FontSize
								};

								rowToAdd.Row = $"{counter}. " + driver.Name + $" ({driver.LivePoints})";
								rowToAdd.Color = GetStandingsColorText(driver.StartingRank, counter);

								standings.Add(rowToAdd);
							}
						}

						_standingsHubContext.Clients.All.UpdateStandings(standings);
					}
				}
				Thread.Sleep(2000);

			}
		}

		private int GetMaxLoopingValue(int driverCount)
		{
			var list = new List<int> { driverCount, _settings.MaxDriversToShow };

			if (_settings.EnablePlayoffMode)
				list.Add(_settings.CurrentPlayoffDrivers);


			return list.Min();
		}

		private string GetStandingsColorText(int startingPosition, int currentPosition)
		{
			//Remember his is backwards.  If they start 3rd and are now 2nd then that is good
			if (startingPosition > currentPosition)
				return "Green";
			if (currentPosition > startingPosition)
				return "Red";
			if (currentPosition == startingPosition)
				return "Black";

			return "Purple";
		}

		private void LoadStandings()
		{
			Drivers = new List<Driver>();

			var urlRoot = "https://www.simracerhub.com/scoring/season_standings.php?season_id=";
			var url = urlRoot + _settings.SimRacerHubSeasonNumber;
			var web = new HtmlWeb();
			var doc = web.Load(url);
			var table = doc.GetElementbyId("driver_table");

			var driverColumnIndex = -1;
			var totalPointsColumnIndex = -1;
			var standingRankColumnIndex = -1;

			foreach (HtmlNode row in table.SelectNodes("tr"))
			{
				if (row.Attributes.Any(_ => _.Value == "jsTableHdr"))
				{
					var headerCells = row.SelectNodes("th|td");
					for (int i = 0; i < headerCells.Count(); i++)
					{
						var cell = headerCells[i];
						if (cell.InnerText == "Driver")
							driverColumnIndex = i;
						if (cell.InnerText == "TotPts")
							totalPointsColumnIndex = i;
						if (cell.InnerText == "Pos")
							standingRankColumnIndex = i;

					}
				}
				else
				{


					var cells = row.SelectNodes("th|td");

					var driver = new Driver
					{
						Name = CleanName(cells[driverColumnIndex].InnerText),
						StartingPoints = Int32.Parse(cells[totalPointsColumnIndex].InnerText, System.Globalization.NumberStyles.Any),
						LivePoints = Int32.Parse(cells[totalPointsColumnIndex].InnerText, System.Globalization.NumberStyles.Any),
						StartingRank = Int32.Parse(cells[standingRankColumnIndex].InnerText),
						RacePoints = new List<int>()
					};
					Drivers.Add(driver);
				}
			}
		}

		private void LoadRaceByRaceStandings()
        {
            Drivers = new List<Driver>();
            var urlRoot = "https://www.simracerhub.com/scoring/season_standings.php?grid=y&season_id=";
			var url = urlRoot + _settings.SimRacerHubSeasonNumber;
			var web = new HtmlWeb();
			var doc = web.Load(url);
			var table = doc.GetElementbyId("driver_grid");

			var driverColumnIndex = -1;
			var pointsColumnIndexes = new List<int>();
			var standingsCounter = 1;

			foreach (HtmlNode row in table.SelectNodes("tr"))
			{
				if (row.Attributes.Any(_ => _.Value.StartsWith("driver_")))
				{
					var cells = row.SelectNodes("th|td");

					var driver = new Driver
					{
						Name = CleanName(cells[0].InnerText),
						StartingRank = standingsCounter++,
						RacePoints = new List<int>()
					};

					for (int i = 1; i < cells.Count - 1; i++)
					{
						if (cells[i].Attributes.Any(_ => _.Name == "colspan"))
						{
							driver.RacePoints.Add(0);
						}
						else
						{

							driver.RacePoints.Add(Int32.Parse(cells[i].InnerText));
							i = i + 3;
						}
					}


					driver.StartingPoints = driver.RacePoints.Sum();
					Drivers.Add(driver);
				}
			}
		}

		private string CleanName(string name)
		{
			var digits = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
			if (_settings.LeagueRemovesNumberSuffix)
				return name.TrimEnd(digits);

			return name;
		}




	}
}
