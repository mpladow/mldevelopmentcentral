import { Alert, Box, Button, Card, CardContent, Stack, Typography } from '@mui/material';
import { ShieldCheck } from 'lucide-react';
import { useMemo } from 'react';
import { useRoles } from './useRoles';

type RolesViewProps = {
  token: string;
};

export function RolesView({ token }: RolesViewProps) {
  const { roles, message, loadRoles } = useRoles(token);

  const roleDescriptions = useMemo(
    () => ({
      Admin: 'Can create accounts and manage Global reference access.',
      User: 'Can work in assigned systems after access is configured.',
      Viewer: 'Read-only access for dashboard and reference views.',
    }),
    [],
  );

  return (
    <Box component="section" sx={{ display: 'grid', gap: 2.5 }}>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ alignItems: 'flex-start', justifyContent: 'space-between' }}>
        <Box>
          <Typography component="h2" variant="h2">Roles</Typography>
          <Typography color="text.secondary">Initial Global permission groups.</Typography>
        </Box>
        <Button onClick={loadRoles} type="button" variant="outlined">Refresh</Button>
      </Stack>

      {message && <Alert severity="info">{message}</Alert>}

      <Box
        sx={{
          display: 'grid',
          gap: 2,
          gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
        }}
      >
        {(roles.length > 0 ? roles : ['Admin', 'User', 'Viewer']).map((role) => (
          <Card key={role} variant="outlined">
            <CardContent sx={{ display: 'grid', gap: 1.25 }}>
              <Box sx={{ color: 'primary.main' }}>
                <ShieldCheck size={24} />
              </Box>
              <Typography component="strong" sx={{ fontWeight: 900 }}>{role}</Typography>
              <Typography color="text.secondary">
                {roleDescriptions[role as keyof typeof roleDescriptions]}
              </Typography>
            </CardContent>
          </Card>
        ))}
      </Box>
    </Box>
  );
}
