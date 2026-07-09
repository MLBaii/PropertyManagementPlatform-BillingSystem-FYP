// Decodes only — does not verify the signature (that's the backend's job on every
// request). This is purely a client-side "is it worth even sending?" check, used to
// avoid silently resuming a session with a token that's already expired.
const BASE64_CHARS = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';

function base64UrlDecode(input: string): string {
  const base64 = input.replace(/-/g, '+').replace(/_/g, '/');
  let output = '';
  let buffer = 0;
  let bits = 0;

  for (const char of base64) {
    const value = BASE64_CHARS.indexOf(char);
    if (value === -1) {
      continue;
    }
    buffer = (buffer << 6) | value;
    bits += 6;
    if (bits >= 8) {
      bits -= 8;
      output += String.fromCharCode((buffer >> bits) & 0xff);
    }
  }

  return output;
}

// Returns the token's `exp` claim in epoch milliseconds, or null if it can't be read.
export function getTokenExpiryMs(token: string): number | null {
  try {
    const payloadSegment = token.split('.')[1];
    if (!payloadSegment) {
      return null;
    }
    const payload = JSON.parse(base64UrlDecode(payloadSegment)) as { exp?: number };
    return typeof payload.exp === 'number' ? payload.exp * 1000 : null;
  } catch {
    return null;
  }
}

export function isTokenExpired(token: string): boolean {
  const expiryMs = getTokenExpiryMs(token);
  // Can't read an expiry claim at all -> treat as expired/invalid rather than trusting it.
  if (expiryMs === null) {
    return true;
  }
  return Date.now() >= expiryMs;
}
