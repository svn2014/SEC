
namespace Security
{
    public class RepoGroup: ASecurityGroup
    {
        #region 基础方法
        public RepoGroup() : base(typeof(Repo)) { }
        public override void LoadData(DataInfoType type)
        {
            //空
        }
        #endregion
    }
}
