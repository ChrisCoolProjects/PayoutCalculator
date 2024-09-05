using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SignalR;
/* * * * * * * * * * * * * * * * * * * * * * * *
 * Outputs of this program:
 * Entrant count and prize pool for a specific tournament
 * Average entrants for all tournaments
 * How many tournaments a player placed in
 * Input data for tournament, tournament is added to database and payout is automatically calculated and displayed
 * 
 * Player Stats:
 * 
 * 
 * 
 * 
 * 
 * Final goal: "Is this player net positive after all their entrances?"
 * * * * * * * * * * * * * * * * * * * * * * * */
public class League{
    private List<Tournament> eventList = new List<Tournament>();
    public static League seasonOne;
    public League(){
        if(seasonOne == null)
        seasonOne = this;
    }
    public void ListEvents(){
        foreach (Tournament eventName in eventList){
            Console.WriteLine(eventName.ID);
        }
    }
    public double AveragePrizePot(){ //will be refactored
        double prizeTotal = 0.0;
        foreach (Tournament eventName in eventList){
            prizeTotal += eventName.calcPrizePot(eventName.getEntrantCount());
        }
        double avgPrizePot = prizeTotal / eventList.Count();
        return avgPrizePot;
    }
    public double AverageEntrants(){ //will be refactored
        int allEntrants = 0;
        foreach(Tournament eventName in eventList){
            allEntrants += eventName.getEntrantCount();
        }
        double avgEntrants = allEntrants / eventList.Count();
        return avgEntrants;
    }
   // public void AddTournament(Tournament newEvent){
    //    eventList.Add(newEvent);
    //}
}


public class Tournament{
    public int ID;
    public Player[] podium = [];
    protected int entrantCount {get; set;} = 0;
    private double prizePot {get; set;}
    public double firstPlacePayout;
    public double secondPlacePayout;
    public double thirdPlacePayout;
    private bool attendanceMarked = false;
    private bool winningsAssigned = false;
        public Tournament(){
    }
    public Tournament(int playerCount, Player firstPlace, Player secondPlace, Player thirdPlace){
        entrantCount = playerCount;
        calcPrizePot(entrantCount);
        setPodium(firstPlace, secondPlace, thirdPlace);
        MarkAttendance();
        AssignWinnings();
    }

    public double calcPrizePot(int entrantCount){
        int potBonus = (entrantCount > 5) ? 50 : 0; //if entrant count is 5 or more, add $50 to pot bonus

        prizePot = (entrantCount * 5) + potBonus;
        firstPlacePayout = prizePot * .5;
        secondPlacePayout = prizePot * .3;
        thirdPlacePayout = prizePot * .2;

        return prizePot;
    }
    public void setPodium(Player firstPlace, Player secondPlace, Player thirdPlace){
        podium = [firstPlace, secondPlace, thirdPlace];
    }
    public void MarkAttendance(){
        if(!attendanceMarked){
//          Tournament currentEvent = new Tournament();
            for(int i = 0; i < 3; i++){ 
                podium[i].Attendance.Add(this); // Works now!!
            }
            attendanceMarked = true;
        }
    }

    public void AssignWinnings(){
        if(prizePot != 0 && winningsAssigned == false){
        podium[0].Earnings+=firstPlacePayout;
        podium[1].Earnings+=secondPlacePayout;
        podium[2].Earnings+=thirdPlacePayout;
        }
        winningsAssigned = true;
    }

    public int getEntrantCount(){
        return entrantCount;
    }

    public void PromptForTournament(){
        int escapeVar = 1;
        while(escapeVar != 0){
        Tournament newTournament = new Tournament();
        double localPrizePot = 0;

        string userFirstPlace, userSecondPlace, userThirdPlace = "";

        Console.WriteLine("Enter the number of entrants for this tournament");
        newTournament.entrantCount = Convert.ToInt32(Console.ReadLine());
        localPrizePot = newTournament.calcPrizePot(newTournament.entrantCount);

        Console.WriteLine("Who got first place?");
        userFirstPlace = Console.ReadLine();

        Console.WriteLine("Who got second place?");
        userSecondPlace = Console.ReadLine();

        Console.WriteLine("Who got third place?");
        userThirdPlace = Console.ReadLine();

        Console.WriteLine($"{userFirstPlace} won ${newTournament.firstPlacePayout}.");
        Console.WriteLine($"{userSecondPlace} won ${newTournament.secondPlacePayout}.");
        Console.WriteLine($"{userThirdPlace} won ${newTournament.thirdPlacePayout}.");
        Console.WriteLine();

        Console.WriteLine("Main Menu: \n 0: Exit \n Any other number: Run calculation again.");
        escapeVar = Convert.ToInt32(Console.ReadLine());

        }

    }
}


public class Player{

    public string Name;
    public double Earnings = 0.0;
    public List<Tournament> Attendance = [];
    public Player(string playerName){
        Name = playerName;
    }

    public void CheckAttendance(){
        Console.WriteLine($"{Name} has attended {Attendance.Count} event{(Attendance.Count == 1 ? "" : "s")}.");
    }
    public void CheckEarnings(){
        Console.WriteLine(Name + " has earned $" + Earnings + " overall from events.");
    }
    
    public void DisplayStats(){
        CheckAttendance();
        CheckEarnings();
        Console.WriteLine();        
    }

}

public class Roster{
    protected List<Player> players = [];

    public void AddPlayer(Player newPlayer){
        if(!players.Contains(newPlayer)){
            players.Add(newPlayer);
        }
    }

    public void ListPlayers(){
        players = players.OrderBy(player=>-player.Earnings).ToList();
        foreach(Player currentPlayer in players){
            Console.WriteLine($"{players.IndexOf(currentPlayer)+1}. {currentPlayer.Name}: ${currentPlayer.Earnings}");//, (players.IndexOf(currentPlayer) + 1), currentPlayer.Name);
        }
    }
}

class Program{
    public static void Main(String[] args){

        Roster currentRoster = new Roster();
        Player player1 = new Player("PapaPesto"); currentRoster.AddPlayer(player1);
        Player player2 = new Player("Silent"); currentRoster.AddPlayer(player2);
        Player player3 = new Player("GreyFaiden"); currentRoster.AddPlayer(player3);
        Player player4 = new Player("StoryTime"); currentRoster.AddPlayer(player4);
        Player player5 = new Player("Knotts"); currentRoster.AddPlayer(player5);
        

        Tournament QDB1 = new Tournament(30, player1, player2, player3);
        //League.seasonOne.AddTournament(QDB1);

        Tournament QDB2 = new Tournament(24, player5, player1, player4);
        //League.seasonOne.AddTournament(QDB2);
        //player1.DisplayStats();
        //player5.DisplayStats();

        currentRoster.ListPlayers();
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