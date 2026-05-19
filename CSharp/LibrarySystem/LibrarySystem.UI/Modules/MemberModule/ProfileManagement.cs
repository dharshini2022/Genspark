using LibrarySystem.BLL.Interfaces;
using LibrarySystem.Exceptions;
using LibrarySystem.Models;
using LibrarySystem.UI.Session;

namespace LibrarySystem.UI.Modules.MemberModule
{
    public class ProfileManagement
    {
        private readonly IMemberService _memberService;

        public ProfileManagement(IMemberService memberService)
        {
            _memberService = memberService;
        }

        public void Show()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("====== PROFILE MANAGEMENT ======");
            Console.ResetColor();
            Console.WriteLine("1. View Profile");
            Console.WriteLine("2. Update Profile");
            Console.WriteLine("3. Deactivate Account");
            Console.WriteLine("0. Back");

            Console.Write("Enter Choice : ");

            int choice = Convert.ToInt32(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    ViewProfile();
                    break;

                case 2:
                    UpdateProfile();
                    break;

                case 3:
                    DeactivateAccount();
                    break;

                case 0:
                    return;

                default:
                    Console.WriteLine("Invalid Choice");
                    break;
            }
        }

        private void ViewProfile()
        {
            try
            {
                int memberId = SessionManager.SessionMember!.MemberId;

                var member = _memberService.ViewProfile(memberId);

                Console.WriteLine("====== MEMBER PROFILE ======");
                Console.WriteLine($"Member Id : {member.MemberId}");
                Console.WriteLine($"Name      : {member.Name}");
                Console.WriteLine($"Email     : {member.Email}");
                Console.WriteLine($"Password     : {member.Password}");
                Console.WriteLine($"Membership Type    : {member.MembershipType}");
                Console.WriteLine($"Joined At : {member.JoinedAt}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void UpdateProfile()
        {
            try
            {
                int memberId = SessionManager.SessionMember!.MemberId;
                Console.Write("Enter New Name : ");
                string name = Console.ReadLine()!;

                Console.Write("Enter New Email : ");
                string email = Console.ReadLine()!;

                var updatedMember = _memberService.UpdateProfile(memberId, name, email);
                Console.WriteLine("Do you want to update your Membership type? (yes / no)");
                string option = (Console.ReadLine() ?? "").Trim().ToLower();
                if(option == "yes")
                {
                    Console.WriteLine();
                    Console.WriteLine("Enter 1:Basic\n2:Student\n3:Premium");
                    int typeOption = Convert.ToInt32(Console.ReadLine());
                    Member.membershipType newType = Member.membershipType.Basic;
                    switch (typeOption)
                    {
                        case 2:
                            Console.WriteLine("Enter your Student Id:");
                            string tempRead = Console.ReadLine() ?? "";
                            newType = Member.membershipType.Student;
                            break;
                        case 3:
                            newType = Member.membershipType.Premium;
                            break;
                        default:
                            newType = Member.membershipType.Basic;
                            break;
                    }
                    _memberService.UpdateMembershipType(memberId, newType);

                }

                SessionManager.SessionMember = updatedMember;

                Console.WriteLine("Profile Updated Successfully");
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

        private void DeactivateAccount()
        {
            try
            {
                int memberId = SessionManager.SessionMember!.MemberId;

                Console.Write("Are you sure you want to deactivate account? (yes/no) : ");

                string choice = Console.ReadLine()!.ToLower();

                if (choice != "yes")
                {
                    Console.WriteLine("Account Deactivation Cancelled");
                    return;
                }

                bool result = _memberService.DeactivateMember(memberId);

                if (result)
                {
                    Console.WriteLine("Account Deactivated Successfully");

                    SessionManager.LogOut();
                }
                else
                {
                    Console.WriteLine("Failed to deactivate account");
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
    }
}