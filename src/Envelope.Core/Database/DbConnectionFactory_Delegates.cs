using System.Data.Common;

namespace Envelope.Database;

public delegate DbConnection DbConnectionFactory(string connectionId);

public delegate Task<DbConnection> DbConnectionFactoryAsync(
		string connectionId,
		CancellationToken cancellationToken = default);
