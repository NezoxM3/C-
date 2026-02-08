// Завдання 1
// Створіть додаток, який відображає кількість парних, непарних, унікальних елементів масиву.
// Розмір масиву 10. Масив заповнюється випадковими числами від 1 до 100.


Console.WriteLine("Завдання 1:\n");

int[] arr = new int[10];
Random rnd = new Random();

for (int i = 0; i < 10; i++)
{
    arr[i] = rnd.Next(1, 101);
    Console.Write(arr[i] + " ");
}

int even = 0, odd = 0, unique = 0;

for (int i = 0; i < 10; i++)
{
    if (arr[i] % 2 == 0) even++;
    else odd++;

    bool isUnique = true;
    for (int j = 0; j < 10; j++)
    {
        if (i != j && arr[i] == arr[j])
        {
            isUnique = false;
            break;
        }
    }
    if (isUnique) unique++;
}

Console.WriteLine("\nПарні: " + even);
Console.WriteLine("Непарні: " + odd);
Console.WriteLine("Унікальні: " + unique);



// Завдання 2
// Створіть додаток, який відображає кількість значень у масиві, 
// менших за заданий користувачем параметр. 
// Наприклад, кількість значень менших, ніж 7 (7 введено користувачем з клавіатури).
// Розмір масиву 10. Масив заповнюється випадковими числами від 1 до 100.


Console.WriteLine("\nЗавдання 2:\n");

int[] arr2 = new int[10];
Random rnd2 = new Random();

for (int i = 0; i < 10; i++)
{
    arr2[i] = rnd2.Next(1, 101);
    Console.Write(arr2[i] + " ");
}

Console.Write("\nВведіть число для перевірки: ");
int userNumber = int.Parse(Console.ReadLine());

int count = 0;
for (int i = 0; i < 10; i++)
{
    if (arr2[i] < userNumber) count++;
}

Console.WriteLine("Кількість значень менших за " + userNumber + ": " + count);



// Завдання 3

//     Користувач вводить із клавіатури три числа. 
//     Необхідно підрахувати скільки разів послідовність із цих 
//     трьох чисел зустрічається в масиві.

//     Наприклад:

//     користувач ввів: 7 6 5.
//     масив: 7 6 5 3 4 7 6 5 8 7 6 5.
//     кількість повторень послідовності: 3.


Console.WriteLine("\nЗавдання 3:\n");

int[] arr3 = new int[12] { 7, 6, 5, 3, 4, 7, 6, 5, 8, 7, 6, 5 };

Console.WriteLine("Масив: " + string.Join(" ", arr3));

Console.Write("Введіть перше число: ");
int num1 = int.Parse(Console.ReadLine());

Console.Write("Введіть друге число: ");
int num2 = int.Parse(Console.ReadLine());

Console.Write("Введіть третє число: ");
int num3 = int.Parse(Console.ReadLine());

int sequenceCount = 0;
for (int i = 0; i < arr3.Length - 2; i++)
{
    if (arr3[i] == num1 && arr3[i + 1] == num2 && arr3[i + 2] == num3)
    {
        sequenceCount++;
    }
}

Console.WriteLine("Кількість повторень послідовності: " + sequenceCount);



// Завдання 4
//     Дано 2 масиви розмірності M і N відповідно.
//     Необхідно переписати до третього масиву загальні елементи 
//     перших двох масивів без повторень.

Console.WriteLine("\nЗавдання 4:\n");

Console.Write("Спільні елементи в масивах: ");

int[] M = { 1, 2, 3, 4, 5 };
int[] N = { 3, 4, 5, 6 };

for (int i = 0; i < M.Length; i++)
{
    bool exists = false;

    for (int j = 0; j < N.Length; j++)
        if (M[i] == N[j])
            exists = true;

    if (exists)
    {
        bool printed = false;
        for (int k = 0; k < i; k++)
            if (M[k] == M[i])
                printed = true;

        if (!printed)
            Console.Write(M[i] + ", ");
    }
}



// Завдання 5
//     Розробіть додаток, який знаходитиме мінімальне та максимальне значення 
//     у двовимірному масиві.


Console.WriteLine("\n\nЗавдання 5:\n");

int[,] matrix = {
    { 84, 21, 56 },
    { 13, 68, 9 },
    { 95, 37, 43 }
};

int min = matrix[0, 0];
int max = matrix[0, 0];

