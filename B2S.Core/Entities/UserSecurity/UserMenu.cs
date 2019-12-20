using System.ComponentModel.DataAnnotations.Schema;

namespace B2S.Core.Entities
{
    public partial class User
    {
        [NotMapped]
        public bool Workspace { get; set; } = false;
        [NotMapped]
        public bool Administration { get; set; } = false; 
    }

}
