using DataAccessLayer.Entities;

namespace BusinessLogicLayer.Services;

public interface IMemberService
{
    Task<string> AddMemberAsync(string fullName, string email, string phoneNumber, string membershipType);
    Task<List<Member>> GetAllMembersAsync();
    Task<Member?> SearchMemberAsync(string searchTerm);
    Task<string> UpdateMembershipTypeAsync(int memberId, string newMembershipType);
    Task<string> DeactivateMemberAsync(int memberId);
    Task<string> ReactivateMemberAsync(int memberId);
    Task<List<MembershipConfig>> GetAllMembershipConfigsAsync();
    Task<Member?> GetMemberByIdAsync(int id);
    Task AddMemberAsync(Member member);
}
