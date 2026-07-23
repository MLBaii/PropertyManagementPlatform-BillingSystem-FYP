// Dedicated Payments feature API model and review service. It is kept separate from
// the shared sidebar shell so Payments cannot overwrite another page's content.
export interface PaymentProofReview { proofId:number; residentName:string; unitNumber:string; fileUrl:string; status:string; submittedAt:string; adminRemarks?:string; }

export async function reviewPaymentProof(token:string, proofId:number, decision:'Confirmed'|'Rejected'):Promise<void> {
  const response=await fetch(`http://localhost:5112/api/admin/payment-proofs/${proofId}/review`,{method:'PUT',headers:{Authorization:`Bearer ${token}`,'Content-Type':'application/json'},body:JSON.stringify({decision})});
  if(!response.ok) throw new Error('Unable to review payment proof.');
}
