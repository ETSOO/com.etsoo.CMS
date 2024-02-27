using com.etsoo.SourceGenerators;
using com.etsoo.SourceGenerators.Attributes;

namespace com.etsoo.CMS.Models
{
    [SqlDeleteCommand("articles", NamingPolicy.SnakeCase, Database = DatabaseName.SQLite)]
    public partial record SqlArticleDelete
    {
        public int Id { get; init; }
        public byte Status { get; init; }
    }
}