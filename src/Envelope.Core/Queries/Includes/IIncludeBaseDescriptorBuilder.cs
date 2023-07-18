using System.Linq.Expressions;

namespace Envelope.Queries.Includes;

public interface IIncludeBaseDescriptorBuilder<TEntity> : IQueryModifier<TEntity>
	where TEntity : class
{
	IIncludeDescriptorBuilder<TEntity, TProperty> IncludeEnumerable<TProperty>(Expression<Func<TEntity, IEnumerable<TProperty>>> memberSelector);

	IIncludeDescriptorBuilder<TEntity, TProperty> Include<TProperty>(Expression<Func<TEntity, TProperty>> memberSelector);
}
