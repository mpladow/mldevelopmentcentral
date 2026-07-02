import {
  Alert,
  Box,
  Button,
  FormControl,
  InputLabel,
  NativeSelect,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import { ArrowLeft, Edit3, Save, UserPlus } from 'lucide-react';
import { FormEvent, useEffect, useState } from 'react';
import type { Account } from '../../types/accounts';
import type { SessionUser } from '../../types/auth';
import { useAccounts } from './useAccounts';

type AccountEditorMode =
  | { type: 'list' }
  | { type: 'create' }
  | { type: 'edit'; account: Account };

type AccountFormState = {
  email: string;
  displayName: string;
  password: string;
  role: string;
};

type AccountsViewProps = {
  token: string;
  user: SessionUser;
};

const emptyAccountForm: AccountFormState = {
  email: '',
  displayName: '',
  password: '',
  role: 'User',
};

export function AccountsView({ token, user }: AccountsViewProps) {
  const [mode, setMode] = useState<AccountEditorMode>({ type: 'list' });
  const [form, setForm] = useState(emptyAccountForm);
  const canManageAccounts = user.roles.includes('Admin');
  const { accounts, message, clearMessage, loadAccounts, createAccount, updateAccount } = useAccounts(token);

  useEffect(() => {
    void loadAccounts();
  }, [loadAccounts]);

  function openCreateAccount() {
    clearMessage();
    setForm(emptyAccountForm);
    setMode({ type: 'create' });
  }

  function openEditAccount(account: Account) {
    clearMessage();
    setForm({
      email: account.email,
      displayName: account.displayName,
      password: '',
      role: account.roles[0] ?? 'User',
    });
    setMode({ type: 'edit', account });
  }

  function closeEditor() {
    clearMessage();
    setMode({ type: 'list' });
  }

  async function handleCreateAccount(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    clearMessage();

    const created = await createAccount({
      email: form.email,
      displayName: form.displayName,
      password: form.password,
      roles: [form.role],
    });

    if (!created) {
      return;
    }

    setForm(emptyAccountForm);
    setMode({ type: 'list' });
  }

  async function handleUpdateAccount(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (mode.type !== 'edit') {
      return;
    }

    clearMessage();

    const updated = await updateAccount(mode.account.id, {
      email: form.email,
      displayName: form.displayName,
      roles: [form.role],
    });

    if (!updated) {
      return;
    }

    setMode({ type: 'list' });
  }

  if (mode.type !== 'list') {
    const isCreateMode = mode.type === 'create';

    return (
      <Paper component="section" sx={{ display: 'grid', gap: 3, maxWidth: 760, p: 3 }} variant="outlined">
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Box>
            <Typography component="h2" variant="h2">{isCreateMode ? 'Create account' : 'Edit account'}</Typography>
            <Typography color="text.secondary">
              {isCreateMode ? 'Admin-created accounts only.' : 'Update account details and role.'}
            </Typography>
          </Box>
          <Button onClick={closeEditor} startIcon={<ArrowLeft size={16} />} type="button" variant="outlined">
            Back
          </Button>
        </Stack>

        <Box
          component="form"
          onSubmit={isCreateMode ? handleCreateAccount : handleUpdateAccount}
          sx={{ display: 'grid', gap: 2, maxWidth: 520 }}
        >
          <TextField
            label="Email address"
            onChange={(event) => setForm((value) => ({ ...value, email: event.target.value }))}
            required
            slotProps={{ htmlInput: { 'aria-label': 'Account email' } }}
            type="email"
            value={form.email}
          />
          <TextField
            label="Display name"
            onChange={(event) => setForm((value) => ({ ...value, displayName: event.target.value }))}
            required
            slotProps={{ htmlInput: { 'aria-label': 'Account display name' } }}
            type="text"
            value={form.displayName}
          />
          {isCreateMode && (
            <TextField
              label="Temporary password"
              onChange={(event) => setForm((value) => ({ ...value, password: event.target.value }))}
              required
              slotProps={{ htmlInput: { 'aria-label': 'Account temporary password' } }}
              type="password"
              value={form.password}
            />
          )}
          <FormControl>
            <InputLabel variant="standard">Role</InputLabel>
            <NativeSelect
              inputProps={{ 'aria-label': 'Account role' }}
              onChange={(event) => setForm((value) => ({ ...value, role: event.target.value }))}
              value={form.role}
            >
              <option value="Admin">Admin</option>
              <option value="User">User</option>
              <option value="Viewer">Viewer</option>
            </NativeSelect>
          </FormControl>
          <Button startIcon={isCreateMode ? <UserPlus size={16} /> : <Save size={16} />} type="submit" variant="contained">
            {isCreateMode ? 'Create account' : 'Save account'}
          </Button>
        </Box>

        {message && <Alert severity="info">{message}</Alert>}
      </Paper>
    );
  }

  return (
    <Paper component="section" sx={{ display: 'grid', gap: 2.5, p: 3 }} variant="outlined">
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ alignItems: 'flex-start', justifyContent: 'space-between' }}>
        <Box>
          <Typography component="h2" variant="h2">Accounts</Typography>
          <Typography color="text.secondary">Global users with assigned roles.</Typography>
        </Box>
        <Stack direction="row" spacing={1}>
          <Button onClick={loadAccounts} type="button" variant="outlined">Refresh</Button>
          {canManageAccounts && (
            <Button onClick={openCreateAccount} startIcon={<UserPlus size={16} />} type="button" variant="contained">
              Create account
            </Button>
          )}
        </Stack>
      </Stack>

      {message && <Alert severity="info">{message}</Alert>}

      <TableContainer aria-label="Accounts grid">
        <Table sx={{ minWidth: canManageAccounts ? 720 : 620 }}>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Email</TableCell>
              <TableCell>Roles</TableCell>
              {canManageAccounts && <TableCell align="right">Actions</TableCell>}
            </TableRow>
          </TableHead>
          <TableBody>
            {accounts.length === 0 ? (
              <TableRow>
                <TableCell colSpan={canManageAccounts ? 4 : 3}>
                  <Typography color="text.secondary">No accounts loaded.</Typography>
                </TableCell>
              </TableRow>
            ) : (
              accounts.map((account) => (
                <TableRow key={account.id}>
                  <TableCell>
                    <Typography sx={{ fontWeight: 800 }}>{account.displayName}</Typography>
                  </TableCell>
                  <TableCell>{account.email}</TableCell>
                  <TableCell>{account.roles.join(', ')}</TableCell>
                  {canManageAccounts && (
                    <TableCell align="right">
                      <Button
                        aria-label={`Edit ${account.displayName}`}
                        onClick={() => openEditAccount(account)}
                        startIcon={<Edit3 size={16} />}
                        type="button"
                        variant="outlined"
                      >
                        Edit
                      </Button>
                    </TableCell>
                  )}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  );
}
