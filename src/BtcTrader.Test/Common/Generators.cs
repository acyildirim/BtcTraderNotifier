using Bogus;

namespace BtcTrader.Test.Common;

public static class Generators
{
    public static bool GenerateRandomBoolean()
    {
        var fake = new Faker();
        return fake.Random.Bool();
    }
    public static DateTime GenerateRandomDate()
    {
        var rnd = new Random();
        var datetoday = DateTime.Now;

        var rndYear = rnd.Next(1995, datetoday.Year);
        var rndMonth = rnd.Next(1, 12);
        var rndDay = rnd.Next(1, 31);
        return new DateTime(rndYear, rndMonth, rndDay);
    }
}