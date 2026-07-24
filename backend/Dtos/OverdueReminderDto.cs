namespace PropertyBill.Api.Dtos;
public class OverdueReminderDto { public int ResidentId { get; set; } public string ResidentName { get; set; } = string.Empty; public string UnitNumber { get; set; } = string.Empty; public string BillReference { get; set; } = string.Empty; public decimal OutstandingAmount { get; set; } public int DaysOverdue { get; set; } public bool ReminderSent { get; set; } }
