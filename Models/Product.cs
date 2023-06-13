using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JWT.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string? Name { get; set; }
        [Required]
        [StringLength(150)]
        public string? Description { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(8,2)")]
        public decimal Price { get; set; }
    }
}
