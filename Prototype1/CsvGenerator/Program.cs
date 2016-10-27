namespace CsvGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                MainFunction obj = new MainFunction(args[0], args[1]);
                obj.StartTimer();
            }
        }

    }
}
