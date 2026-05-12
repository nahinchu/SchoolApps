using SchoolApp.Models;

namespace SchoolApp.Repositories.PaymentRepository
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Payment? GetByOrderCode(long orderCode);
        IQueryable<Payment> GetByStudent(int studentId);

    }
}
