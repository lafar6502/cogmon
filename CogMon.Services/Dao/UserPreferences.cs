using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CogMon.Lib;

namespace CogMon.Services.Dao
{
    public class UserPreferences
    {
        public virtual int Id { get; set; }
        public virtual UserInfo Owner { get; set; }

    }
}
