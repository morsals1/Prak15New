using System.ComponentModel.DataAnnotations.Schema;

namespace Prak15Mensh.Models
{
    [Table("product_tags")]
    public class ProductTag
    {
        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("tag_id")]
        public int TagId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }
    }
}