namespace MerkatorS.Models
{
    public class TeamRanking
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int Points { get; set; }
        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
    }
}
