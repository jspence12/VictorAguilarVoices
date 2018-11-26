using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VictorMVC.Models
{
    interface ITable
    {
        int ID { get; set; }
    }
    interface IOrderedTable : ITable
    {
        int List_Order { get; set; }
    }
    /// <summary>
    /// Base class for all db tables
    /// </summary>
    public abstract class Table : ITable
    {
        [Key]
        [Editable(false)]
        public int ID { get; set; }
    }

    /// <summary>
    /// DbTable without a user-defined ordering. Separated inheritane
    /// used for typing of DbContext methods with generic types
    /// </summary>
    public abstract class UnorderedTable : Table, ITable
    {
    }

    /// <summary>
    /// db table with a user-defined ordering.
    /// </summary>
    public abstract class OrderedTable : Table, IOrderedTable
    {
        [Required]
        [DisplayName("List Order")]
        [Editable(true)]
        [Range(1, int.MaxValue)]
        public int List_Order { get; set; }
    }
}