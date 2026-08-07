using Xunit;

namespace Ordis.Tests;

/// <summary>
/// Unit tests for the pure account-id helpers that enforce the 3-digit (0-999)
/// cap guaranteeing test ids can never collide with real Discord snowflakes.
/// </summary>
public class TestAccountTests
{
    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("42", 42)]
    [InlineData("999", 999)]
    public void TryParseId_AcceptsThreeDigitIntegers(string raw, int expected)
    {
        Assert.True(TestAccount.TryParseId(raw, out var id));
        Assert.Equal(expected, id);
        Assert.True(TestAccount.IsValidId(id));
    }

    [Theory]
    [InlineData("1000")]                  // 4 digits — just past the cap
    [InlineData("123456789012345678")]    // real-style 18-digit Discord snowflake
    [InlineData("1234")]                  // any 4-digit value
    [InlineData("-1")]                    // negative
    [InlineData("+5")]                    // leading sign
    [InlineData("5.0")]                   // non-integer
    [InlineData("abc")]                   // non-numeric
    [InlineData("")]                      // empty
    [InlineData(null)]                    // null
    public void TryParseId_RejectsOutOfRangeOrNonNumeric(string? raw)
    {
        Assert.False(TestAccount.TryParseId(raw, out var id));
        Assert.Equal(0, id);
    }

    [Fact]
    public void NextFreeId_ReturnsSmallestUnusedId()
    {
        Assert.Equal(0, TestAccount.NextFreeId(new HashSet<int>()));
        Assert.Equal(2, TestAccount.NextFreeId(new HashSet<int> { 0, 1, 3 }));
        // 0 is a valid id and is unused here, so it is the smallest free id.
        Assert.Equal(0, TestAccount.NextFreeId(new HashSet<int> { 1, 2, 3 }));
        Assert.Equal(4, TestAccount.NextFreeId(new HashSet<int> { 0, 1, 2, 3 }));
    }

    [Fact]
    public void NextFreeId_ReturnsNullWhenSpaceExhausted()
    {
        var all = new HashSet<int>(Enumerable.Range(TestAccount.MinId, TestAccount.MaxId - TestAccount.MinId + 1));
        Assert.Null(TestAccount.NextFreeId(all));
    }

    [Fact]
    public void IsValidId_HonoursRangeBoundaries()
    {
        Assert.True(TestAccount.IsValidId(TestAccount.MinId));
        Assert.True(TestAccount.IsValidId(TestAccount.MaxId));
        Assert.False(TestAccount.IsValidId(TestAccount.MinId - 1));
        Assert.False(TestAccount.IsValidId(TestAccount.MaxId + 1));
    }
}
