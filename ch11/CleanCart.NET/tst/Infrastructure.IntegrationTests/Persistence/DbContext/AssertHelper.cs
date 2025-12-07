using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Reflection;

namespace Infrastructure.IntegrationTests.Persistence.DbContext;

public static class AssertHelper
{
    public static void IsPrimaryKeyValid(IProperty? property)
    {
        Assert.NotNull(property);
        Assert.False(property.IsNullable);
        Assert.True(property.IsPrimaryKey());
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
    }

    public static void HasForeignKey<T>(IProperty? property)
    {
        Assert.NotNull(property);
        var foreignKeys = property.GetContainingForeignKeys().ToList();
        Assert.NotEmpty(foreignKeys);
        var foreignKey = foreignKeys.FirstOrDefault(x => x.PrincipalEntityType.Name == typeof(T).FullName && x.PrincipalKey.Properties.Any(y => y.Name == "Id"));
        Assert.NotNull(foreignKey);
    }

    public static void IsPropertyValid(
        IProperty? property,
        int? columnLength,
        int? columnOrder,
        bool isUniqueIndex = false,
        bool isNullable = false,
        ValueGenerated valueGenerated = ValueGenerated.Never,
        bool isPrimaryKey = false,
        string? columnType = null
    )
    {
        Assert.NotNull(property);
        Equal(valueGenerated, property.ValueGenerated, nameof(property.ValueGenerated));
        Equal(isNullable, property.IsNullable, nameof(property.IsNullable));
        Equal(columnLength, property.GetMaxLength(), "ColumnLength");
        Equal(columnOrder, property.GetColumnOrder(), "ColumnOrder");
        Equal(isUniqueIndex, property.IsUniqueIndex(), "UniqueIndex");
        Equal(isPrimaryKey, property.IsPrimaryKey(), "PrimaryKey");

        if (columnType == null) return;
        Equal(columnType, property.GetColumnType(), "ColumnType");
    }

    public static void Equal(object? expected, object? actual, string propertyName)
    {
        try
        {
            Assert.Equal(expected, actual);
        }
        catch (Exception)
        {
            throw new Exception($"Assert.Equal() Failure\nProperty: {propertyName}\nExpected: {expected}\nActual:   {actual}");
        }
    }

    public static void HasTwoColumnIndex(IProperty property, string columnName1, string columnName2, string? indexName = null)
    {
        var indexes = property!.GetContainingIndexes().ToList();
        Assert.NotEmpty(indexes);
        var index = indexes.FirstOrDefault(x => x.Properties.Any(y => y.Name == columnName1) && x.Properties.Any(y => y.Name == columnName2));
        Assert.NotNull(index);
        Equal(indexName, index.Name, "IndexName");
        Equal(2, index.Properties.Count, "IndexPropertyCount");
    }

    public static void PropertyNamesMatchColumnNames<TPropertyType>(IEntityType entityType)
    {
        IEnumerable<MemberInfo> memberInfos = typeof(TPropertyType).GetMembers().Where(x => x.MemberType == MemberTypes.Property);

        foreach (MemberInfo memberInfo in memberInfos)
        {
            if (memberInfo.Name.StartsWith("Nav")) continue;
            var property = entityType.FindProperty(memberInfo.Name);
            Assert.NotNull(property);
            Assert.Equal(memberInfo.Name, property.GetColumnName());
        }
    }
}
