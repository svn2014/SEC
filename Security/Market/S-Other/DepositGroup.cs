
namespace Security
{
    public class DepositGroup: ASecurityGroup
    {
        #region 基础方法
        public DepositGroup() : base(typeof(Deposit)) { }
        public override void LoadData(DataInfoType type)
        {
            //空
        }
        #endregion
    }
}
