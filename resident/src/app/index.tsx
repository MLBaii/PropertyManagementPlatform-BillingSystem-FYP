import { Redirect } from 'expo-router';
import { ActivityIndicator, View } from 'react-native';

import { useAuth } from '@/services/auth/AuthContext';
import { colors } from '@/theme';

// Auth gate: sends unauthenticated residents to the login flow, authenticated
// residents straight into the tabs. Nothing else should render at this route.
export default function Index() {
  const { resident, isLoading } = useAuth();

  if (isLoading) {
    return (
      <View style={{ flex: 1, alignItems: 'center', justifyContent: 'center', backgroundColor: colors.background }}>
        <ActivityIndicator color={colors.accent} />
      </View>
    );
  }

  return <Redirect href={resident ? '/(tabs)/home' : '/(auth)/login'} />;
}
