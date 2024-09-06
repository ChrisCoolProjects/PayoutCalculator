using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SignalR;

/* * * * * * * * * * * * * * * * * * * * * * * *
 * Outputs of this program:
 * Entrant count and prize pool for a specific tournament
 * Average entrants for all tournaments
 * How many tournaments a player placed in
 * Input data for tournament, tournament is added to database and payout is automatically calculated and displayed
 *
 * Important questions that need answering:
 How am I handling the data?
    My options are to:
    Call the API any time I need any data and run everything at runtime.
    Store the data locally and run an update function that fetches data via the main menu.
        Is this data being stored in jsons or an excel or both?
        I need to store tournament data and player data. Should I have one json file for each?
        I think the answer for this question lies in what data I want to store.
        There's a lot of redundancies in storing tourney and player data. I already accepted that.
I think the main thing bugging me right now is the scenario where I'm importing players from a new tournament and running into duplicate players that are already stored. I want an easy way to run check duplicates and "diving into a json file" feels like an awful solution. If no persistent storage means that I'm calling the API super often and JSON persistent storage means I have to check against multiple JSON files whenever I run an import then I think some sort of csv/xlsx is the easiest way to hold onto my data. I only have to bring things into runtime memory when they're needed and the JSON files only get used during the import process which is fine with me.
ASSUMING that any persistent tournament data has all players and not just top x I think it's better to only make data persistent when it's pulled directly from the start.gg API. The "payout calculator" function will ask for entrant count and top 3/4 but is almost completely separate from any long term stats besides implementing some of the same math functions. The start.gg import will ask for tournament name and probably game name.

So the pipeline now is:
Main Menu:
0: Use Payout Calculator
1: Import Start.GG Data
	Please Enter the Tournament to Import
	Please Choose the Game to Update
	1.1: Under Night In-Birth II Sys:Celes
	1.2: Granblue Fantasy Versus: Rising
	1.3: Blazblue: Central Fiction
	1.4: Persona 4 Arena: Ultimax
2: View Stats
	2.1: View Player Stats
	2.2: View Head-to-Head Stats?? (I LOVE feature creep!!)
	2.3: View Tournament Stats
	2.4: View Year/League Stats

Different games will be stored in different excel sheets.
Different tournaments will be stored in different rows of the excel sheets
	(This may change if we expand to include match data)
All players will probably be stored in one spreadsheet? This matters for calculating total earnings across games.


Tournament Data:
Entrant Count
Prize Pot
Date?
Top 4

Player Data:
Name
Events Attended
Events Won
Money Won

 *
 * Okay there's three different floodgate tiers to this project at the moment:
    Import EVERYONE into the data instead of top 4
    Import match results as well
 *
 *
 *
 *
 *
 *
 * Final goal: "Is this player net positive after all their entrances?"
 * * * * * * * * * * * * * * * * * * * * * * * */
public class League {
	private List<Tournament> eventList = new List<Tournament>();
	public static League seasonOne;
	public League() {
		if (seasonOne == null)
			seasonOne = this;
	}
	public void ListEvents() {
		foreach (Tournament eventName in eventList) {
			//Console.WriteLine(eventName.ID);
		}
	}
	public double AveragePrizePot() { //will be refactored
		double prizeTotal = 0.0;
		foreach (Tournament eventName in eventList) {
			prizeTotal += eventName.calcPrizePot(eventName.getEntrantCount());
		}
		double avgPrizePot = prizeTotal / eventList.Count();
		return avgPrizePot;
	}
	public double AverageEntrants() { //will be refactored
		int allEntrants = 0;
		foreach (Tournament eventName in eventList) {
			allEntrants += eventName.getEntrantCount();
		}
		double avgEntrants = allEntrants / eventList.Count();
		return avgEntrants;
	}
	// public void AddTournament(Tournament newEvent){
	//    eventList.Add(newEvent);
	//}
}


public class Tournament {
	public Player[] podium = [];
	protected int entrantCount { get; set; } = 0;
	private double prizePot { get; set; }
	private double firstPlacePayout;
	private double secondPlacePayout;
	private double thirdPlacePayout;
	private double fourthPlacePayout;
	private bool attendanceMarked = false;
	private bool winningsAssigned = false;
	public Tournament() {
	}

