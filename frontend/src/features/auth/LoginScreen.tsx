import { Alert, Avatar, Box, Button, Paper, Stack, TextField, Typography } from '@mui/material';
import { LockKeyhole } from 'lucide-react';
import { FormEvent, useState } from 'react';
import { useAuth } from './AuthContext';
import { useLogin } from './useLogin';

export function LoginScreen() {
  const [email, setEmail] = useState('ml.development.2022@gmail.com');
  const [password, setPassword] = useState('');
  const { login: completeLogin } = useAuth();
  const { login, error, isLoading } = useLogin();

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    try {
      const result = await login(email, password);
      completeLogin(result);
    } catch {
      // Error state is managed by the hook.
    }
  }

  return (
    <Box
      component="main"
      sx={{
        alignItems: 'center',
        bgcolor: 'background.default',
        display: 'grid',
        minHeight: '100vh',
        p: 2,
      }}
    >
      <Paper
        aria-label="Login"
        component="section"
        elevation={0}
        sx={{ justifySelf: 'center', maxWidth: 460, p: { xs: 3, sm: 4 }, width: '100%' }}
        variant="outlined"
      >
        <Stack spacing={3}>
          <Stack direction="row" spacing={1.5} sx={{ alignItems: 'center' }}>
            <Avatar sx={{ bgcolor: 'primary.main' }}>
              <LockKeyhole size={22} />
            </Avatar>
            <Box>
              <Typography color="primary" sx={{ fontWeight: 900, textTransform: 'uppercase' }} variant="caption">
                Global
              </Typography>
              <Typography component="h1" variant="h1">ML Dev Dashboard</Typography>
            </Box>
          </Stack>

          <Box component="form" onSubmit={handleSubmit} sx={{ display: 'grid', gap: 2 }}>
            <TextField
              autoComplete="email"
              label="Email address"
              name="email"
              onChange={(event) => setEmail(event.target.value)}
              required
              type="email"
              value={email}
            />

            <TextField
              autoComplete="current-password"
              label="Password"
              name="password"
              onChange={(event) => setPassword(event.target.value)}
              required
              type="password"
              value={password}
            />

            {error && <Alert severity="error">{error}</Alert>}

            <Button disabled={isLoading} type="submit" variant="contained">
              {isLoading ? 'Signing in' : 'Sign in'}
            </Button>
          </Box>
        </Stack>
      </Paper>
    </Box>
  );
}
