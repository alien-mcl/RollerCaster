using System.Diagnostics.CodeAnalysis;

namespace RollerCaster.Data
{
    public interface IMethodCarrier
    {
        bool Property { get; }

        bool InvalidProperty { get; set; }

        void Action();

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        void Action(int param);

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        void Action(int param1, int param2);

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        void Action(int param1, int param2, int param3);

        [SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Justification = "This is a test subject only.")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        void Action(int param1, int param2, int param3, int param4);

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        [SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Justification = "This is a test subject only.")]
        void Action(int param1, int param2, int param3, int param4, int param5);

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        [SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Justification = "This is a test subject only.")]
        void Action(int param1, int param2, int param3, int param4, int param5, int param6);

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        [SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Justification = "This is a test subject only.")]
        void Action(int param1, int param2, int param3, int param4, int param5, int param6, int param7);

        bool Func();

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        bool Func(int param);

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        bool Func(int param1, int param2);

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        bool Func(int param1, int param2, int param3);

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        [SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Justification = "This is a test subject only.")]
        bool Func(int param1, int param2, int param3, int param4);

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        [SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Justification = "This is a test subject only.")]
        bool Func(int param1, int param2, int param3, int param4, int param5);

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        [SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Justification = "This is a test subject only.")]
        bool Func(int param1, int param2, int param3, int param4, int param5, int param6);

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Parameter name is not a hungarian notation.")]
        [SuppressMessage("Microsoft.Design", "CA1025:ReplaceRepetitiveArgumentsWithParamsArray", Justification = "This is a test subject only.")]
        bool Func(int param1, int param2, int param3, int param4, int param5, int param6, int param7);
    }
}