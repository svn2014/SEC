using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Security
{
    public class ADatePeriod
    {
        public const int C_TimeSeriesExtendedDays = 90;
        public DateTime ReportDate;
        public DateTime TimeSeriesStart = DateTime.Today.AddDays(-8);
        public DateTime TimeSeriesStartExtended = DateTime.Today.AddDays(-8 - C_TimeSeriesExtendedDays);
        public DateTime TimeSeriesEnd = DateTime.Today.AddDays(-1);
        public virtual void SetDatePeriod(DateTime start, DateTime end)
        {
            if (end >= start)
            {
                this.TimeSeriesStart = start;
                this.TimeSeriesEnd = end;
            }
            else
            {
                this.TimeSeriesStart = end;
                this.TimeSeriesEnd = start;
            }

            //为了计算收益率等时间衍生数据,数据时间段向前推N日
            this.TimeSeriesStartExtended = this.TimeSeriesStart.AddDays(-C_TimeSeriesExtendedDays);
        }
    }
}
