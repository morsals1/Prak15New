using Prak15Mensh.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prak15Mensh.Service
{
    class DBService
    {
        private Pract15DbContext context;
        public Pract15DbContext Context => context;
        private static DBService? instance;
        public static DBService Instance
        {
            get
            {
                if (instance == null)
                    instance = new DBService();
                return instance;
            }
        }

        private DBService()
        {
            context = new Pract15DbContext();
        }
    }
}
