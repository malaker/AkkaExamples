﻿using Dapper;
using MediatR;
using Shared.Interfaces;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
    public class InsertOrUpdateCommandHandler : IRequestHandler<InsertOrUpdateSomeContract>
    {
        private ISqlConnectionProvider provider;

        public InsertOrUpdateCommandHandler(ISqlConnectionProvider sqlProvider)
        {
            this.provider = sqlProvider;
        }

        public async Task<Unit> Handle(InsertOrUpdateSomeContract request, CancellationToken cancellationToken)
        {
            using (var conn = new SqlConnection(this.provider.Provide()))
            {
                request.Data.ForEach(r =>
                {
                    conn.Execute(@"
                        UPDATE SomeContract
                        SET Content = @MyXML , ModifiedOn = GETUTCDATE()
                        WHERE Id = @Id AND ModifiedOn < @Timestamp
                        IF @@ROWCOUNT = 0
                            INSERT INTO SomeContract (Id,Content,ModifiedOn)
                            VALUES(@Id, @MyXML, @Timestamp)

                    ", r);
                });
            }

            return await Unit.Task;
        }
    }
}