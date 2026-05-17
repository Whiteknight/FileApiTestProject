using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TestProject.Api;

public static class Assert
{
    /* Note: Defensive Programming
     * 
     * ArgumentNullException.ThrowIfNull() is more idiomatic for this situation, but I find having
     * the return value means we can include our check in-line as part of an expression which is
     * often a win for readability and conciseness. 
     * 
     * While not needed here, additional assert methods for defensive programming scenarios could
     * also be added here and then all included in one place, which ArgumentNullException
     * cannot do.
     */
    [return: NotNull]
    public static T NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string? name = null)
        => value ?? throw new ArgumentNullException(name ?? nameof(value));
}