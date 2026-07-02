import { Box, Paper, Stack, Typography } from '@mui/material';
import type { ActiveView } from '../types/navigation';

type DashboardHeaderProps = {
  activeView: ActiveView;
};

export function DashboardHeader({ activeView }: DashboardHeaderProps) {
  const activeTitle = activeView === 'accounts'
    ? 'Accounts'
    : activeView === 'systems'
      ? 'Systems'
      : 'Roles';

  return (
    <Box
      component="section"
      sx={{
        alignItems: { xs: 'flex-start', lg: 'center' },
        display: 'flex',
        flexDirection: { xs: 'column', lg: 'row' },
        gap: 2,
        justifyContent: 'space-between',
      }}
    >
      <Box>
        <Typography color="primary" sx={{ fontWeight: 900, textTransform: 'uppercase' }} variant="caption">
          Global
        </Typography>
        <Typography component="h1" variant="h1">
          {activeTitle}
        </Typography>
      </Box>
      <Stack
        aria-label="Global summary"
        direction={{ xs: 'column', sm: 'row' }}
        spacing={1.5}
        sx={{ width: { xs: '100%', lg: 'auto' } }}
      >
        {[
          ['System', 'Global'],
          ['Mode', 'Admin'],
          ['Auth', 'Identity'],
        ].map(([label, value]) => (
          <Paper
            component="article"
            key={label}
            variant="outlined"
            sx={{
              minWidth: 150,
              px: 2,
              py: 1.5,
            }}
          >
            <Typography color="text.secondary" sx={{ fontWeight: 800 }} variant="caption">
              {label}
            </Typography>
            <Typography sx={{ fontWeight: 900 }}>{value}</Typography>
          </Paper>
        ))}
      </Stack>
    </Box>
  );
}
