using System;
using System.Collections.Generic;

namespace DesignPatternsHomework
{


    public interface ICoffee
    {
        string GetDescription();
        double GetPrice();
    }

    public class Espresso : ICoffee
    {
        public string GetDescription() => "Espresso";
        public double GetPrice() => 35.0;
    }

    public abstract class CoffeeDecorator : ICoffee
    {
        protected ICoffee coffee;

        protected CoffeeDecorator(ICoffee coffee)
        {
            this.coffee = coffee;
        }

        public virtual string GetDescription()
        {
            return coffee.GetDescription();
        }

        public virtual double GetPrice()
        {
            return coffee.GetPrice();
        }
    }

    public class Milk : CoffeeDecorator
    {
        public Milk(ICoffee coffee) : base(coffee) { }

        public override string GetDescription()
        {
            return coffee.GetDescription() + ", Milk";
        }

        public override double GetPrice()
        {
            return coffee.GetPrice() + 12;
        }
    }

    public class VanillaSyrup : CoffeeDecorator
    {
        public VanillaSyrup(ICoffee coffee) : base(coffee) { }

        public override string GetDescription()
        {
            return coffee.GetDescription() + ", Vanilla Syrup";
        }

        public override double GetPrice()
        {
            return coffee.GetPrice() + 15;
        }
    }

    public class Chocolate : CoffeeDecorator
    {
        public Chocolate(ICoffee coffee) : base(coffee) { }

        public override string GetDescription()
        {
            return coffee.GetDescription() + ", Chocolate";
        }

        public override double GetPrice()
        {
            return coffee.GetPrice() + 18;
        }
    }


    public class OldCashRegister
    {
        public void PrintCheck(string text)
        {
            Console.WriteLine("CHECK");
            Console.WriteLine("-------------------");
            Console.WriteLine(text);
        }
    }

    public interface IPrinter
    {
        void Print(ICoffee coffee);
    }

    public class PrinterAdapter : IPrinter
    {
        private readonly OldCashRegister cashRegister;

        public PrinterAdapter(OldCashRegister cashRegister)
        {
            this.cashRegister = cashRegister;
        }

        public void Print(ICoffee coffee)
        {
            string receipt =
                $"Drink: {coffee.GetDescription()}\n" +
                $"Price: {coffee.GetPrice()} UAH";

            cashRegister.PrintCheck(receipt);
        }
    }


    public interface IMenuPrinter
    {
        void PrintSectionStart(string name, int level);
        void PrintSectionEnd(int level);
        void PrintDish(string name, double price, int level);
    }


    public abstract class MenuComponent
    {
        public string Name { get; set; }

        protected MenuComponent(string name)
        {
            Name = name;
        }

        public abstract void Print(IMenuPrinter printer, int level = 0);
    }


    public class Dish : MenuComponent
    {
        public double Price { get; set; }

        public Dish(string name, double price)
            : base(name)
        {
            Price = price;
        }

        public override void Print(IMenuPrinter printer, int level = 0)
        {
            printer.PrintDish(Name, Price, level);
        }
    }

    public class MenuSection : MenuComponent
    {
        private readonly List<MenuComponent> items =
            new List<MenuComponent>();

        public MenuSection(string name)
            : base(name)
        {
        }

        public void Add(MenuComponent component)
        {
            items.Add(component);
        }

        public override void Print(IMenuPrinter printer, int level = 0)
        {
            printer.PrintSectionStart(Name, level);

            foreach (var item in items)
            {
                item.Print(printer, level + 1);
            }

            printer.PrintSectionEnd(level);
        }
    }

    public class TextPrinter : IMenuPrinter
    {
        public void PrintSectionStart(string name, int level)
        {
            Console.WriteLine(
                new string(' ', level * 2) +
                $"[{name}]");
        }

        public void PrintSectionEnd(int level)
        {
        }

        public void PrintDish(string name, double price, int level)
        {
            Console.WriteLine(
                new string(' ', level * 2) +
                $"- {name} ({price} UAH)");
        }
    }

    public class HtmlPrinter : IMenuPrinter
    {
        public void PrintSectionStart(string name, int level)
        {
            Console.WriteLine($"<h2>{name}</h2>");
            Console.WriteLine("<ul>");
        }

        public void PrintSectionEnd(int level)
        {
            Console.WriteLine("</ul>");
        }

        public void PrintDish(string name, double price, int level)
        {
            Console.WriteLine(
                $"<li>{name} - {price} UAH</li>");
        }
    }


    public class JsonPrinter : IMenuPrinter
    {
        public void PrintSectionStart(string name, int level)
        {
            Console.WriteLine(
                $"{{ \"section\": \"{name}\", \"items\": [");
        }

        public void PrintSectionEnd(int level)
        {
            Console.WriteLine("]}");
        }

        public void PrintDish(string name, double price, int level)
        {
            Console.WriteLine(
                $"{{ \"dish\": \"{name}\", \"price\": {price} }},");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================");
            Console.WriteLine("DECORATOR + ADAPTER");
            Console.WriteLine("=================================");

            ICoffee coffee = new Espresso();

            coffee = new Milk(coffee);
            coffee = new VanillaSyrup(coffee);
            coffee = new Chocolate(coffee);

            Console.WriteLine("Description: " +
                              coffee.GetDescription());

            Console.WriteLine("Price: " +
                              coffee.GetPrice() +
                              " UAH");

            Console.WriteLine();

            IPrinter printer =
                new PrinterAdapter(
                    new OldCashRegister());

            printer.Print(coffee);

            Console.WriteLine();
            Console.WriteLine("=================================");
            Console.WriteLine("COMPOSITE + BRIDGE");
            Console.WriteLine("=================================");

            var menu = new MenuSection("Main Menu");

            var soups = new MenuSection("Soups");
            soups.Add(new Dish("Borscht", 95));
            soups.Add(new Dish("Solyanka", 110));

            var mainCourses =
                new MenuSection("Main Courses");

            mainCourses.Add(
                new Dish("Four Cheese Pizza", 220));

            mainCourses.Add(
                new Dish("Ribeye Steak", 380));

            menu.Add(soups);
            menu.Add(mainCourses);

            Console.WriteLine();
            Console.WriteLine("TEXT OUTPUT:");
            menu.Print(new TextPrinter());

            Console.WriteLine();
            Console.WriteLine("HTML OUTPUT:");
            menu.Print(new HtmlPrinter());

            Console.WriteLine();
            Console.WriteLine("JSON OUTPUT:");
            menu.Print(new JsonPrinter());

            Console.ReadKey();
        }
    }
}