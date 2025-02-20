using System.ComponentModel.DataAnnotations;

namespace GestionPer.Models
{
    public class Malady
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Prenom { get; set; }

        public string Role { get; set; }
        public string Description { get; set; }
    }
}
