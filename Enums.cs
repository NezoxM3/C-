using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HomeworkExceptionsOOP
{

//  * 1.
//  * Користувач вводить з клавіатури в рядок набір 0 і 1. 
//  * Необхідно перетворити рядок у число цілого типу в десятковому поданні. 
//  * Передбачити випадок виходу за межі діапазону, що визначається типом int, 
//  * неправильне введення. Використовуйте механізм виключень.
//  * 
//  * 0001 -> 1
//  * 
//  * 1000 -> 8
//  * 
//  * 10101010101010111001101101010101010111111111111111 => вихід за межі int
//  * 
//  * Межі int: -2.147.483.648 до 2.147.483.647
//  *      uint: 0 до 4.294.967.295

    class BinaryConverter
    {
        public static int ConvertToInt(string binary)
        {
            if (string.IsNullOrWhiteSpace(binary))
                throw new ArgumentException("Рядок порожній.");

            if (!binary.All(c => c == '0' || c == '1'))
                throw new FormatException("Рядок повинен містити тільки 0 та 1.");

            try
            {
                return Convert.ToInt32(binary, 2);
            }
            catch (OverflowException)
            {
                throw new OverflowException("Число виходить за межі типу int.");
            }
        }
    }

// * 2.
//  * Користувач вводить у рядок з клавіатури математичний вираз. 
//  * Наприклад, 3*2*1*4. Програма повинна порахувати результат введеного виразу. 
//  * У рядку можуть бути тільки цілі числа й оператор *. 
//  * Для обробки помилок введення використовуйте механізм виключень.

    class ExpressionCalculator
    {
        public static int Calculate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Порожній вираз.");

            if (!Regex.IsMatch(expression, @"^\d+(\*\d+)*$"))
                throw new FormatException("Некоректний формат. Дозволені лише числа та *.");

            try
            {
                string[] numbers = expression.Split('*');
                checked
                {
                    int result = 1;
                    foreach (var num in numbers)
                        result *= int.Parse(num);
                    return result;
                }
            }
            catch (OverflowException)
            {
                throw new OverflowException("Результат виходить за межі int.");
            }
        }
    }

// 3.
//  Створіть клас "Працівник" з методами для розрахунку заробітної плати. 
//  Поля: (ім'я, ставка за годину)

//  Перевантажте методи для різних типів розрахунків (за фіксованою ставкою, за годинним тарифом і з урахуванням премій). 
// - метод calculateSalary (без параметрів)  - повертає зарплату за місяць (робочі години * на ставку)
// - метод calculateSalary (години) - повертає зарпрлату за відпрацьовані години
// - метод calculateSalary (премія (відсоток)) - повертає зарплату за місяць + премія
// Додайте можливість повертати результат у вигляді кортежу, де буде вказана зарплата, 
// тип розрахунку (enum) і примітка (наприклад, "За місяць без премії" або "з урахуванням премій" і т.і).

// Відповідно до типу розрахунку - (створити enum) через switch вивести назву 
// розрахунку, примітку та вираховане значеня

// Дані отримувати від користувача а також отримувати тип розрахунку від користувача.

    enum SalaryType
    {
        Monthly = 1,
        Hourly = 2,
        WithBonus = 3
    }

    class Employee
    {
        public string Name { get; set; }
        public decimal HourRate { get; set; }
        private const int DefaultMonthlyHours = 160;

        public Employee(string name, decimal rate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ім'я не може бути порожнім.");

            if (rate <= 0)
                throw new ArgumentException("Ставка повинна бути більше 0.");

            Name = name;
            HourRate = rate;
        }

        public (decimal salary, SalaryType type, string note) CalculateSalary()
        {
            decimal salary = HourRate * DefaultMonthlyHours;
            return (salary, SalaryType.Monthly, "За місяць без премії");
        }

        public (decimal salary, SalaryType type, string note) CalculateSalary(int hours)
        {
            decimal salary = HourRate * hours;
            return (salary, SalaryType.Hourly, "За відпрацьовані години");
        }

        public (decimal salary, SalaryType type, string note) CalculateSalary(decimal bonusPercent)
        {
            decimal baseSalary = HourRate * DefaultMonthlyHours;
            decimal salary = baseSalary + (baseSalary * bonusPercent / 100);
            return (salary, SalaryType.WithBonus, "З урахуванням премії");
        }
    }

