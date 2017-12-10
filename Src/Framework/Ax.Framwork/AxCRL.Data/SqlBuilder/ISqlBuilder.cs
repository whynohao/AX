using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxCRL.Data.SqlBuilder
{
    /// <summary>
    /// 语法构造器接口
    /// </summary>
    public interface ISqlBuilder
    {
        /// <summary>
        /// 功能标识号
        /// </summary>
        string ProgId { get; set; }
        /// <summary>
        /// 返回查询语句,用于指定表头主键，查询一个功能所有字段
        /// </summary>
        /// <param name="pks">表头主键</param>
        /// <returns>返回查询语句</returns>
        string GetQueryAllSql(object[] pks);
        /// <summary>
        /// 返回查询语句
        /// </summary>
        /// <param name="tableIndex">表索引</param>
        /// <param name="selectFields">查询字段</param>
        /// <param name="where">查询条件</param>
        /// <returns>返回查询语句</returns>
        string GetQuerySql(int tableIndex, string selectFields, string where);
        /// <summary>
        /// 返回查询语句
        /// </summary>
        /// <param name="tableIndex">表索引</param>
        /// <param name="selectFields">查询字段</param>
        /// <param name="where">查询条件</param>
        /// <param name="sortCondition">排序条件</param>
        /// <returns>返回查询语句</returns>
        string GetQuerySql(int tableIndex, string selectFields, string where, string sortCondition);
        /// <summary>
        /// 返回查询语句
        /// </summary>
        /// <param name="tableIndex">表索引</param>
        /// <param name="selectFields">查询字段</param>
        /// <param name="where">查询条件</param>
        /// <param name="sortCondition">排序条件</param>
        /// <param name="groupCondition">分组条件</param>
        /// <param name="distinct"></param>
        /// <returns>返回查询语句</returns>
        string GetQuerySql(int tableIndex, string selectFields, string where, string sortCondition, string groupCondition, bool distinct);
    }
}
