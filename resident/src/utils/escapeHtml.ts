// Shared by generateBillPdf.ts and generateReceiptPdf.ts — both build a print-appropriate
// HTML document from resident/API text and need the same minimal entity escaping.
export function escapeHtml(value: string): string {
  return value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}
