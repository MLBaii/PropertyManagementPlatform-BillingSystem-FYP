// Bridges the axios response interceptor (a module-level singleton, outside the
// React tree) to AuthContext's in-memory `resident` state. Without this, clearing
// SecureStore on a 401 wouldn't be enough — the (auth)/(tabs) route guards check
// AuthContext's state, not SecureStore directly, so the guard would still think
// the user is logged in until next app launch.
let handler: (() => void) | null = null;

export function setSessionExpiredHandler(fn: (() => void) | null): void {
  handler = fn;
}

export function notifySessionExpired(): void {
  handler?.();
}
