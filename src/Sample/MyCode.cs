namespace Kehlet.Generators.ConstantMethod.Sample;

public static partial class MyCode
{
    public static string RepeatedCharacter(char character, int repetitions)
    {
        var s = "";
        for (int i = 0; i < repetitions; i++)
        {
            s += character;
        }
    
        return s;
    }
    
    [ConstantMethod(nameof(RepeatedCharacter), 'H', 10)]
    public static partial string RepeatedHs();
    
    public static int Fib(int n)
    {
        return n switch
        {
            0 => 1,
            1 => 1,
            _ => Fib(n - 1) + Fib(n - 2)
        };
    }
    
    [ConstantMethod(nameof(Fib), 11)]
    public static partial int FibConstant();
    
    public static int Sum(int start, int count) => Enumerable.Range(start, count).Sum();
    
    [ConstantMethod(nameof(Sum), 1, 100)]
    public static partial int Sum100();

    public static int SumArray(int[] arr)
    {
        var sum = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            sum += arr[i];
        }

        return sum;
    }

    [ConstantMethod(nameof(SumArray), new[] { 42, 69, 420 })]
    public static partial int SumArrayConstant();

    public static float FloatingPointCalc()
    {
        var sum = 0f;
        for (int i = 0; i < 10; i++)
        {
            sum += 0.1f;
        }
    
        return sum;
    }
    
    [ConstantMethod(nameof(FloatingPointCalc))]
    public static partial float FloatingPointConstant();
    
    public static decimal DecimalCalc()
    {
        var sum = 0m;
        for (int i = 0; i < 10; i++)
        {
            sum += 0.1m;
        }
    
        return sum;
    }
    
    [ConstantMethod(nameof(DecimalCalc))]
    public static partial decimal DecimalConstant();
}
