using System.Linq.Expressions;

namespace Envelope.Queries.Includes;

public interface IIncludeDescriptorBuilder<TEntity> : IQueryModifier<TEntity>
	where TEntity : class
{
}

public interface IIncludeDescriptorBuilder<TEntity, TProperty> : IIncludeDescriptorBuilder<TEntity>, IQueryModifier<TEntity>
	where TEntity : class
{
	IIncludeDescriptorBuilder<TEntity, T> IncludeEnumerable<T>(Expression<Func<TEntity, IEnumerable<T>>> memberSelector);

	IIncludeDescriptorBuilder<TEntity, T> Include<T>(Expression<Func<TEntity, T>> memberSelector);

	IThenIncludeDescriptorBuilder<TEntity, TProperty, TNextProperty> ThenIncludeEnumerable<TNextProperty>(Expression<Func<TProperty, IEnumerable<TNextProperty>>> memberSelector);

	IThenIncludeDescriptorBuilder<TEntity, TProperty, TNextProperty> ThenInclude<TNextProperty>(Expression<Func<TProperty, TNextProperty>> memberSelector);
}
