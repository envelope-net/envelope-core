using System.Linq.Expressions;

namespace Envelope.Queries.Includes;

public interface IThenIncludeDescriptorBuilder<TEntity, out TProperty> : IQueryModifier<TEntity>
	where TEntity : class
{
}

public interface IThenIncludeDescriptorBuilder<TEntity, TProperty, TNextProperty> : IThenIncludeDescriptorBuilder<TEntity, TProperty>, IQueryModifier<TEntity>
	where TEntity : class
{
	IIncludeDescriptorBuilder<TEntity, T> IncludeEnumerable<T>(Expression<Func<TEntity, IEnumerable<T>>> memberSelector);

	IIncludeDescriptorBuilder<TEntity, T> Include<T>(Expression<Func<TEntity, T>> memberSelector);

	IThenIncludeDescriptorBuilder<TEntity, TNextProperty, TNextNestedProperty> ThenIncludeEnumerable<TNextNestedProperty>(Expression<Func<TNextProperty, IEnumerable<TNextNestedProperty>>> memberSelector);

	IThenIncludeDescriptorBuilder<TEntity, TNextProperty, TNextNestedProperty> ThenInclude<TNextNestedProperty>(Expression<Func<TNextProperty, TNextNestedProperty>> memberSelector);
}