for (int i = 0; i < matrix.GetLength(0); i++)
{
    for (int j = 0; j < matrix.GetLength(1); j++)
    {
        if (matrix[i, j] < min)
            min = matrix[i, j];
        if (matrix[i, j] > max)
            max = matrix[i, j];
    }
}

Console.WriteLine("Мінімальне значення: " + min);
Console.WriteLine("Максимальне значення: " + max);



// Завдання 6
//     Користувач вводить речення з клавіатури. 
//     Вам необхідно підрахувати кількість слів у ньому.


Console.WriteLine("\nЗавдання 6:\n");

Console.Write("Введіть речення для перевірки: ");

string sentence = Console.ReadLine();
string[] words = sentence.Split(new char[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

Console.WriteLine("Кількість слів: " + words.Length);



// Завдання 7
//     Користувач вводить речення з клавіатури. 
//     Перевернути кожне слово в реченні.
//     Вивести строку користувача та результат на екран.


Console.WriteLine("\nЗавдання 7:\n");

Console.Write("Введіть речення, щоб перевернути кожне слово: ");

string inputSentence = Console.ReadLine();
string[] inputWords = inputSentence.Split(' ');

for (int i = 0; i < inputWords.Length; i++)
{
    char[] charArray = inputWords[i].ToCharArray();
    Array.Reverse(charArray);
    inputWords[i] = new string(charArray);
}

string resultSentence = string.Join(" ", inputWords);
Console.WriteLine("Результат перевертання слів: " + resultSentence);



// Завдання 8
// Створіть додаток для підрахунку кількості входжень підрядка в рядок. 
// Користувач вводить вихідний рядок і слово для пошуку. 
// Додаток відображає результат пошуку.
// Наприклад:
// користувач ввів: "Why she had to go. I don't know, she wouldn't say".
// підрядок для пошуку: she.
// результат пошуку: 2.

Console.WriteLine("\nЗавдання 8:\n");

Console.Write("Введіть рядок: ");
string mainString = Console.ReadLine();

Console.Write("Введіть підрядок для пошуку: ");
string substring = Console.ReadLine();

int occurrenceCount = 0;
int index = mainString.IndexOf(substring);

while (index != -1)
{
    occurrenceCount++;
    index = mainString.IndexOf(substring, index + substring.Length);
}

Console.WriteLine("Кількість входжень підрядка '" + substring + "': " + occurrenceCount);



// Завдання 9
// Напишіть програму, яка приймає через аргументи командного рядка рядок 
// і генерує всі можливі перестановки його символів. 
// Наприклад, якщо рядок містить 3 символи, програма повинна вивести 
// всі 6 можливих перестановок. Приклад використання програми:

// > "abc".

// Результат:

// abc
// acb
// bac
// bca
// cab
// cba

Console.WriteLine("\nЗавдання 9:\n");

Console.Write("Введіть рядок для генерації перестановок: ");

string input = Console.ReadLine();

void Permute(string str, int l, int r)
{
    if (l == r)
        Console.WriteLine(str);
    else
    {
        for (int i = l; i <= r; i++)
        {
            str = Swap(str, l, i);
            Permute(str, l + 1, r);
            str = Swap(str, l, i);
        }
    }
}

string Swap(string a, int i, int j)
{
    char temp;
    char[] charArray = a.ToCharArray();
    temp = charArray[i];
    charArray[i] = charArray[j];
    charArray[j] = temp;
    return new string(charArray);
}

Permute(input, 0, input.Length - 1);


// Завдання 10
// Напишіть програму, яка приймає через командний рядок текст 
// і відображає частотний аналіз — скільки разів кожен символ зустрічається в тексті. 
// Програма має враховувати як великі, так і малі літери окремо. 
// Приклад використання програми:

// > "Hello World".

// H: 1
// e: 1
// l: 3
// o: 2
// W: 1
// r: 1
// d: 1

Console.WriteLine("\nЗавдання 10:\n");

Console.Write("Введіть текст для частотного аналізу: ");
string text = Console.ReadLine();

Dictionary<char, int> frequency = new Dictionary<char, int>();

foreach (char c in text)
{
    if (frequency.ContainsKey(c))
        frequency[c]++;
    else
        frequency[c] = 1;
}

foreach (var pair in frequency)
{
    Console.WriteLine(pair.Key + ": " + pair.Value);
}