using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocprocShared
{
    class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string Message) : base(Message)
        {
            
        }
    }
}
