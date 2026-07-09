// Split out from secureStorage.ts/client.ts so both can reference these keys
// without importing from each other (client.ts needs secureStorage.clearSession
// for the 401 handler, which would otherwise be a circular import).
export const AUTH_TOKEN_KEY = 'propertybill_auth_token';
export const RESIDENT_INFO_KEY = 'propertybill_resident_info';
