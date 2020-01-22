using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF
{
    public class MultiAccountException
    {
        #region Properties

        public virtual Account Account { get; set; }

        public long AccountId { get; set; }

        [Key]
        public long ExceptionId { get; set; }

        public byte ExceptionLimit { get; set; }

        #endregion
    }
}