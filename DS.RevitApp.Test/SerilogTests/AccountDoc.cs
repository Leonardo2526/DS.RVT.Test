using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unidecode.NET;

namespace DS.RevitApp.Test.SerilogTests
{
    internal class AccountDoc
    {
        public AccountDoc(int elem1Id, int elem2Id, string status, string revitDocName, string revitLinkName = "")
        {
            this.Elem1Id = elem1Id;
            this.Elem2Id = elem2Id;
            this.Status = status;
            this.RevitDocName = revitDocName;
            this.RevitLinkName = revitLinkName;
        }

        public string Date { get; set; }
        public string UserName { get; set; } = Environment.UserName.Unidecode();
        public string UserCompName { get; set; } = Environment.MachineName;
        public string RevitDocName { get; set; }
        public string RevitLinkName { get; set; }
        public int Elem1Id { get; set; }
        public int Elem2Id { get; set; }
        public string Status { get; set; }
    }
}
