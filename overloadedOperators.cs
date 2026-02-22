using System;
using System.Collections.Generic;

namespace PracticeApp
{
    // Task 1
    class CreditCard
    {
        public string CardNumber { get; set; }
        public string OwnerName { get; set; }

        private int cvc;
        public int CVC
        {
            get => cvc;
            set
            {
                if (value < 100 || value > 999)
                    throw new ArgumentException("CVC must be 3 digits");
                cvc = value;
            }
        }

        public decimal Balance { get; private set; }

        public CreditCard(string number, string owner, int cvc, decimal balance)
        {
            CardNumber = number;
            OwnerName = owner;
            CVC = cvc;
            Balance = balance;
        }

        public static CreditCard operator +(CreditCard card, decimal amount)
        {
            card.Balance += amount;
            return card;
        }

        public static CreditCard operator -(CreditCard card, decimal amount)
        {
            if (card.Balance < amount)
                throw new InvalidOperationException("Not enough money");
            card.Balance -= amount;
            return card;
        }

        public static bool operator ==(CreditCard c1, CreditCard c2)
        {
            return c1.CVC == c2.CVC;
        }

        public static bool operator !=(CreditCard c1, CreditCard c2)
        {
            return !(c1 == c2);
        }

        public static bool operator >(CreditCard c1, CreditCard c2)
        {
            return c1.Balance > c2.Balance;
        }

        public static bool operator <(CreditCard c1, CreditCard c2)
        {
            return c1.Balance < c2.Balance;
        }

        public override bool Equals(object obj)
        {
            if (obj is CreditCard other)
                return this.CVC == other.CVC;
            return false;
        }

        public override int GetHashCode()
        {
            return CVC.GetHashCode();
        }

        public override string ToString()
        {
            return $"{OwnerName} | Balance: {Balance}";
        }
    }

    // Task 2
    class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }

        public Book(string title, string author)
        {
            Title = title;
            Author = author;
        }

        public static bool operator ==(Book b1, Book b2)
        {
            return b1.Title == b2.Title && b1.Author == b2.Author;
        }

        public static bool operator !=(Book b1, Book b2)
        {
            return !(b1 == b2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Book other)
                return this == other;
            return false;
        }

        public override int GetHashCode()
        {
            return Title.GetHashCode() ^ Author.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Title} - {Author}";
        }
    }

    class ReadingList
    {
        private List<Book> books = new List<Book>();

        public Book this[int index]
        {
            get => books[index];
            set => books[index] = value;
        }

        public void AddBook(Book book) => books.Add(book);
        public void RemoveBook(Book book) => books.Remove(book);
        public bool Contains(Book book) => books.Contains(book);

        public static ReadingList operator +(ReadingList list, Book book)
        {
            list.AddBook(book);
            return list;
        }

        public static ReadingList operator -(ReadingList list, Book book)
        {
            list.RemoveBook(book);
            return list;
        }

        public void PrintAll()
        {
            foreach (var book in books)
                Console.WriteLine(book);
        }
    }

    // Task 3
    class Fraction
    {
        private int denominator;

        public int Numerator { get; set; }

        public int Denominator
        {
            get => denominator;
            set
            {
                if (value == 0)
                    throw new DivideByZeroException("Denominator cannot be zero");
                denominator = value;
            }
        }

        public Fraction(int num, int den)
        {
            Numerator = num;
            Denominator = den;
            Simplify();
        }

        private void Simplify()
        {
            int gcd = GCD(Math.Abs(Numerator), Math.Abs(Denominator));
            Numerator /= gcd;
            Denominator /= gcd;

            if (Denominator < 0)
            {
                Numerator *= -1;
                Denominator *= -1;
            }
        }

        private int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public static Fraction operator +(Fraction f1, Fraction f2)
        {
            return new Fraction(
                f1.Numerator * f2.Denominator + f2.Numerator * f1.Denominator,
                f1.Denominator * f2.Denominator);
        }

        public static Fraction operator -(Fraction f1, Fraction f2)
        {
            return new Fraction(
                f1.Numerator * f2.Denominator - f2.Numerator * f1.Denominator,
                f1.Denominator * f2.Denominator);
        }

        public static Fraction operator *(Fraction f1, Fraction f2)
        {
            return new Fraction(
                f1.Numerator * f2.Numerator,
                f1.Denominator * f2.Denominator);
        }

        public static Fraction operator /(Fraction f1, Fraction f2)
        {
            return new Fraction(
                f1.Numerator * f2.Denominator,
                f1.Denominator * f2.Numerator);
        }

        public static bool operator ==(Fraction f1, Fraction f2)
        {
            return f1.Numerator == f2.Numerator &&
                   f1.Denominator == f2.Denominator;
        }

        public static bool operator !=(Fraction f1, Fraction f2)
        {
            return !(f1 == f2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Fraction other)
                return this == other;
            return false;
        }

        public override int GetHashCode()
        {
            return Numerator.GetHashCode() ^ Denominator.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Numerator}/{Denominator}";
        }
    }


    class Program
    {
        static void Main()
        {
            // 1
            var card1 = new CreditCard("1234", "Max", 123, 1000);
            var card2 = new CreditCard("5678", "Alex", 456, 500);

            card1 += 200;
            card1 -= 100;

            Console.WriteLine(card1);
            Console.WriteLine(card1 > card2);

            // 2
            var list = new ReadingList();
            var book1 = new Book("1984", "Orwell");
            var book2 = new Book("Dune", "Herbert");

            list += book1;
            list += book2;
            list.PrintAll();

            Console.WriteLine(list.Contains(book1));

            // 3
            var f1 = new Fraction(1, 2);
            var f2 = new Fraction(3, 4);

            var result = f1 + f2;
            Console.WriteLine(result);
        }
    }
}
