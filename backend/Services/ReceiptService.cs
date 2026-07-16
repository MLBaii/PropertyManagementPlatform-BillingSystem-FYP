using PropertyBill.Api.Dtos;
using PropertyBill.Api.Models;
using PropertyBill.Api.Repositories;

namespace PropertyBill.Api.Services;

public class ReceiptService : IReceiptService
{
    private readonly IPaymentRepository _paymentRepository;

    public ReceiptService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<List<ReceiptDto>> GetReceiptsAsync(int unitId)
    {
        var payments = await _paymentRepository.GetConfirmedByUnitIdAsync(unitId);
        return payments.Select(ToDto).ToList();
    }

    public async Task<ReceiptDetailDto?> GetReceiptDetailAsync(int paymentId, int unitId)
    {
        var payment = await _paymentRepository.GetConfirmedByIdForUnitAsync(paymentId, unitId);
        if (payment is null)
        {
            return null;
        }

        return new ReceiptDetailDto
        {
            PaymentId = payment.PaymentId,
            ReceiptNumber = BuildReceiptNumber(payment.PaymentId),
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            Channel = payment.Channel,
            BillReferenceNumber = payment.Bill.ReferenceNumber,
            BillingPeriod = payment.Bill.BillingPeriod,
            UnitNumber = payment.Bill.Unit.UnitNumber,
            PropertyName = payment.Bill.Unit.Property.Name,
        };
    }

    private static ReceiptDto ToDto(Payment payment)
    {
        return new ReceiptDto
        {
            PaymentId = payment.PaymentId,
            ReceiptNumber = BuildReceiptNumber(payment.PaymentId),
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            Channel = payment.Channel,
            BillReferenceNumber = payment.Bill.ReferenceNumber,
            BillingPeriod = payment.Bill.BillingPeriod,
        };
    }

    private static string BuildReceiptNumber(int paymentId) => $"RCPT-{paymentId:D6}";
}
