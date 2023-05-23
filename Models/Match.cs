namespace MerkatorS.Models
{
    public class Match
    {
        public int Id { get; set; }
        public int HomeTeamId { get; set; }
        public int HomeTeamGoals { get; set; }
        public Team? HomeTeam { get; set; }
		public string HomeTeamName { get; set; }
		public int AwayTeamId { get; set; }
        public int AwayTeamGoals { get; set; }
        public Team? AwayTeam { get; set; }
		public string AwayTeamName { get; set; }
		public int WhoWon { get; set; }//1 home won, 0 draw, 2 away won
    }
}
