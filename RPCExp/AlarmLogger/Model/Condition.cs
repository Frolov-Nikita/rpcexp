using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.AlarmLogger.Model
{
    public class Condition
    {//see ArcControlller
        public Condition(string str)
        {
            FromString(str);
        }

        void FromString(string str)
        {

        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
