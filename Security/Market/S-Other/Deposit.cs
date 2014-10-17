using System;

namespace Security
{
    public class Deposit: ASecurity
    {
        public const string C_Code_CurrentDeposit = "CURRNT";   //活期
        public const string C_Code_TimeDeposit = "TIMEDP";      //定期
        public const string C_Code_ClearDeposit = "CLERDP";     //清算备付金
        public const string C_Code_ClearMargin = "MARGIN";      //结算保证金
        public const string C_Code_ClearSettlement = "SETTLE";  //证券清算款

        public Deposit(string code) : base(code) { }
        public override void LoadData(DataInfoType type)
        {
            //空
        }
        protected override void BuildHistoryObjects(string code, DateTime start, DateTime end)
        {
            //空
        }
        protected override void BuildSecurity()
        {
            base.Type = SecurityType.Deposit;
            base.Exchange = SecExchange.OTC;
        }
    }
}
