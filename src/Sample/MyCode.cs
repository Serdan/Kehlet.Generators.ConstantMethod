namespace Kehlet.Generators.ConstantMethodGenerator;

public static partial class MyCode
{
    [ConstantMethod(nameof(Calc))]
    public static partial string CalcConstant();

    public static string Calc()
    {
        var s = "";
        for (int i = 0; i < 10; i++)
        {
            s += "H";
        }

        return s;
    }

    [ConstantMethod(nameof(Fib10), 10)]
    public static partial int FibConstant();

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

    [ConstantMethod(nameof(FloatingPointCalc))]
    public static partial float FloatingPointConstant();

    public static float FloatingPointCalc()
    {
        var result = 0f;
        for (int i = 0; i < 10; i++)
        {
            result += 0.1f;
        }

        return result;
    }
}
