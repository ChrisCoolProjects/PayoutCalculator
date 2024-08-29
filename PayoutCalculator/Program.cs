using Microsoft.AspNetCore.Mvc.Formatters;
/* * * * * * * * * * * * * * * * * * * * * * * *
 * Outputs of this program:
 * Entrant count and prize pool for a specific tournament
 * Average entrants for all tournaments
 * How many tournaments a player placed in
 * Input data for tournament, tournament is added to database and payout is automatically calculated and displayed
 * 
 *
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
    public void listEvents(){
        foreach (Tournament eventName in eventList){
            Console.WriteLine(eventName.ID);
        }
    }
    public void averagePrizePot(){
        double prizeTotal = 0.0;
        foreach (Tournament eventName in eventList){
            prizeTotal += eventName.calcPrizePot(eventName.entrantCount);
        }
        double avgPrizePot = prizeTotal / eventList.Count();
        Console.WriteLine(avgPrizePot);
    }
}
class Tournament{
    public int ID;
    Player[] podium;
    public int entrantCount = 0;
    public double prizePot;
    public double firstPlacePayout;
    public double secondPlacePayout;
    public double thirdPlacePayout;

    public Tournament(){

    }
    public Tournament(int tID, Player firstPlace, Player secondPlace, Player thirdPlace){
        ID = tID;
        Player[] podium = {firstPlace, secondPlace, thirdPlace};
    }

    public double calcPrizePot(int entrantCount){
        int potBonus = (entrantCount > 5) ? 50 : 0; //if entrant count is 5 or more, add $50 to pot bonus

        prizePot = (entrantCount * 5) + potBonus;
        firstPlacePayout = prizePot * .5;
        secondPlacePayout = prizePot * .3;
        thirdPlacePayout = prizePot * .2;

        return prizePot;
    }
}


class Player{

    public string Name;
    public double Earnings = 0.0;

    public Player(string playerName){
        Name = playerName;
    }
}

class Program{
    public static void Main(String[] args){
        Player player1 = new Player("PapaPesto");
        Player player2 = new Player("Silent");
        Player player3 = new Player("GreyFaiden");

        Tournament QDB1 = new Tournament(1, player1, player2, player3);
        QDB1.entrantCount = 30;
        QDB1.calcPrizePot(QDB1.entrantCount);

        player1.Earnings += QDB1.firstPlacePayout;
        player2.Earnings += QDB1.secondPlacePayout;
        player3.Earnings += QDB1.thirdPlacePayout;

        Console.WriteLine(player1.Name + "'s earned $" + player1.Earnings + " overall from events.");
        Console.WriteLine(player2.Name + "'s earned $" + player2.Earnings + " overall from events.");
        Console.WriteLine(player3.Name + "'s earned $" + player3.Earnings + " overall from events.");
        
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