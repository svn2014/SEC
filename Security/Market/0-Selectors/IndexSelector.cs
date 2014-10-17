using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Security
{
    public class IndexSelector
    {
        public IndexGroup GetSWIndustryIndex()
        {
            //申万一级行业指数
            IndexGroup ig = new IndexGroup();
            ig.Name = "申万一级行业指数";
            ig.Add("801010");
            ig.Add("801020");
            ig.Add("801030");
            ig.Add("801040");
            ig.Add("801050");
            ig.Add("801060");
            ig.Add("801070");
            ig.Add("801080");
            ig.Add("801090");
            ig.Add("801100");
            ig.Add("801110");
            ig.Add("801120");
            ig.Add("801130");
            ig.Add("801140");
            ig.Add("801150");
            ig.Add("801160");
            ig.Add("801170");
            ig.Add("801180");
            ig.Add("801190");
            ig.Add("801200");
            ig.Add("801210");
            ig.Add("801220");
            ig.Add("801230");
            return ig;
        }
        public IndexGroup GetZXIndustryIndex()
        {
            //中信一级行业指数
            IndexGroup ig = new IndexGroup();
            ig.Name = "中信一级行业指数";
            ig.Add("817000");
            ig.Add("817001");
            ig.Add("817002");
            ig.Add("817003");
            ig.Add("817004");
            ig.Add("817005");
            ig.Add("817006");
            ig.Add("817007");
            ig.Add("817008");
            ig.Add("817009");
            ig.Add("817010");
            ig.Add("817011");
            ig.Add("817012");
            ig.Add("817013");
            ig.Add("817015");
            ig.Add("817014");
            ig.Add("817016");
            ig.Add("817017");
            ig.Add("817018");
            ig.Add("817019");
            ig.Add("817020");
            ig.Add("817021");
            ig.Add("817022");
            ig.Add("817023");
            ig.Add("817024");
            ig.Add("817025");
            ig.Add("817026");
            ig.Add("817027");
            ig.Add("817028");
            return ig;
        }
        public IndexGroup GetJCIndustryIndex()
        {
            //巨潮1000行业指数
            IndexGroup ig = new IndexGroup();
            ig.Name = "巨潮1000行业指数";
            ig.Add("399381");
            ig.Add("399382");
            ig.Add("399383");
            ig.Add("399384");
            ig.Add("399385");
            ig.Add("399386");
            ig.Add("399387");
            ig.Add("399388");
            ig.Add("399389");
            ig.Add("399390");
            return ig;
        }
        public IndexGroup GetZZSizeIndex()
        {
            //中证规模指数（100，200，500）
            IndexGroup ig = new IndexGroup();
            ig.Name = "中证规模指数和创业板指";
            ig.Add("000903");
            ig.Add("000904");
            ig.Add("000905");
            ig.Add("399006");
            return ig;
        }
        public IndexGroup GetJCSizeIndex()
        {
            //巨潮规模指数（大，中，小)
            IndexGroup ig = new IndexGroup();
            ig.Name = "巨潮规模指数";
            ig.Add("399314");
            ig.Add("399315");
            ig.Add("399316");
            return ig;
        }
        public IndexGroup GetJCStyleIndex()
        {
            //巨潮风格指数（大/中/小，成长/价值）
            IndexGroup ig = new IndexGroup();
            ig.Name = "巨潮风格指数";
            ig.Add("399372");
            ig.Add("399373");
            ig.Add("399374");
            ig.Add("399375");
            ig.Add("399376");
            ig.Add("399377");
            return ig;
        }
        public IndexGroup GetSWProfitIndex()
        {
            //申万盈利能力指数
            IndexGroup ig = new IndexGroup();
            ig.Name = "申万盈利能力指数";
            ig.Add("801851");
            ig.Add("801852");
            ig.Add("801853");
            return ig;
        }
    }
}
