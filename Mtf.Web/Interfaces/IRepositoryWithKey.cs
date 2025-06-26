using System.Collections.Generic;

namespace Mtf.Web.Interfaces
{
    public interface IRepositoryWithKey<TModel, TKey>
    {
        IEnumerable<TModel> SelectAll();

        TModel SelectByKey(TKey key);

        void Insert(TModel model);

        void Update(TModel model);

        void DeleteByKey(TKey key);
    }
}
