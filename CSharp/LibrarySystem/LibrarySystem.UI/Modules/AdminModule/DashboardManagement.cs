namespace LibrarySystem.UI.Modules.AdminModule
{
    public class DashboardManagement
    {
        public void Show()
        {
            Console.WriteLine("====== DASHBOARD REPORTS ======");

            Console.WriteLine("1. Overall Borrowed Books");
            Console.WriteLine("2. Overall Overdue Books");
            Console.WriteLine("3. Members with Pending Fines");
            Console.WriteLine("4. Expired Memberships");
            Console.WriteLine("5. Most Borrowed Book");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch(choice)
            {
                case 1:
                    OverallBorrowedBooks();
                    break;

                case 2:
                    OverallOverdueBooks();
                    break;

                case 3:
                    MembersWithPendingFines();
                    break;

                case 4:
                    ExpiredMemberships();
                    break;

                case 5:
                    MostBorrowedBook();
                    break;
            }
        }

        private void OverallBorrowedBooks()
        {

        }

        private void OverallOverdueBooks()
        {

        }

        private void MembersWithPendingFines()
        {

        }

        private void ExpiredMemberships()
        {

        }

        private void MostBorrowedBook()
        {

        }
    }
}