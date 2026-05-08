     try
     {
         checked
         {
             int num1 = int.MaxValue;
             //num1--;      //to stop Overflow exception
             num1++;
             Console.WriteLine("The updated value is " + num1);
             Console.WriteLine("Now you can enter a number");
             num1 = Convert.ToInt32(Console.ReadLine());
             Console.WriteLine("Please enter the dinominator");
             int num2 = Convert.ToInt32(Console.ReadLine());
             var result = num1 / num2;
             Console.WriteLine("The final result is " + result);
         }
     }
     catch(OverflowException ofe)
     {
         Console.WriteLine(ofe.Message);//for programmer
         Console.WriteLine("OverflowException! value exceeded the max val of int");//end user
     }
     catch(FormatException fe)
     {
         Console.WriteLine(fe.Message);
         Console.WriteLine("Format Exception! Enter a number");
     }
     catch(DivideByZeroException dbze)
     {
         Console.WriteLine(dbze.Message);
         Console.WriteLine("DivideByZero Exception! Denominator must not be zero");
     }
     catch (Exception ex) 
     { 
         Console.WriteLine(ex.Message);
         Console.WriteLine("Generic Exception! Something went wrong");
     }
     Console.WriteLine("End of Execution");

 
 
