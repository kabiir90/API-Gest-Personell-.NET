using System.ComponentModel.DataAnnotations;


namespace GestionPer.Models
{
    public class Employees
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nom { get; set; }

        [Required]
        public string Prenom { get; set; }

        public string Tele { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Photo { get; set; }
        public string Role { get; set; }
    }
}
