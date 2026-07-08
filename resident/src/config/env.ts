// Set EXPO_PUBLIC_API_BASE_URL in a .env file at the project root (see .env.example) —
// EXPO_PUBLIC_ vars are inlined at bundle time, so restart `npx expo start` after changing it.
//
// Which host to use depends on where the app is running relative to the backend:
// - Android emulator : http://10.0.2.2:<port>/api        (10.0.2.2 is the emulator's alias for your dev machine)
// - iOS simulator     : http://localhost:<port>/api        (simulator shares the host's network stack)
// - Physical device   : http://<dev-machine-LAN-IP>:<port>/api  (device and dev machine must be on the same Wi-Fi)
const FALLBACK_API_BASE_URL = 'http://10.0.2.2:5112/api';

export const API_BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL ?? FALLBACK_API_BASE_URL;
