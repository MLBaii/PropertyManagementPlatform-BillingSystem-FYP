import { File, Paths } from 'expo-file-system';
import * as Print from 'expo-print';
import * as Sharing from 'expo-sharing';

import { BillDetail } from '@/services/bills/billsService';
import { formatBillingPeriod, formatShortDate } from '@/utils/formatDate';

type BillPdfInput = {
  bill: BillDetail;
  residentName: string;
};

function escapeHtml(value: string): string {
  return value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

// Print-appropriate styling (white background, serif/sans system fonts) rather than the
// app's dark editorial theme — a PDF meant to be printed or saved needs to read on paper,
// not a phone screen. System fonts only: expo-print renders through the OS's WebView/print
// engine, which can't reliably fetch the app's Google Fonts, and pulling them in would also
// risk missing the ~5s generation target (NFR-03).
function buildBillHtml({ bill, residentName }: BillPdfInput): string {
  const lineItemsHtml = bill.lineItems
    .map((item) => {
      const isPenalty = item.lineItemType === 'Penalty';
      return `
        <tr>
          <td class="desc${isPenalty ? ' penalty' : ''}">${escapeHtml(item.description)}</td>
          <td class="amount${isPenalty ? ' penalty' : ''}">RM ${item.amount.toFixed(2)}</td>
        </tr>`;
    })
    .join('');

  return `<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8" />
<style>
  * { margin: 0; padding: 0; box-sizing: border-box; }
  body {
    font-family: Georgia, 'Times New Roman', serif;
    color: #221C18;
    padding: 40px 48px;
  }
  .sans { font-family: Helvetica, Arial, sans-serif; }
  .masthead {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    border-bottom: 3px solid #D67D5C;
    padding-bottom: 16px;
    margin-bottom: 24px;
  }
  .brand { font-size: 22px; font-weight: 700; color: #D67D5C; letter-spacing: 0.5px; }
  .doc-title {
    font-family: Helvetica, Arial, sans-serif;
    font-size: 11px;
    letter-spacing: 1.5px;
    text-transform: uppercase;
    color: #6B6258;
    margin-top: 4px;
  }
  .meta-grid {
    display: flex;
    justify-content: space-between;
    margin-bottom: 24px;
    font-family: Helvetica, Arial, sans-serif;
  }
  .meta-right { text-align: right; }
  .label {
    font-size: 10px;
    letter-spacing: 1px;
    text-transform: uppercase;
    color: #6B6258;
    margin-bottom: 3px;
  }
  .value { font-size: 13px; color: #221C18; margin-bottom: 10px; }
  .value.muted { color: #6B6258; font-size: 12px; }
  .mono { font-family: 'Courier New', monospace; }
  table { width: 100%; border-collapse: collapse; font-family: Helvetica, Arial, sans-serif; margin-bottom: 8px; }
  thead th {
    text-align: left;
    font-size: 10px;
    letter-spacing: 1px;
    text-transform: uppercase;
    color: #6B6258;
    border-bottom: 1px solid #D8D2C8;
    padding-bottom: 8px;
  }
  th.amount, td.amount { text-align: right; }
  tbody td { font-size: 13px; padding: 10px 0; border-bottom: 1px solid #EDE8E0; }
  td.penalty { color: #B5502F; }
  .total-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-top: 2px solid #221C18;
    margin-top: 8px;
    padding-top: 12px;
    font-family: Helvetica, Arial, sans-serif;
  }
  .total-row span:first-child { font-size: 14px; font-weight: 700; }
  .total-amount { font-size: 20px; font-weight: 700; color: #D67D5C; font-family: 'Courier New', monospace; }
  .due-note {
    font-family: Helvetica, Arial, sans-serif;
    font-size: 11px;
    color: #B5502F;
    margin-top: 6px;
    text-align: right;
  }
  .footer {
    margin-top: 40px;
    padding-top: 16px;
    border-top: 1px solid #EDE8E0;
    font-family: Helvetica, Arial, sans-serif;
    font-size: 10px;
    color: #6B6258;
    text-align: center;
  }
</style>
</head>
<body>
  <div class="masthead">
    <div>
      <div class="brand">PropertyBill</div>
      <div class="doc-title">Billing Statement</div>
    </div>
    <div class="meta-right">
      <div class="label">Reference</div>
      <div class="value mono">${escapeHtml(bill.referenceNumber)}</div>
    </div>
  </div>

  <div class="meta-grid">
    <div>
      <div class="label">Billed To</div>
      <div class="value">${escapeHtml(residentName)}</div>
      <div class="value muted">Unit ${escapeHtml(bill.unitNumber)} · ${escapeHtml(bill.propertyName)}</div>
    </div>
    <div class="meta-right">
      <div class="label">Billing Period</div>
      <div class="value">${formatBillingPeriod(bill.billingPeriod)}</div>
      <div class="label">Issue Date</div>
      <div class="value">${formatShortDate(bill.issueDate)}</div>
    </div>
  </div>

  <table>
    <thead><tr><th>Description</th><th class="amount">Amount</th></tr></thead>
    <tbody>${lineItemsHtml}</tbody>
  </table>

  <div class="total-row">
    <span>Total Due</span>
    <span class="total-amount">RM ${bill.outstandingBalance.toFixed(2)}</span>
  </div>
  <div class="due-note">Due ${formatShortDate(bill.dueDate)}</div>

  <div class="footer">
    Generated by PropertyBill on ${formatShortDate(new Date().toISOString())} &middot; This is a system-generated billing statement.
  </div>
</body>
</html>`;
}

// Renders the bill to a PDF on-device (no server round trip — see §2.4.6) and opens the
// native share sheet. printToFileAsync writes to a randomly-named cache file; expo-file-system
// then copies it to a human-readable Bill_[ReferenceNumber].pdf name before sharing, since the
// filename offered in the share sheet / Files app comes from the file's actual path.
export async function generateAndShareBillPdf(input: BillPdfInput): Promise<void> {
  const html = buildBillHtml(input);
  const { uri } = await Print.printToFileAsync({ html, base64: false });

  const filename = `Bill_${input.bill.referenceNumber}.pdf`;
  const destination = new File(Paths.cache, filename);
  await new File(uri).copy(destination, { overwrite: true });

  const canShare = await Sharing.isAvailableAsync();
  if (canShare) {
    await Sharing.shareAsync(destination.uri, {
      mimeType: 'application/pdf',
      dialogTitle: filename,
      UTI: 'com.adobe.pdf',
    });
  }
}
