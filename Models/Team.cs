using System.Numerics;

namespace MerkatorS.Models
{
    public class Team
    {
        public int TeamId { get; set; }
        public string Name { get; set; }
        public int? Points { get; set; }

        public ICollection<Player>? Players { get; set; }

        public ICollection<Match>? Matches { get; set; }
        
    }
}
