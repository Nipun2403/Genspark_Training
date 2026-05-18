using BusinessLogicLayer.Services;
using DataAccessLayer.Entities;
using PresentationLayer.UI;

namespace PresentationLayer.Menus;


/// Console menu for member management operations with full input validation and list selections.

public class MemberMenu
{
    private readonly MemberService _memberService;

    public MemberMenu(MemberService memberService)
    {
        _memberService = memberService;
    }

    public async Task ShowAsync()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("--- MEMBER MANAGEMENT ---");
            Console.WriteLine("1. Add New Member");
            Console.WriteLine("2. View All Members");
            Console.WriteLine("3. Search Member (by Phone/Email)");
            Console.WriteLine("4. Update Membership Type");
            Console.WriteLine("5. Deactivate Member");
            Console.WriteLine("6. Reactivate Member");
            Console.WriteLine("0. Back to Main Menu");
            
            var choice = InputValidator.GetString("Select: ");
            switch (choice)
            {
                case "1": await AddMemberAsync(); break;
                case "2": await ViewAllMembersAsync(); break;
                case "3": await SearchMemberAsync(); break;
                case "4": await UpdateMembershipAsync(); break;
                case "5": await DeactivateMemberAsync(); break;
                case "6": await ReactivateMemberAsync(); break;
                case "0": back = true; break;
                default: Console.WriteLine("  [Error] Invalid option."); break;
            }
        }
    }

    private async Task AddMemberAsync()
    {
        Console.WriteLine("\n--- Add New Member ---");
        var name = InputValidator.GetValidName("Full Name: ");
        var email = InputValidator.GetValidEmail("Email: ");
        var phone = InputValidator.GetValidPhoneNumber("Phone Number: ");

        var configs = await _memberService.GetAllMembershipConfigsAsync();
        if (configs.Count == 0)
        {
            Console.WriteLine("Error: No membership configurations found in the system.");
            return;
        }

        var selectedConfig = InputValidator.GetSelection(
            configs,
            "Select Membership Type: ",
            c => $"{c.MembershipType,-10} | Max Borrows: {c.MaxActiveBorrowings,-2} | Max Days: {c.MaxBorrowDays,-3}"
        );

        if (selectedConfig == null) return; // Cancelled

        var result = await _memberService.AddMemberAsync(name, email, phone, selectedConfig.MembershipType);
        Console.WriteLine(result);
    }

    private async Task ViewAllMembersAsync()
    {
        var members = await _memberService.GetAllMembersAsync();
        if (members.Count == 0)
        {
            Console.WriteLine("No members found.");
            return;
        }

        Console.WriteLine($"\n{"ID",-6} {"Name",-25} {"Email",-30} {"Phone",-15} {"Type",-10} {"Active",-7}");
        Console.WriteLine(new string('-', 95));
        foreach (var m in members)
        {
            Console.WriteLine($"{m.MemberId,-6} {m.FullName,-25} {m.Email,-30} {m.PhoneNumber,-15} {m.MembershipType,-10} {(m.IsActive ? "Yes" : "No"),-7}");
        }
    }

    private async Task SearchMemberAsync()
    {
        var search = InputValidator.GetString("Enter Phone Number or Email: ");

        var member = await _memberService.SearchMemberAsync(search);
        if (member == null)
        {
            Console.WriteLine("Member not found.");
            return;
        }

        Console.WriteLine($"\n  ID: {member.MemberId}");
        Console.WriteLine($"  Name: {member.FullName}");
        Console.WriteLine($"  Email: {member.Email}");
        Console.WriteLine($"  Phone: {member.PhoneNumber}");
        Console.WriteLine($"  Type: {member.MembershipType} (Max borrows: {member.MembershipConfig.MaxActiveBorrowings}, Max days: {member.MembershipConfig.MaxBorrowDays})");
        Console.WriteLine($"  Active: {(member.IsActive ? "Yes" : "No")}");
        Console.WriteLine($"  Joined: {member.JoinDate:yyyy-MM-dd}");
    }

    private async Task UpdateMembershipAsync()
    {
        var members = await _memberService.GetAllMembersAsync();
        if (members.Count == 0)
        {
            Console.WriteLine("No members found.");
            return;
        }

        var member = InputValidator.GetSelection(
            members, 
            "Select member to update: ", 
            m => $"{m.FullName} ({m.Email}) [Current Type: {m.MembershipType}]"
        );
        if (member == null) return;

        var configs = await _memberService.GetAllMembershipConfigsAsync();
        var selectedConfig = InputValidator.GetSelection(
            configs,
            "Select New Membership Type: ",
            c => $"{c.MembershipType,-10} | Max Borrows: {c.MaxActiveBorrowings,-2} | Max Days: {c.MaxBorrowDays,-3}"
        );

        if (selectedConfig == null) return;

        var result = await _memberService.UpdateMembershipTypeAsync(member.MemberId, selectedConfig.MembershipType);
        Console.WriteLine(result);
    }

    private async Task DeactivateMemberAsync()
    {
        var members = await _memberService.GetAllMembersAsync();
        var activeMembers = members.Where(m => m.IsActive).ToList();

        if (activeMembers.Count == 0)
        {
            Console.WriteLine("No active members found to deactivate.");
            return;
        }

        var member = InputValidator.GetSelection(
            activeMembers, 
            "Select member to deactivate: ", 
            m => $"{m.FullName} ({m.Email})"
        );
        if (member == null) return;

        var result = await _memberService.DeactivateMemberAsync(member.MemberId);
        Console.WriteLine(result);
    }

    private async Task ReactivateMemberAsync()
    {
        var members = await _memberService.GetAllMembersAsync();
        var inactiveMembers = members.Where(m => !m.IsActive).ToList();

        if (inactiveMembers.Count == 0)
        {
            Console.WriteLine("No inactive members found to reactivate.");
            return;
        }

        var member = InputValidator.GetSelection(
            inactiveMembers, 
            "Select member to reactivate: ", 
            m => $"{m.FullName} ({m.Email})"
        );
        if (member == null) return;

        var result = await _memberService.ReactivateMemberAsync(member.MemberId);
        Console.WriteLine(result);
    }
}
