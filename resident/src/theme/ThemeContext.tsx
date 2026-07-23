import * as SecureStore from 'expo-secure-store';
import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';

import { palettes, ThemeColors, ThemeName } from './colors';

const THEME_STORAGE_KEY = 'propertybill_theme';

type ThemeContextValue = {
  themeName: ThemeName;
  colors: ThemeColors;
  isLoaded: boolean;
  setThemeName: (name: ThemeName) => void;
  toggleTheme: () => void;
};

const ThemeContext = createContext<ThemeContextValue | undefined>(undefined);

// Dark is the app's original/default look — matches every screen's design before this
// toggle existed, so an install that's never touched the setting stays exactly as-is.
export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [themeName, setThemeNameState] = useState<ThemeName>('dark');
  const [isLoaded, setIsLoaded] = useState(false);

  useEffect(() => {
    SecureStore.getItemAsync(THEME_STORAGE_KEY)
      .then((stored) => {
        if (stored === 'light' || stored === 'dark') {
          setThemeNameState(stored);
        }
      })
      .finally(() => setIsLoaded(true));
  }, []);

  const setThemeName = useCallback((name: ThemeName) => {
    setThemeNameState(name);
    void SecureStore.setItemAsync(THEME_STORAGE_KEY, name);
  }, []);

  const toggleTheme = useCallback(() => {
    setThemeNameState((prev) => {
      const next: ThemeName = prev === 'dark' ? 'light' : 'dark';
      void SecureStore.setItemAsync(THEME_STORAGE_KEY, next);
      return next;
    });
  }, []);

  const value = useMemo(
    () => ({ themeName, colors: palettes[themeName], isLoaded, setThemeName, toggleTheme }),
    [themeName, isLoaded, setThemeName, toggleTheme]
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext);
  if (!ctx) {
    throw new Error('useTheme must be used within a ThemeProvider');
  }
  return ctx;
}
