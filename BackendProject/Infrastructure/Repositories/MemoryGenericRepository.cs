using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppCore.Models;
using AppCore.Repositories;
using AppCore.Dto;

namespace Infrastructure.Repositories;

public class MemoryGenericRepository<T> : IGenericRepositoryAsync<T> where T : EntityBase
{
    protected readonly Dictionary<Guid, T> _data = new();

    public Task<T?> FindByIdAsync(Guid id)
    {
        _data.TryGetValue(id, out var result);
        return Task.FromResult(result);
    }

    public Task<IEnumerable<T>> FindAllAsync()
    {
        return Task.FromResult<IEnumerable<T>>(_data.Values.ToList());
    }

    public Task<PagedResult<T>> FindPagedAsync(int page, int pageSize)
    {
        var totalCount = _data.Count;
        var items = _data.Values
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<T>(items, totalCount, page, pageSize));
    }

    public Task<T> AddAsync(T entity)
    {
        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }
        _data[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task<T> UpdateAsync(T entity)
    {
        if (!_data.ContainsKey(entity.Id))
        {
            throw new KeyNotFoundException($"Entity with id {entity.Id} not found.");
        }
        _data[entity.Id] = entity;
        return Task.FromResult(entity);
    }

    public Task RemoveByIdAsync(Guid id)
    {
        if (!_data.ContainsKey(id))
        {
            throw new KeyNotFoundException($"Entity with id {id} not found.");
        }
        _data.Remove(id);
        return Task.CompletedTask;
    }
}
