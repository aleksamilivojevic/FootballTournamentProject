using System.ComponentModel.DataAnnotations;

namespace MerkatorS.Models
{
    public class Administrator
    {
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