	public Tournament(int playerCount, Player firstPlace, Player secondPlace, Player thirdPlace, Player fourthPlace) {
		entrantCount = playerCount;
		calcPrizePot(entrantCount);
		setPodium(firstPlace, secondPlace, thirdPlace, fourthPlace);
		MarkAttendance();
		AssignWinnings();
	}
	public double calcPrizePot(int entrantCount) {
		int potBonus = (entrantCount > 5) ? 50 : 0; //if entrant count is 5 or more, add $50 to pot bonus
		prizePot = (entrantCount * 5) + potBonus; //each player adds $5 to prize pot as entry fee

		if (entrantCount < 10) { //less than 10 entrants means top 3 payout, 50/30/20 split
			firstPlacePayout = prizePot * .5;
			secondPlacePayout = prizePot * .3;
			thirdPlacePayout = prizePot * .2;
			fourthPlacePayout = 0;
		}
		else { //10 or more entrants means top 4 payout, 50/25/15/10 split
			firstPlacePayout = prizePot * .5;
			secondPlacePayout = prizePot * .25;
			thirdPlacePayout = prizePot * .15;
			fourthPlacePayout = prizePot * .1;
		}

		return prizePot;
	}


	public void setPodium(Player firstPlace, Player secondPlace, Player thirdPlace, Player fourthPlace){
		podium = [firstPlace, secondPlace, thirdPlace, fourthPlace];
	}

	public void MarkAttendance() {
		if (!attendanceMarked) {
			//          Tournament currentEvent = new Tournament();
			for (int i = 0; i < podium.Length; i++) {
				podium[i].Attendance.Add(this); // Works now!!
			}
			attendanceMarked = true;
		}
	}

	public void AssignWinnings() {
		if (prizePot != 0 && winningsAssigned == false) {
			podium[0].Earnings += firstPlacePayout;
			podium[1].Earnings += secondPlacePayout;
			podium[2].Earnings += thirdPlacePayout;
			if(fourthPlacePayout > 0){podium[3].Earnings += fourthPlacePayout;}
		}
		winningsAssigned = true;
	}

	public int getEntrantCount() {
		return entrantCount;
	}

	public void PromptForTournament() {
		int escapeVar = 1;
		while (escapeVar != 0) {
			Tournament newTournament = new Tournament();
			double localPrizePot = 0;

			string userFirstPlace, userSecondPlace, userThirdPlace, userFourthPlace = "";

			Console.WriteLine("Enter the number of entrants for this tournament");
			newTournament.entrantCount = Convert.ToInt32(Console.ReadLine());
			localPrizePot = newTournament.calcPrizePot(newTournament.entrantCount);

			Console.WriteLine("Who got first place?");
			userFirstPlace = Console.ReadLine();

			Console.WriteLine("Who got second place?");
			userSecondPlace = Console.ReadLine();

			Console.WriteLine("Who got third place?");
			userThirdPlace = Console.ReadLine();

			Console.WriteLine("Who got fourth place?");
			userFourthPlace = Console.ReadLine();

			Console.WriteLine($"{userFirstPlace} won ${newTournament.firstPlacePayout}.");
			Console.WriteLine($"{userSecondPlace} won ${newTournament.secondPlacePayout}.");
			Console.WriteLine($"{userThirdPlace} won ${newTournament.thirdPlacePayout}.");
			Console.WriteLine($"{userFourthPlace} won ${newTournament.fourthPlacePayout}.");
			Console.WriteLine();

			Console.WriteLine("Main Menu: \n 0: Exit \n Any other number: Run calculation again.");
			escapeVar = Convert.ToInt32(Console.ReadLine());

		}

	}
}


public class Player {

	public string Name;
	public double Earnings = 0.0;
	public List<Tournament> Attendance = [];
	public Player(string playerName) {
		Name = playerName;
	}

