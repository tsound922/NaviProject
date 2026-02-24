using Dapper;
using Pgvector;
using System.Data;

namespace NaviProject.Infrastructure.TypeHandlers;

public class VectorTypeHandler : SqlMapper.TypeHandler<Vector>
{
    public override void SetValue(IDbDataParameter parameter, Vector? value)
    {
        parameter.Value = value?.ToString();
    }

    public override Vector Parse(object value)
    {
        return new Vector(value.ToString()!);
    }
}