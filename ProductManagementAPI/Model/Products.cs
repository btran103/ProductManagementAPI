using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManagementAPI.Models;

[Table("products")]
public class Product
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column("name", TypeName = "varchar(255)")]
    public string Name { get; set; } = string.Empty;

    [Column("description", TypeName = "nvarchar(max)")]
    public string? Description { get; set; }

    [Column("price", TypeName = "decimal(10,2)")]
    [Range(typeof(decimal), "0.01", "99999999.99")]
    public decimal Price { get; set; }

    [Column("quantity")]
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}