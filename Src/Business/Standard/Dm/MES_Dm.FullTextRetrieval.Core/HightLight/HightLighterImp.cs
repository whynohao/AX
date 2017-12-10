/**********************************************************************
 * CopyRight 2016 杭州集控科技有限公司 版权所有
 * 功能描述：关键字高亮
 * 创建标识：CHENQI 2016/12/01
 * 修改标识：
 * 修改描述：
************************************************************************/
using MES_Dm.FullTextRetrieval.Core.Model;
using PanGu;
using PanGu.HighLight;
using System;
using System.Collections.Generic;

namespace MES_Dm.FullTextRetrieval.Core.HightLight
{
    /// <summary>
    /// 关键字高亮功能的实现类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HightLighterImp : IHightLighter
    {
        //内容高亮时，选取片段的大小
        private static readonly int MAXFRAGMENTSIZE = 200;
        //保存需要高亮显示关键字的属性
        private HashSet<HightLightField> hightLightFields = null;

        /// <summary>
        /// 构造函数初始化，需要高亮的字段
        /// </summary>
        public HightLighterImp(){
            hightLightFields = new HashSet<HightLightField>();
            hightLightFields.Add(HightLightField.Content);
            //hightLightFields.Add(HightLightField.FileName);
        }

        /// <summary>
        /// 将关键字进行高亮显示
        /// </summary>
        /// <param name="keywords">属性和关键字的字典</param>
        /// <param name="t">高亮显示的对象</param>
        /// <returns></returns>
        public AbstractFileBase InitHightLight(Dictionary<string, string> keywords, AbstractFileBase t)
        {
            if(t == null)
            {
                throw new Exception("需要高亮显示的对象不存在");
            }
            SimpleHTMLFormatter simpleHTMLFormatter = new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");
            Highlighter highlighter = new PanGu.HighLight.Highlighter(simpleHTMLFormatter, new Segment());
            highlighter.FragmentSize = MAXFRAGMENTSIZE;
            foreach (var item in hightLightFields)
            {
                switch (item)
                {
                    //case HightLightField.FileName:
                    //    string fileName = string.Empty;
                    //    keywords.TryGetValue("fileName", out fileName);
                    //    if (!string.IsNullOrWhiteSpace(fileName))
                    //    {
                    //        t.FileName = highlighter.GetBestFragment(fileName, t.FileName);
                    //    }
                    //    break;
                    //case HightLightField.FilePath:
                    //    string filePath = string.Empty;
                    //    keywords.TryGetValue("filePath", out filePath);
                    //    if (!string.IsNullOrWhiteSpace(filePath))
                    //    {
                    //        t.FilePath = highlighter.GetBestFragment(filePath, t.FilePath);
                    //    }
                    //    break;
                    //case HightLightField.CreateTime:
                    //    string createTime = string.Empty;
                    //    keywords.TryGetValue("createTime", out createTime);
                    //    if (!string.IsNullOrWhiteSpace(createTime))
                    //    {
                    //        t.CreateTime = highlighter.GetBestFragment(createTime, t.CreateTime);
                    //    }
                    //    break;
                    case HightLightField.Content:
                        string content = string.Empty;
                        keywords.TryGetValue("content", out content);
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            //有问题
                            t.Content = highlighter.GetBestFragment(content, t.Content);
                        }
                        break;
                    //case HightLightField.UpLoadPersonId:
                    //    string upLoadPersonId = string.Empty;
                    //    keywords.TryGetValue("upLoadPersonId", out upLoadPersonId);
                    //    if (!string.IsNullOrWhiteSpace(upLoadPersonId))
                    //    {
                    //        //有问题
                    //        t.UpLoadPersonId = highlighter.GetBestFragment(upLoadPersonId, t.UpLoadPersonId);
                    //    }
                    //    break;
                }
            }
            return t;
        }

        /// <summary>
        /// 设置需要高亮显示的属性集合
        /// </summary>
        /// <param name="hightLightFieldsList"></param>
        public void SetHightLightFields(List<HightLightField> hightLightFieldsList)
        {
            foreach (var item in hightLightFieldsList)
            {
                hightLightFields.Add(item);
            }
        }
    }
}
