using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Publishing.FileSystemProvider.Schema
{
    public static class Queries
    {
        public static readonly string IdTableByIdAndPrefixQuery =
            @" 
            SELECT IDTable.ID, ParentID, [Key]
            FROM    IDTable
            INNER JOIN @Ids Ids
            ON IDTable.Id = Ids.Id
            WHERE   IDTable.Prefix = @Prefix";

        public static readonly string IdTableByPrefixQuery =
        @" 
            SELECT ID, ParentID, [Key]
            FROM    IDTable
            WHERE   IDTable.Prefix = @Prefix";
    }
}
