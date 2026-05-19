using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;

namespace LibrarySystem.UI.Modules.AdminModule
{
    public class MemberManagement
    {
        private readonly IMemberService _memberService;

        public MemberManagement(IMemberService memberService)
        {
            _memberService = memberService;
        }

        public void Show()
        {
            Console.Clear();

            Console.WriteLine("====== MEMBER MANAGEMENT ======");

            Console.WriteLine("1. View All Members");
            Console.WriteLine("2. Search Member By Id");
            Console.WriteLine("3. Search Member By Email");
            Console.WriteLine("4. Search Member By Name");
            Console.WriteLine("5. Deactivate Member");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    ViewAllMembers();
                    break;

                case 2:
                    SearchById();
                    break;

                case 3:
                    SearchByEmail();
                    break;

                case 4:
                    SearchByName();
                    break;

                case 5:
                    DeactivateMember();
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid Choice");
                    break;
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private void ViewAllMembers()
        {
            try
            {
                List<Member> members = _memberService.ViewAllMembers();

                if (members.Count == 0)
                {
                    Console.WriteLine("No Members Found");
                    return;
                }

                foreach (var member in members)
                {
                    DisplayMember(member);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SearchById()
        {
            try
            {
                Console.Write("Enter Member Id : ");

                int memberId = Convert.ToInt32(Console.ReadLine());

                Member member = _memberService.SearchMemberById(memberId);

                DisplayMember(member);
            }
            catch (InvalidMemberException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SearchByEmail()
        {
            try
            {
                Console.Write("Enter Email : ");

                string email = Console.ReadLine()!;

                Member member = _memberService.SearchMemberByEmail(email);

                DisplayMember(member);
            }
            catch (InvalidMemberException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SearchByName()
        {
            try
            {
                Console.Write("Enter Name : ");

                string name = Console.ReadLine()!;

                List<Member> members = _memberService.SearchMemberByName(name);

                if (members.Count == 0)
                {
                    Console.WriteLine("No Members Found");
                    return;
                }

                foreach (var member in members)
                {
                    DisplayMember(member);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DeactivateMember()
        {
            try
            {
                Console.Write("Enter Member Id : ");

                int memberId = Convert.ToInt32(Console.ReadLine());

                bool result = _memberService.DeactivateMember(memberId);

                if (result)
                {
                    Console.WriteLine("Member Deactivated Successfully");
                }
                else
                {
                    Console.WriteLine("Failed To Deactivate Member");
                }
            }
            catch (InvalidMemberException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void DisplayMember(Member member)
        {
            Console.WriteLine("----------------------------------");
            Console.WriteLine($"Member Id      : {member.MemberId}");
            Console.WriteLine($"Name           : {member.Name}");
            Console.WriteLine($"Email          : {member.Email}");
            Console.WriteLine($"Membership Type: {member.MembershipType}");
            Console.WriteLine($"Is Active      : {member.IsActive}");
            Console.WriteLine($"Joined At     : {member.JoinedAt}");
            Console.WriteLine("----------------------------------");
        }
    }
}