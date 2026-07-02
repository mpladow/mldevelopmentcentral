import { createTheme } from '@mui/material/styles';

export type DashboardThemeSettings = {
  primaryColor: string;
  secondaryColor: string;
  backgroundColor: string;
  surfaceColor: string;
  textColor: string;
  borderRadius: number;
};

export const defaultDashboardThemeSettings: DashboardThemeSettings = {
  primaryColor: '#1d4ed8',
  secondaryColor: '#0f766e',
  backgroundColor: '#f6f8fb',
  surfaceColor: '#ffffff',
  textColor: '#111827',
  borderRadius: 8,
};

export function createDashboardTheme(settings: DashboardThemeSettings = defaultDashboardThemeSettings) {
  return createTheme({
    palette: {
      mode: 'light',
      primary: {
        main: settings.primaryColor,
      },
      secondary: {
        main: settings.secondaryColor,
      },
      background: {
        default: settings.backgroundColor,
        paper: settings.surfaceColor,
      },
      text: {
        primary: settings.textColor,
        secondary: '#667085',
      },
      divider: '#e4e7ec',
    },
    shape: {
      borderRadius: settings.borderRadius,
    },
    typography: {
      fontFamily: 'Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif',
      h1: {
        fontSize: '2rem',
        fontWeight: 800,
        letterSpacing: 0,
      },
      h2: {
        fontSize: '1.2rem',
        fontWeight: 800,
        letterSpacing: 0,
      },
      button: {
        fontWeight: 800,
        letterSpacing: 0,
        textTransform: 'none',
      },
    },
    components: {
      MuiButton: {
        defaultProps: {
          disableElevation: true,
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: {
            backgroundImage: 'none',
          },
        },
      },
    },
  });
}

export const dashboardTheme = createDashboardTheme();
