
// First Task

Console.WriteLine("Write #1 Number:");
int num1 = int.Parse(Console.ReadLine());
Console.WriteLine("Write #2 Number:");
int num2 = int.Parse(Console.ReadLine());
Console.WriteLine("Write #3 Number:");
int num3 = int.Parse(Console.ReadLine());
Console.WriteLine("Write #4 Number:");
int num4 = int.Parse(Console.ReadLine());
Console.WriteLine("Write #5 Number:");
int num5 = int.Parse(Console.ReadLine());

int sum = num1 + num2 + num3 + num4 + num5;
Console.WriteLine("The sum is: " + sum);

int min = Math.Min(Math.Min(Math.Min(Math.Min(num1, num2), num3), num4), num5);
Console.WriteLine("The minimum value is: " + min);

int max = Math.Max(Math.Max(Math.Max(Math.Max(num1, num2), num3), num4), num5);
Console.WriteLine("The maximum value is: " + max);

int prodact = num1 * num2 * num3 * num4 * num5;
Console.WriteLine("The product is: " + prodact);

// Second Task

Console.WriteLine("Write A Number:");
int A = int.Parse(Console.ReadLine());
Console.WriteLine("Write B Number:");
int B = int.Parse(Console.ReadLine());
for (int i = A; i <= B; i++)
{
    for (int j = 0; j < i; j++)
    {
        Console.Write(i + " ");
    }
    Console.WriteLine();
}

//Third Task

Console.WriteLine("Enter the length of the line:");
int length = int.Parse(Console.ReadLine());
Console.WriteLine("Enter the fill character:");
char fillChar = Console.ReadLine()[0];
Console.WriteLine("Enter the direction of the line (h for horizontal, v for vertical):");
char direction = Console.ReadLine()[0];
