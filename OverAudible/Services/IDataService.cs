using AudibleApi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Services
{
    public interface IDataService<T>
    {
        Task<List<T>> GetAll();
        Task<List<(T, ContentMetadata)>> GetAllWithMetadata();

        Task<T> GetById(string id);

        Task<(T, ContentMetadata)> GetByIdWithMetadata(string id);

        Task<T> Create(T entity);

        Task<T> Update(string id,T entity);

        Task UpdateMetadata(string id, ContentMetadata extra);

        Task<bool> Delete(string id);

        Task<bool> DeleteAll();
    }

    
}
