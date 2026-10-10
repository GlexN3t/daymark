using Daymark.Api.Services;
using Xunit;

namespace Daymark.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_then_verify_roundtrips()
    {
        var hash = PasswordHasher.Hash("correct horse battery staple");

        Assert.True(PasswordHasher.Verify("correct horse battery staple", hash));
    }

    [Fact]
    public void Verify_rejects_a_wrong_password()
    {
        var hash = PasswordHasher.Hash("the-real-password");

        Assert.False(PasswordHasher.Verify("the-wrong-password", hash));
    }

    [Fact]
    public void Same_password_produces_different_hashes()
    {
        Assert.NotEqual(PasswordHasher.Hash("same"), PasswordHasher.Hash("same"));
    }

    [Fact]
    public void Verify_rejects_malformed_hashes()
    {
        Assert.False(PasswordHasher.Verify("anything", "not-a-hash"));
        Assert.False(PasswordHasher.Verify("anything", "a.b.c"));
        Assert.False(PasswordHasher.Verify("anything", "nonsense.nonsense.nonsense"));
    }
}
