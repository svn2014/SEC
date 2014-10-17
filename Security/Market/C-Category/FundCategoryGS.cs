using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Security
{
    public class FundCategoryGS: AFundCategory
    {
        public new string CategoryName = "银河证券基金分类体系（2013）";
        //参考：http://fund.chinastock.com.cn/yhwz/fund/info.do?methodCall=studyCenter&target=0&id=rule&docId=3298864

        #region 基础方法
        public override void SetupCategory(string text, int level)
        {
            switch (level)
            {
                case 1:
                case 2:
                    throw new NotImplementedException();
                case 3:
                    updateCategory3(text);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        private void updateCategory3(string text)
        {
            base.Name3 = text;
            base.OperationCategory = FundOperationCategory.OpenEnd;
            base.InvestmentCategory = FundInvestmentCategory.Active;
            base.StructureCategory = FundStructureCategory.Parent;

            switch (text)
            {
                case "标准股票型基金":
                case "普通股票型基金":
                    base.AssetCategory = FundAssetCategory.Equity;
                    base.Name1 = "股票基金";
                    base.Name2 = "股票型基金";
                    break;
                case "标准指数股票型基金":
                case "增强指数股票型基金":
                case "标准指数型股票基金":
                case "增强指数型股票基金":                    
                case "ETF联接基金":
                    base.InvestmentCategory = FundInvestmentCategory.Passive;
                    base.AssetCategory = FundAssetCategory.Equity;
                    base.Name1 = "股票基金";
                    base.Name2 = "指数股票型基金";
                    break;
                case "股票型分级子基金(优先份额)":
                case "股票型分级子基金(进取份额)":
                    base.StructureCategory = FundStructureCategory.Child;
                    base.AssetCategory = FundAssetCategory.Equity;
                    base.Name1 = "股票基金";
                    base.Name2 = "股票型分级子基金";
                    break;
                case "偏股型基金(股票上限95%)":
                case "偏股型基金(股票上限80%)":
                    base.AssetCategory = FundAssetCategory.Hybrid;
                    base.Name1 = "混合基金";                    
                    base.Name2 = "偏股型基金";    
                    break;
                case "灵活配置型基金(股票上限95%)":
                case "灵活配置型基金(股票上限80%)":
                    base.AssetCategory = FundAssetCategory.Hybrid;
                    base.Name1 = "混合基金"; 
                    base.Name2 = "灵活配置型基金";
                    break;
                case "股债平衡型基金":
                    base.AssetCategory = FundAssetCategory.Hybrid;
                    base.Name1 = "混合基金"; 
                    base.Name2 = "股债平衡型基金";
                    break;
                case "偏债型基金":
                case "保本型基金":
                case "特定策略混合型基金":
                    base.AssetCategory = FundAssetCategory.Hybrid;
                    base.Name1 = "混合基金"; 
                    base.Name2 = base.Name3;
                    break;
                case "长期标准债券型基金(A类)":
                case "长期标准债券型基金(B/C类)":
                case "中短期标准债券型基金(A类)":
                case "中短期标准债券型基金(B类)":
                    base.AssetCategory = FundAssetCategory.Bond;
                    base.Name1 = "债券基金";                     
                    base.Name2 = "标准债券型基金";
                    break;
                case "普通债券型基金(一级A类)":
                case "普通债券型基金(一级B/C类)":
                case "普通债券型基金(二级A类)":
                case "普通债券型基金(二级B/C类)":            
                    base.AssetCategory = FundAssetCategory.Bond;
                    base.Name1 = "债券基金";  
                    base.Name2 = "普通债券型基金";
                    break;
                case "可转换债券型基金(A类)":
                case "可转换债券型基金(B/C类)":
                    base.AssetCategory = FundAssetCategory.Bond;
                    base.Name1 = "债券基金";  
                    base.Name2 = "可转换债券型基金";
                    break;
                case "指数债券型基金(A类)":
                case "指数债券型基金(B/C类)":
                    base.InvestmentCategory = FundInvestmentCategory.Passive;
                    base.AssetCategory = FundAssetCategory.Bond;
                    base.Name1 = "债券基金";  
                    base.Name2 = "指数债券型基金";
                    break;
                case "债券型分级子基金(优先份额)":
                case "债券型分级子基金(进取份额)":
                    base.StructureCategory = FundStructureCategory.Child;
                    base.AssetCategory = FundAssetCategory.Bond;
                    base.Name1 = "债券基金";  
                    base.Name2 = "债券型分级子基金";
                    break;
                case "短期理财债券型基金(A类)":
                case "短期理财债券型基金(B类)":
                    base.AssetCategory = FundAssetCategory.Bond;
                    base.Name1 = "债券基金";  
                    base.Name2 = "短期理财债券型基金";
                    break;
                case "货币市场基金(A类)":
                case "货币市场基金(B类)":
                    base.AssetCategory = FundAssetCategory.Monetory;
                    base.Name1 = "货币市场基金";
                    base.Name2 = base.Name3;
                    break;
                case "QDII全球股票型基金":
                case "QDII亚太股票型基金":
                case "QDII新兴市场股票型基金":
                case "QDII成熟市场股票型基金":
                case "QDII大中华股票型基金":
                case "QDII全球股票基金":
                case "QDII亚太股票基金":
                case "QDII新兴市场股票基金":
                case "QDII成熟市场股票基金":
                case "QDII大中华股票基金":
                    base.AssetCategory = FundAssetCategory.QDII;
                    base.Name1 = "QDII基金";                    
                    base.Name2 = "QDII股票基金";
                    break;
                case "QDII指数股票型基金":
                case "QDII指数型股票基金":
                case "QDII-ETF联接基金":
                    base.InvestmentCategory = FundInvestmentCategory.Passive;
                    base.AssetCategory = FundAssetCategory.QDII;
                    base.Name1 = "QDII基金";                    
                    base.Name2 = "QDII股票基金";
                    break;                    
                case "QDII混合基金":
                case "QDII债券基金":
                case "QDII商品基金":
                    base.AssetCategory = FundAssetCategory.QDII;
                    base.Name1 = "QDII基金";
                    base.Name2 = base.Name3;
                    break;
                case "封闭式标准股票型基金":
                case "封闭式普通股票型基金":
                    base.OperationCategory = FundOperationCategory.CloseEnd;
                    base.AssetCategory = FundAssetCategory.Equity;
                    base.Name1 = "封闭式股票基金";
                    base.Name2 = "封闭式股票型基金";
                    break;                    
                case "封闭式标准指数型股票基金":
                    base.InvestmentCategory = FundInvestmentCategory.Passive;
                    base.OperationCategory = FundOperationCategory.CloseEnd;
                    base.AssetCategory = FundAssetCategory.Equity;
                    base.Name1 = "封闭式股票基金";
                    base.Name2 = "封闭式指数型股票基金";
                    break;                    
                case "封闭式股票型分级子基金(优先份额)":
                case "封闭式股票型分级子基金(进取份额)":
                    base.StructureCategory = FundStructureCategory.Child;
                    base.OperationCategory = FundOperationCategory.CloseEnd;
                    base.AssetCategory = FundAssetCategory.Equity;
                    base.Name1 = "封闭式股票基金";
                    base.Name2 = "封闭式股票型分级子基金";
                    break;
                case "封闭式长期标准债券型基金":
                    base.OperationCategory = FundOperationCategory.CloseEnd;
                    base.AssetCategory = FundAssetCategory.Bond;
                    base.Name1 = "封闭式债券基金";
                    base.Name2 = "封闭式标准债券型基金";
                    break;                    
                case "封闭式普通债券型基金(一级)":
                case "封闭式普通债券型基金(二级)":
                    base.OperationCategory = FundOperationCategory.CloseEnd;
                    base.AssetCategory = FundAssetCategory.Bond;
                    base.Name1 = "封闭式债券基金";
                    base.Name2 = "封闭式普通债券基金";
                    break;                    
                case "封闭式债券型分级子基金(优先份额)":
                case "封闭式债券型分级子基金(进取份额)":
                    base.StructureCategory = FundStructureCategory.Child;
                    base.OperationCategory = FundOperationCategory.CloseEnd;
                    base.AssetCategory = FundAssetCategory.Bond;
                    base.Name1 = "封闭式债券基金";
                    base.Name2 = "封闭式债券型分级子基金";
                    break;
                case "":
                    base.Name1 = "未分类";
                    base.Name2 = "未分类";
                    base.Name3 = "未分类";
                    base.AssetCategory = FundAssetCategory.Undefined;
                    base.OperationCategory = FundOperationCategory.Undefined;
                    base.StructureCategory = FundStructureCategory.Undefined;
                    base.InvestmentCategory = FundInvestmentCategory.Undefined;
                    break;
                default:
                    base.AssetCategory = FundAssetCategory.Other;
                    MessageManager.GetInstance().AddMessage(MessageType.Warning, Message.C_Msg_MF6, text);
                    break;
            }
        }
        #endregion
    }
}
