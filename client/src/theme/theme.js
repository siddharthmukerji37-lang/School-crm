import { createTheme } from '@mui/material/styles';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1565C0',
      light: '#1E88E5',
      dark: '#0D47A1',
      contrastText: '#ffffff',
    },
    secondary: {
      main: '#7B1FA2',
      light: '#AB47BC',
      dark: '#4A148C',
      contrastText: '#ffffff',
    },
    error: {
      main: '#D32F2F',
      light: '#EF5350',
      dark: '#C62828',
    },
    warning: {
      main: '#F57C00',
      light: '#FFB74D',
      dark: '#E65100',
    },
    info: {
      main: '#0288D1',
      light: '#4FC3F7',
      dark: '#01579B',
    },
    success: {
      main: '#2E7D32',
      light: '#66BB6A',
      dark: '#1B5E20',
    },
    background: {
      default: '#F5F7FA',
      paper: '#FFFFFF',
    },
    text: {
      primary: '#212121',
      secondary: '#616161',
    },
  },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    h1: {
      fontWeight: 700,
      fontSize: '2.5rem',
    },
    h2: {
      fontWeight: 700,
      fontSize: '2rem',
    },
    h3: {
      fontWeight: 600,
      fontSize: '1.75rem',
    },
    h4: {
      fontWeight: 600,
      fontSize: '1.5rem',
    },
    h5: {
      fontWeight: 600,
      fontSize: '1.25rem',
    },
    h6: {
      fontWeight: 600,
      fontSize: '1rem',
    },
    subtitle1: {
      fontSize: '1rem',
      fontWeight: 500,
    },
    subtitle2: {
      fontSize: '0.875rem',
      fontWeight: 500,
    },
    body1: {
      fontSize: '0.875rem',
      lineHeight: 1.6,
    },
    body2: {
      fontSize: '0.8125rem',
      lineHeight: 1.5,
    },
    button: {
      textTransform: 'none',
      fontWeight: 600,
    },
  },
  shape: {
    borderRadius: 8,
  },
  shadows: [
    'none',
    '0px 1px 3px rgba(0,0,0,0.08)',
    '0px 2px 6px rgba(0,0,0,0.08)',
    '0px 4px 12px rgba(0,0,0,0.1)',
    '0px 6px 16px rgba(0,0,0,0.1)',
    '0px 8px 24px rgba(0,0,0,0.12)',
    '0px 10px 32px rgba(0,0,0,0.12)',
    '0px 12px 40px rgba(0,0,0,0.14)',
    '0px 14px 48px rgba(0,0,0,0.14)',
    '0px 16px 56px rgba(0,0,0,0.16)',
    '0px 18px 64px rgba(0,0,0,0.16)',
    '0px 20px 72px rgba(0,0,0,0.18)',
    '0px 22px 80px rgba(0,0,0,0.18)',
    '0px 24px 88px rgba(0,0,0,0.2)',
    '0px 26px 96px rgba(0,0,0,0.2)',
    '0px 28px 104px rgba(0,0,0,0.22)',
    '0px 30px 112px rgba(0,0,0,0.22)',
    '0px 32px 120px rgba(0,0,0,0.24)',
    '0px 34px 128px rgba(0,0,0,0.24)',
    '0px 36px 136px rgba(0,0,0,0.26)',
    '0px 38px 144px rgba(0,0,0,0.26)',
    '0px 40px 152px rgba(0,0,0,0.28)',
    '0px 42px 160px rgba(0,0,0,0.28)',
    '0px 44px 168px rgba(0,0,0,0.3)',
    '0px 46px 176px rgba(0,0,0,0.3)',
    '0px 48px 184px rgba(0,0,0,0.32)',
  ],
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          padding: '8px 20px',
          fontSize: '0.875rem',
          fontWeight: 600,
          boxShadow: 'none',
          '&:hover': {
            boxShadow: '0px 2px 8px rgba(21, 101, 192, 0.3)',
          },
        },
        contained: {
          '&:hover': {
            boxShadow: '0px 4px 12px rgba(21, 101, 192, 0.4)',
          },
        },
        outlined: {
          borderWidth: 2,
          '&:hover': {
            borderWidth: 2,
          },
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          boxShadow: '0px 2px 8px rgba(0,0,0,0.08)',
          border: '1px solid rgba(0,0,0,0.04)',
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          borderRadius: 12,
        },
      },
    },
    MuiTextField: {
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            borderRadius: 8,
          },
        },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        root: {
          padding: '12px 16px',
          fontSize: '0.875rem',
        },
        head: {
          fontWeight: 700,
          backgroundColor: '#F5F7FA',
          color: '#424242',
        },
      },
    },
    MuiDrawer: {
      styleOverrides: {
        paper: {
          border: 'none',
          boxShadow: '2px 0 8px rgba(0,0,0,0.06)',
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          boxShadow: '0px 1px 4px rgba(0,0,0,0.06)',
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          fontWeight: 500,
        },
      },
    },
    MuiTab: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          fontWeight: 600,
          fontSize: '0.875rem',
        },
      },
    },
  },
});

export default theme;
