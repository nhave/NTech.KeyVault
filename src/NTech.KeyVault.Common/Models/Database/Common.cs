namespace NTech.KeyVault.Common.Models.Database
{
    public class Common
    {
        /// <summary>
        /// The unique identifier for the entity. This property is used to uniquely identify each record in the database and is typically generated as a GUID (Globally Unique Identifier) to ensure uniqueness across different systems and databases.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The date and time when the entity was created. This property is used to track when a record was initially created in the database. It is typically set to the current date and time when a new record is inserted into the database, allowing for auditing and historical tracking of data changes.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// The date and time when the entity was last updated. This property is used to track when a record was last modified in the database. It is typically updated to the current date and time whenever the record is updated, allowing for auditing and historical tracking of data changes.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
