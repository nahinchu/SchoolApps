using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Models;

namespace SchoolApp.Repositories.PaymentRepository
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context) : base(context) { }

        public Payment? GetByOrderCode(long orderCode)
        {
            return _context.Payments
                .Include(p => p.User)
                .Include(p => p.Course)
                .FirstOrDefault(p => p.OrderCode == orderCode);
        }

        public IQueryable<Payment> GetByStudent(int userId)
        {
            return _context.Payments
                .Include(p => p.Course)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt);
        }
    }
}
