int[] sample = {1,2,3,4,5,6,6,7,9};
//sample query result
var queryresult = from a in sample where a%2==0 select a;
//sample method result
queryresult = sample.Where(n => n%2 == 0);
foreach(int n in queryresult)
{
    Console.Write(n + " ");
}
Console.WriteLine();

//1. Even Numbers
List<int> num = new List<int>{1,2,3,4,5,6,7,8,9,10,11,12,13,14,15};
var evenNum = num.Where(n => n % 2 == 0);
foreach(var n in evenNum)
{
    Console.Write(n + " ");
}
Console.WriteLine();

//2. Positive numbers within 11
List<int> num2 = new List<int>{-9,-5,-3,-4,1,2,3,4,5,9,11,45,66};
var positiveNumbers = num2.Where(n => n > 1 && n < 11);
foreach(var n in positiveNumbers)
{
    Console.Write(n + " ");
}
Console.WriteLine();

//3. square digits in an array
int[] arr = {1,2,5,7,8,10,59};
var square = arr.Select(n => n * n).ToArray();
for(int idx = 0; idx < arr.Length; ++idx)
{
    Console.WriteLine($"Number: {arr[idx]} - Square: {square[idx]}");
}

//4. Frequency of no. in an array
int[] arr2 = {1,1,4,3,2,4,3};
var result = arr2.GroupBy(n => n);
foreach(var group in result)
{
    Console.WriteLine($"Number {group.Key} appears {group.Count()} times");
}