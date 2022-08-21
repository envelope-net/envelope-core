using System.Data.Common;

namespace Envelope.Database;

public delegate DbConnection DbConnectionFactory();

public delegate Task<DbConnection> DbConnectionFactoryAsync(CancellationToken cancellationToken = default);
