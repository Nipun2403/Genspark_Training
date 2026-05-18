using BusinessLogicLayer.Services;
using PresentationLayer.UI;
using System.Linq;

namespace PresentationLayer.Menus;


/// Console menu for fine management (view, pay, history).

public class FineMenu
{
    private readonly FineService _fineService;
    private readonly MemberService _memberService;

    public FineMenu(FineService fineService, MemberService memberService)
    {
        _fineService = fineService;
        _memberService = memberService;
    }

    public async Task ShowAsync()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("--- FINE MANAGEMENT ---");
            Console.WriteLine("1. View Pending Fines");
            Console.WriteLine("2. Pay Fine");
            Console.WriteLine("3. View Fine History");
            Console.WriteLine("0. Back to Main Menu");

            var choice = InputValidator.GetString("Select: ");
            switch (choice)
            {
                case "1": await ViewPendingFinesAsync(); break;
                case "2": await PayFineAsync(); break;
                case "3": await ViewFineHistoryAsync(); break;
                case "0": back = true; break;
                default: Console.WriteLine("  [Error] Invalid option."); break;
            }
        }
    }

    private async Task ViewPendingFinesAsync()
    {
        var members = await _memberService.GetAllMembersAsync();
        var activeMembers = members.Where(m => m.IsActive).ToList();

        if (activeMembers.Count == 0)
        {
            Console.WriteLine("No active members found.");
            return;
        }

        var member = InputValidator.GetSelection(activeMembers, "Select Member to view fines: ", m => $"{m.FullName} ({m.Email})");
        if (member == null) return;

        var fines = await _fineService.GetPendingFinesAsync(member.MemberId);
        if (fines.Count == 0)
        {
            Console.WriteLine("No pending fines for this member.");
            return;
        }

        Console.WriteLine($"\nPending Fines for {member.FullName}:");
        Console.WriteLine($"{"Fine ID",-8} {"Type",-22} {"Amount",-10} {"Paid",-10} {"Created",-12}");
        Console.WriteLine(new string('-', 65));
        foreach (var f in fines)
        {
            Console.WriteLine($"{f.FineId,-8} {f.FineType,-22} ₹{f.Amount,-9:F2} ₹{f.PaidAmount,-9:F2} {f.CreatedAt:yyyy-MM-dd}");
        }

        var total = await _fineService.GetTotalUnpaidFineAsync(member.MemberId);
        Console.WriteLine(new string('-', 65));
        Console.WriteLine($"Total Pending Balance: ₹{total:F2}");
    }

    private async Task PayFineAsync()
    {
        var members = await _memberService.GetAllMembersAsync();
        var activeMembers = members.Where(m => m.IsActive).ToList();

        if (activeMembers.Count == 0)
        {
            Console.WriteLine("No active members found.");
            return;
        }

        var member = InputValidator.GetSelection(activeMembers, "Select Member to pay fine: ", m => $"{m.FullName} ({m.Email})");
        if (member == null) return;

        var fines = await _fineService.GetPendingFinesAsync(member.MemberId);
        if (fines.Count == 0)
        {
            Console.WriteLine("This member has no pending fines to pay.");
            return;
        }

        var selectedFine = InputValidator.GetSelection(
            fines,
            "Select fine to pay: ",
            f => $"Fine #{f.FineId} | {f.FineType} | Total: ₹{f.Amount:F2} | Remaining Balance: ₹{(f.Amount - f.PaidAmount):F2}"
        );

        if (selectedFine == null) return;

        decimal remaining = selectedFine.Amount - selectedFine.PaidAmount;
        var paymentAmount = InputValidator.GetValidDecimal($"Enter payment amount (Remaining balance is ₹{remaining:F2}): ");

        var result = await _fineService.PayFineAsync(selectedFine.FineId, paymentAmount);
        Console.WriteLine(result);
    }

    private async Task ViewFineHistoryAsync()
    {
        var members = await _memberService.GetAllMembersAsync();
        if (members.Count == 0)
        {
            Console.WriteLine("No members found.");
            return;
        }

        var member = InputValidator.GetSelection(members, "Select Member for fine history: ", m => $"{m.FullName} ({m.Email})");
        if (member == null) return;

        var fines = await _fineService.GetFineHistoryAsync(member.MemberId);
        if (fines.Count == 0)
        {
            Console.WriteLine("No fine history for this member.");
            return;
        }

        Console.WriteLine($"\nFine History for {member.FullName}:");
        Console.WriteLine($"{"Fine ID",-8} {"Type",-22} {"Amount",-10} {"Paid",-10} {"Status",-12} {"Created"}");
        Console.WriteLine(new string('-', 76));
        foreach (var f in fines)
        {
            string status = f.IsPaid ? "PAID" : "UNPAID";
            Console.WriteLine($"{f.FineId,-8} {f.FineType,-22} ₹{f.Amount,-9:F2} ₹{f.PaidAmount,-9:F2} {status,-12} {f.CreatedAt:yyyy-MM-dd}");
        }
    }
}
