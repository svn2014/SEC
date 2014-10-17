using System;

namespace Security
{
    public abstract class ARealTimeSeries
    {
        public string DataSource;
        public DateTime TimeStamp;
        public string Code = "";
        public string Name = "";
    }
}