	public void CheckAttendance() {
		Console.WriteLine($"{Name} has attended {Attendance.Count} event{(Attendance.Count == 1 ? "" : "s")}.");
	}
	public void CheckEarnings() {
		Console.WriteLine(Name + " has earned $" + Earnings + " overall from events.");
	}

	public void DisplayStats() {
		CheckAttendance();
		CheckEarnings();
		Console.WriteLine();
	}

}

public class Roster {
	protected List<Player> players = [];

	public void AddPlayer(Player newPlayer) {
		if (!players.Contains(newPlayer)) {
			players.Add(newPlayer);
		}
	}

	public void ListPlayers() {
		players = players.OrderBy(player => -player.Earnings).ToList();
		foreach (Player currentPlayer in players) {
			Console.WriteLine($"{players.IndexOf(currentPlayer) + 1}. {currentPlayer.Name}: ${currentPlayer.Earnings}");//, (players.IndexOf(currentPlayer) + 1), currentPlayer.Name);
		}
	}
}

class Program {
	public static void Main(String[] args) {

		/*Main Menu:
0: Use Payout Calculator
1: Import Start.GG Data
	Please Enter the Tournament to Import -- Only UNI2 for now
2: View Stats
	2.1: View Player Stats
	2.2: View Head-to-Head Stats?? (I LOVE feature creep!!)
	2.3: View Tournament Stats
	2.4: View Year/League Stats
*/
		int menuNumber = 99;

		Roster currentRoster = new Roster();
		Player player1 = new Player("PapaPesto"); currentRoster.AddPlayer(player1);
		Player player2 = new Player("Silent"); currentRoster.AddPlayer(player2);
		Player player3 = new Player("GreyFaiden"); currentRoster.AddPlayer(player3);
		Player player4 = new Player("StoryTime"); currentRoster.AddPlayer(player4);
		Player player5 = new Player("Knotts"); currentRoster.AddPlayer(player5);


		Tournament QDB1 = new Tournament(30, player1, player2, player3, player4);
		//League.seasonOne.AddTournament(QDB1);

		Tournament QDB2 = new Tournament(24, player5, player1, player4, player2);

		while(menuNumber != 0){
			Console.Write("Welcome to the Main Menu. Enter the corresponding number to continue, otherwise, press 0 to exit.\n"
						+ "1: Use Payout Calculator\n"
						+ "2: Import start.gg Data\n"
						+ "3: View Stats\n"
						+ "0: Exit\n\n");
			menuNumber = Convert.ToInt32(Console.ReadLine());
			switch (menuNumber){
				case 1:
				QDB2.PromptForTournament();
				break;

				case 2:
				Console.WriteLine("This feature is coming soon. Moving back to main menu shortly.");
				Thread.Sleep(5000);
				break;

				case 3: //This will eventually branch into "do you want player stats or tourney or league stats" but for now it's just a list of top earners.
				currentRoster.ListPlayers();
				Console.WriteLine("Press any key to continue...");
				Console.ReadLine();
				break;

				case 0:
				break;

				default:
				Console.WriteLine("That is not a valid entry for this menu. Please enter a valid number.");
				Thread.Sleep(5000);
				break;
			}
			Console.WriteLine();
		}

		//League.seasonOne.AddTournament(QDB2);
		//player1.DisplayStats();
		//player5.DisplayStats();

		//currentRoster.ListPlayers();
		//QDB2.PromptForTournament();

	}
}



/**
League - Singleton with a list of all tournaments
List<Tournament> seasonX = new List<Tournament>;
public double averagePrizePot(), lowestPrizePot(), highestPrizePot();
public int averageEntrantCount(), lowestEntrantCount(), highestEntrantCount();

Tournament(int ID, Player[] podium, int entrantCount)
double prizePot = private double calcPrizePot(entrantCount);
public getID(), getPodium(), getEntrantCount();
League.add(Tournament); //happens at the end of instantiation

Player
> string tag, overallEarnings(!?),


Roster???
-Same as League but with a list of players
-I feel like having this should imply that all players are read in and not just podium
-

Questions for Future Chris:
where on earth are you putting the calculateEarnings function for each player. Is that something that'll be handled by the player class or is that purely an informational class.
If that's an informational class does it REALLY have a purpose?


**/
