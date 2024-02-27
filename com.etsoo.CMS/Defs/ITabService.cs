using com.etsoo.CMS.Models;
using com.etsoo.CMS.RQ.Tab;
using com.etsoo.CoreFramework.Models;
using com.etsoo.Utils.Actions;

namespace com.etsoo.CMS.Defs
{
    /// <summary>
    /// Website tab service interface
    /// 网站栏目业务逻辑服务接口
    /// </summary>
    public interface ITabService : ICommonService
    {
        Task<IActionResult> CreateAsync(TabCreateRQ rq, CancellationToken cancellationToken = default);

        ValueTask<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task ListAsync(TiplistRQ<int> rq, HttpResponse response, CancellationToken cancellationToken = default);

        Task QueryAsync(TabQueryRQ rq, HttpResponse response, CancellationToken cancellationToken = default);

        Task<int> SortAsync(Dictionary<int, short> sortData, CancellationToken cancellationToken = default);

        Task<IActionResult> UpdateAsync(TabUpdateRQ rq, CancellationToken cancellationToken = default);

        ValueTask<string?> UploadLogoAsync(int id, Stream logoStream, string contentType, CancellationToken cancellationToken = default);

        Task UpdateReadAsync(int id, HttpResponse response, CancellationToken cancellationToken = default);

        Task<ParentTab[]> AncestorReadAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

        Task AncestorReadAsync(int id, HttpResponse response, CancellationToken cancellationToken = default);
    }
}