// 4. 
// Створіть клас "Кредитна картка". Вам необхідно зберігати інформацію про номер картки, 
// ПІБ власника, CVC, дату завершення роботи картки тощо. 
// Передбачити механізми для ініціалізації полів класу. 
// Якщо значення для ініціалізації невірне, генеруйте виключення.
// Карта в форматі 1111-1111-1111-1111 або без - підряд 1111111111111111
// Дата завершення вводиться в форматі 10/2025 - місяць/рік
// Для перевірки валідності номера карти реалізувати алгоритм Луна https://uk.wikipedia.org/wiki/%D0%90%D0%BB%D0%B3%D0%BE%D1%80%D0%B8%D1%82%D0%BC_%D0%9B%D1%83%D0%BD%D0%B0

// можливі помилки
// - пусте ім'я
// - невалідний номер карти
// - CVC номер не 3 цифри
// - Дата та рік завершення пройшли (10/2025)


    class CreditCard
    {
        public string CardNumber { get; private set; }
        public string FullName { get; private set; }
        public string CVC { get; private set; }
        public DateTime ExpiryDate { get; private set; }

        public CreditCard(string number, string name, string cvc, string expiry)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ім'я не може бути порожнім.");

            number = number.Replace("-", "");
            if (!Regex.IsMatch(number, @"^\d{16}$") || !IsValidLuhn(number))
                throw new ArgumentException("Невалідний номер карти.");

            if (!Regex.IsMatch(cvc, @"^\d{3}$"))
                throw new ArgumentException("CVC повинен містити 3 цифри.");

            if (!DateTime.TryParseExact(expiry, "MM/yyyy", null,
                System.Globalization.DateTimeStyles.None, out DateTime date))
                throw new ArgumentException("Невірний формат дати (MM/yyyy).");

            if (date < DateTime.Now)
                throw new ArgumentException("Термін дії карти минув.");

            CardNumber = number;
            FullName = name;
            CVC = cvc;
            ExpiryDate = date;
        }

        private bool IsValidLuhn(string number)
        {
            int sum = 0;
            bool alternate = false;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                int n = number[i] - '0';

                if (alternate)
                {
                    n *= 2;
                    if (n > 9)
                        n -= 9;
                }

                sum += n;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }
    }

    class Program
    {
        static void Main()
        {
            // 1
            Console.WriteLine("Завдання 1: Перетворення двійкового числа");
            try
            {
                Console.Write("Введіть 0 і 1: ");
                string binary = Console.ReadLine();
                int result = BinaryConverter.ConvertToInt(binary);
                Console.WriteLine($"Десяткове число: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }

            // 2
            Console.WriteLine("\nЗавдання 2: Обчислення виразу");
            try
            {
                Console.Write("Введіть вираз (тільки *): ");
                string expr = Console.ReadLine();
                int result = ExpressionCalculator.Calculate(expr);
                Console.WriteLine($"Результат: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }

            // 3
            Console.WriteLine("\nЗавдання 3: Зарплата працівника");
            try
            {
                Console.Write("Ім'я: ");
                string name = Console.ReadLine();

                Console.Write("Ставка за годину: ");
                decimal rate = decimal.Parse(Console.ReadLine());

                Employee emp = new Employee(name, rate);

                Console.WriteLine("Тип розрахунку: 1-Місяць 2-Години 3-Премія");
                SalaryType type = (SalaryType)int.Parse(Console.ReadLine());

                var result = type switch
                {
                    SalaryType.Monthly => emp.CalculateSalary(),
                    SalaryType.Hourly => emp.CalculateSalary(
                        int.Parse(Console.ReadLine())),
                    SalaryType.WithBonus => emp.CalculateSalary(
                        decimal.Parse(Console.ReadLine())),
                    _ => throw new ArgumentException("Невірний тип.")
                };

                switch (result.type)
                {
                    case SalaryType.Monthly:
                        Console.WriteLine("Тип: За місяць");
                        break;
                    case SalaryType.Hourly:
                        Console.WriteLine("Тип: За години");
                        break;
                    case SalaryType.WithBonus:
                        Console.WriteLine("Тип: З премією");
                        break;
                }

                Console.WriteLine($"Примітка: {result.note}");
                Console.WriteLine($"Сума: {result.salary}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }

            // 4
            Console.WriteLine("\nЗавдання 4: Кредитна картка");
            try
            {
                Console.Write("Номер карти: ");
                string number = Console.ReadLine();

                Console.Write("ПІБ: ");
                string name = Console.ReadLine();

                Console.Write("CVC: ");
                string cvc = Console.ReadLine();

                Console.Write("Дата завершення (MM/yyyy): ");
                string expiry = Console.ReadLine();

                CreditCard card = new CreditCard(number, name, cvc, expiry);

                Console.WriteLine("Карта успішно створена!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }
    }
}
