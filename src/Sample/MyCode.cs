using System.Security.Cryptography;

namespace Kehlet.Generators.ConstantMethodGenerator;

public static class MyCode
{
    [ConstantMethod]
    public static string Calc()
    {
        var s = "";
        for (int i = 0; i < 10; i++)
        {
            s += "H";
        }

        return s;
    }

    [ConstantMethod]
    public static int Fib10()
    {
        static int Core(int n)
        {
            return n switch
            {
                0 => 1,
                1 => 1,
                _ => Core(n - 1) + Core(n - 2)
            };
        }

        return Core(10);
    }
}
