
namespace Security
{
    public class RealtimeNetAssetValue : ARealTimeSeries
    {
        public double PreCloseNAV = 0;
        public double PreCloseAccumNAV = 0;
        
        public double RealtimeNAV = 0;//估算值
        public double RealtimeUpDown = 0;

        public bool IsUpdated = false;
    }
}
