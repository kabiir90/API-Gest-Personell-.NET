using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GestionPer.Models
{
    public class Holiday
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Prenom { get; set; }

        public string Role { get; set; }

        [Column(TypeName = "Date")]
        public DateTime DateDebut { get; set; }

        [Column(TypeName = "Date")]
        public DateTime DateFin { get; set; }
        
    }
}
