using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Data;
using Newtonsoft.Json;

using Foo foo = new();

foo.Bar = 1000;

public interface IAutoSerialize<T> : IDisposable
{
}
public static class AutoSerializeExtension
{
    public static void serialize<T>(this T obj)
    {
        File.WriteAllText($"{System.Reflection.MethodBase.GetCurrentMethod().GetParameters()[0].Name}.json", JsonConvert.SerializeObject(obj));
    }
}

public class Foo : IAutoSerialize<Foo>
{

    public int Bar = 2525;

    ~Foo()
    {
        this.serialize();
    }

    public void Dispose()
    {
        this.serialize();
    }
}


//// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

//IWebDriver driver = new ChromeDriver();

//driver.Navigate().GoToUrl("https://www.twitch.tv/andersonjph");

//Console.ReadLine();

//driver.Quit();

