import { apiClient } from '@/services/api/client';

export type PropertyContact = {
  propertyName: string;
  contactEmail: string;
  contactPhone: string;
};

// Unauthenticated on purpose — called from the Login screen's "Forgot Password?" card,
// before a resident has a session.
export async function getPropertyContact(): Promise<PropertyContact> {
  const { data } = await apiClient.get<PropertyContact>('/property/contact');
  return data;
}
