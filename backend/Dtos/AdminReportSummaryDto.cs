namespace PropertyBill.Api.Dtos;
public class AdminReportSummaryDto { public decimal Current { get; set; } public decimal Days1To30 { get; set; } public decimal Days31To60 { get; set; } public decimal Days61To90 { get; set; } public decimal Over90Days { get; set; } public decimal TotalOutstanding { get; set; } }
