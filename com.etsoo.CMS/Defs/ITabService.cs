using com.etsoo.CMS.RQ.Tab;
using com.etsoo.CoreFramework.Models;
using com.etsoo.CoreFramework.Services;
using com.etsoo.Utils.Actions;
using System.Net;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Website tab service interface
    /// 网站栏目业务逻辑服务接口
    /// </summary>
    public interface ITabService : IServiceBase
    {
        /// <summary>
        /// Create tab
        /// 创建栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        Task<IActionResult> CreateAsync(TabCreateRQ rq, IPAddress ip);

        /// <summary>
        /// Delete single user
        /// 删除单个用户
        /// </summary>
        /// <param name="id">User id</param>
        /// <returns>Action result</returns>
        ValueTask<IActionResult> DeleteAsync(int id);

        /// <summary>
        /// List
        /// 列表
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        Task ListAsync(TiplistRQ<int> rq, HttpResponse response);

        /// <summary>
        /// Query tab
        /// 查询栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="response">Response</param>
        /// <returns>Task</returns>
        Task QueryAsync(TabQueryRQ rq, HttpResponse response);

        /// <summary>
        /// Sort data
        /// 数据排序
        /// </summary>
        /// <param name="sortData">Data to sort</param>
        /// <returns>Rows affected</returns>
        Task<int> SortAsync(Dictionary<int, short> sortData);

        /// <summary>
        /// Update tab
        /// 更新栏目
        /// </summary>
        /// <param name="rq">Request data</param>
        /// <param name="ip">IP address</param>
        /// <returns>Result</returns>
        Task<IActionResult> UpdateAsync(TabUpdateRQ rq, IPAddress ip);

        /// <summary>
        /// Read for updae
        /// 更新浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        Task UpdateReadAsync(int id, HttpResponse response);

        /// <summary>
        /// Read for ancestors
        /// 上层栏目浏览
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="response">HTTP Response</param>
        /// <returns>Task</returns>
        Task AncestorReadAsync(int id, HttpResponse response);
    }
}
