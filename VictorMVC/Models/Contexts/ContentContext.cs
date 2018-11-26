using MySql.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace VictorMVC.Models
{

    /// <summary>
    /// DbContext for all tables used in some way by visitors to the website
    /// </summary>
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class ContentContext : DbContext
    {
        private static string _demoDir = $"{HttpRuntime.AppDomainAppPath}/Content/Audio/";

        public ContentContext() : base("victor") { }
        public DbSet<Demo> Demos {get;  set;}                       //Voice Demos and locations
        public DbSet<EmailRecipient> EmailRecipients { get; set; }  //List of recipients of emails sent from contact page
        public DbSet<SmtpHost> SmtpHosts { get; set; }              //Single Smtp Host which sends the email
        public DbSet<Article> Articles { get; set; }                //Articles On the front page of the website
        public DbSet<Image> Images { get; set; }

        /// <summary>
        /// Creates a new ordered record in database.
        /// Business logic ensures that List_Order field remains contiguous.
        /// </summary>
        /// <param name="newRow">Ordered Row to Add to database</param>
        public void CreateOrdered<T>(DbSet<T> dbset, T newRow)
            where T : OrderedTable
        {
            newRow.List_Order = getNewPriority(Demos, newRow.List_Order, true);
            IQueryable<T> ListOrderChanges = dbset.Where(d => d.List_Order >= newRow.List_Order);
            ListOrderChanges = IncrementOrder(ListOrderChanges);
            dbset.Add(newRow);
        }

        /// <summary>
        /// Updates an existing ordered record in the database. Row ID is passed in hidden field in UI to server.
        /// Business logic ensures that List_Order field remains contiguous.
        /// </summary>
        /// <param name="updatedRow"></param>
        public void UpdateOrdered<T>(DbSet<T> dbset, T updatedRow)
            where T : OrderedTable
        {
            T currentRow = dbset.Find(updatedRow.ID);
            if (currentRow == null)
            {
                throw new KeyNotFoundException();
            }
            int? oldPriority = currentRow?.List_Order;
            updatedRow.List_Order = getNewPriority(dbset,updatedRow.List_Order, false);
            // Listed later; decrement dbset in list
            if (updatedRow.List_Order > oldPriority)
            {
                IQueryable<T> ListOrderChanges = dbset.Where(
                    d => d.List_Order <= updatedRow.List_Order
                    && d.List_Order > oldPriority);
                ListOrderChanges = DecrementOrder(ListOrderChanges);
            }
            // Listed earlier; increment dbset in list
            else if (updatedRow.List_Order < oldPriority)
            {
                IQueryable<T> ListOrderChanges = dbset.Where(
                    d => d.List_Order >= updatedRow.List_Order
                    && d.List_Order < oldPriority);
                ListOrderChanges = IncrementOrder(ListOrderChanges);
            }
            Entry(currentRow).CurrentValues.SetValues(updatedRow);
        }


        /// <summary>
        /// Deletes an ordered record from the database. Row ID is passed in hidden field in UI to server.
        /// Business logic ensures that List_Order field remains contiguous.
        /// </summary>
        /// <param name="id"></param>
        public void DeleteOrdered<T>(DbSet<T> dbset, int id)
            where T : OrderedTable
        {
            T currentRow = dbset.Find(id);
            IQueryable<T> ListOrderChanges = dbset.Where(d => d.List_Order > currentRow.List_Order);

            if (currentRow is null)
            {
                //TODO: Add log for file which we tried to remove
                throw new KeyNotFoundException();
            }
            ListOrderChanges = DecrementOrder(ListOrderChanges);
            dbset.Remove(currentRow);
        }

        /// <summary>
        /// Creates a new unordered record
        /// </summary>
        /// <param name="newRow"></param>
        public void CreateUnordered<T>(DbSet<T> dbset, T newRow)
            where T : UnorderedTable
        {
            dbset.Add(newRow);
        }

        /// <summary>
        /// updates existing record from an unordered table.
        /// </summary>
        /// <param name="updatedRow">row user has requested to be updated. Row ID is passed in hidden field to server.</param>
        public void UpdateUnordered<T>(DbSet<T> dbset, T updatedRow)
            where T : UnorderedTable
        {
            T currentRow = dbset.SingleOrDefault(e => e.ID == updatedRow.ID);

            if (currentRow != null )
            {
                Entry(currentRow).CurrentValues.SetValues(updatedRow);
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Deletes unordered record from database
        /// </summary>
        /// <param name="id">db ID of row to be deleted</param>
        public void DeleteUnordered<T>(DbSet<T> dbset,int id)
            where T : UnorderedTable
        {
            T currentRow = dbset.SingleOrDefault(e => e.ID == id);
            if (currentRow is null)
            {
                throw new KeyNotFoundException();
            }
            dbset.Remove(currentRow);
        }

        /// <summary>
        /// Decrements List_Order property for all rows in a given query. DB Set must inherit from 
        /// </summary>
        /// <param name="Queryset">Query of a table inheriting from OrderedTable class</param>
        /// <returns>Queryset where all values have been decreased by one</returns>
        private IQueryable<T> DecrementOrder<T>(IQueryable<T> Queryset)
            where T : OrderedTable
        {
            foreach (OrderedTable row in Queryset)
            {
                row.List_Order -= 1;
            }
            return Queryset;
        }

        /// <summary>
        /// Increments List_Order property for all rows in a given query.
        /// </summary>
        /// <param name="Queryset">Query of a table inheriting from OrderedTable class</param>
        /// <returns>Queryset where all values have been increased by one</returns>
        private IQueryable<T> IncrementOrder<T>(IQueryable<T> Queryset)
            where T : OrderedTable
        {
            foreach (T row in Queryset)
            {
                row.List_Order += 1;
            }
            return Queryset;
        }
        /// <summary>
        /// Maintains contiguous nature of List_Order field in an ordered table by capping max allowable input
        /// </summary>
        /// <param name="dbset">Data set to check list order against</param>
        /// <param name="postedPriority">List_Order submitted by the user</param>
        /// <param name="isNew">True if for a create operation. False if for an update operation</param>
        /// <returns>Corrected list order in case current result is out of range</returns>
        private int getNewPriority<T>( DbSet<T> dbset, int postedPriority, bool isNew)
            where T : OrderedTable
        {
            int maxPriority = dbset.OrderByDescending(d => d.List_Order).FirstOrDefault()?.List_Order ?? 1;
            if (isNew)
            {
                maxPriority += 1;
            }
            return Math.Min(postedPriority, maxPriority);
        }
    }
}