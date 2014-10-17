using System;

namespace Security
{
    public class Repo: ASecurity
    {
        public Repo(string code) : base(code) { base.Type = SecurityType.RevRepo; }
        public Repo(string code, SecExchange exchange) : base(code) { base.Exchange = exchange; base.Type = SecurityType.RevRepo; }
        public Repo(string code, SecurityType type): base(code){ base.Type = type; }
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
            string code3 = this.Code.Substring(0, 3);
            switch (code3)
            {
                case "204":
                    base.Exchange = SecExchange.SHE;
                    break;
                case "131":
                    base.Exchange = SecExchange.SZE;
                    break;
                default:
                    base.Exchange = SecExchange.IBM;
                    break;
            }
        }
    }
}
