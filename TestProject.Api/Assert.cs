using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TestProject.Api;

public static class Assert
{
    [return: NotNull]
    public static T NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string? name = null)
        => value ?? throw new ArgumentNullException(name ?? nameof(value));
}