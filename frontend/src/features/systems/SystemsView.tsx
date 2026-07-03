import {
  Alert,
  Box,
  Button,
  Checkbox,
  FormControl,
  FormControlLabel,
  FormGroup,
  FormLabel,
  MenuItem,
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
import { ArrowLeft, Edit3, Save, Server } from 'lucide-react';
import { FormEvent, useEffect, useState } from 'react';
import type { DashboardSystem } from '../../types/systems';
import { useRoles } from '../roles/useRoles';
import { useSystems } from './useSystems';

type SystemEditorMode =
  | { type: 'list' }
  | { type: 'create' }
  | { type: 'edit'; system: DashboardSystem };

type SystemFormState = {
  label: string;
  accounts: SystemAccountAssignment[];
};

type SystemAccountAssignment = {
  accountId: string;
  role: string;
};

type SystemsViewProps = {
  onSystemsChanged: () => void;
  token: string;
};

const emptySystemForm: SystemFormState = {
  label: '',
  accounts: [],
};

export function SystemsView({ onSystemsChanged, token }: SystemsViewProps) {
  const [mode, setMode] = useState<SystemEditorMode>({ type: 'list' });
  const [form, setForm] = useState<SystemFormState>(emptySystemForm);
  const { systems, accounts, message, loadSystems, loadAccounts, saveSystem } = useSystems(token);
  const { roles, loadRoles } = useRoles(token);
  const roleOptions = getRoleOptions();
  const defaultRole = roleOptions[0]?.name ?? '';

  useEffect(() => {
    void loadSystems();
    void loadAccounts();
    void loadRoles();
  }, [loadAccounts, loadRoles, loadSystems]);

  function openCreateSystem() {
    setForm(emptySystemForm);
    setMode({ type: 'create' });
  }

  function openEditSystem(system: DashboardSystem) {
    setForm({
      label: system.label,
      accounts: system.accounts.map((account) => ({
        accountId: account.id,
        role: account.role || getSystemRoles(system.id)[0]?.name || '',
      })),
    });
    setMode({ type: 'edit', system });
  }

  function closeEditor() {
    setMode({ type: 'list' });
  }

  function isAccountSelected(accountId: string) {
    return form.accounts.some((account) => account.accountId === accountId);
  }

  function toggleAccount(accountId: string) {
    setForm((value) => ({
      ...value,
      accounts: value.accounts.some((account) => account.accountId === accountId)
        ? value.accounts.filter((account) => account.accountId !== accountId)
        : [...value.accounts, { accountId, role: defaultRole }],
    }));
  }

  function updateAccountRole(accountId: string, role: string) {
    setForm((value) => ({
      ...value,
      accounts: value.accounts.map((account) =>
        account.accountId === accountId ? { ...account, role } : account,
      ),
    }));
  }

  function getSelectedAccountRole(accountId: string) {
    return form.accounts.find((account) => account.accountId === accountId)?.role ?? defaultRole;
  }

  function getRoleOptions() {
    if (mode.type === 'edit') {
      return getSystemRoles(mode.system.id);
    }

    return getDefaultRoleNames(form.label).map((name) => ({
      id: name,
      name,
      displayName: name,
    }));
  }

  function getSystemRoles(systemId: number) {
    return roles
      .filter((role) => role.systemId === systemId)
      .map((role) => ({
        id: role.id,
        name: role.name,
        displayName: role.displayName,
      }));
  }

  function getDefaultRoleNames(systemLabel: string) {
    const prefix = systemLabel.replace(/[^a-zA-Z0-9]/g, '') || 'System';
    return [`${prefix}Admin`, `${prefix}User`, `${prefix}Viewer`];
  }

  async function saveSystemHandler(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const systemId = mode.type === 'edit' ? mode.system.id : undefined;
    const didSave = await saveSystem({
      label: form.label,
      accounts: form.accounts,
    }, systemId);

    if (!didSave) {
      return;
    }

    setMode({ type: 'list' });
    onSystemsChanged();
  }

  if (mode.type !== 'list') {
    const isCreateMode = mode.type === 'create';

    return (
      <Paper component="section" sx={{ display: 'grid', gap: 3, maxWidth: 900, p: 3 }} variant="outlined">
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Box>
            <Typography component="h2" variant="h2">{isCreateMode ? 'Create system' : 'Edit system'}</Typography>
            <Typography color="text.secondary">
              {isCreateMode ? 'Name the system and assign account access.' : 'Update the system name and account access.'}
            </Typography>
          </Box>
          <Button onClick={closeEditor} startIcon={<ArrowLeft size={16} />} type="button" variant="outlined">
            Back
          </Button>
        </Stack>

        <Box component="form" onSubmit={saveSystemHandler} sx={{ display: 'grid', gap: 2.5 }}>
          <TextField
            aria-label="System name"
            label="System name"
            onChange={(event) => {
              const nextLabel = event.target.value;
              setForm((value) => ({
                ...value,
                accounts: mode.type === 'create'
                  ? value.accounts.map((account) => ({
                    ...account,
                    role: getDefaultRoleNames(nextLabel)[0],
                  }))
                  : value.accounts,
                label: nextLabel,
              }));
            }}
            required
            sx={{ maxWidth: 520 }}
            type="text"
            value={form.label}
          />

          <FormControl component="fieldset" variant="standard">
            <FormLabel component="legend">Account access</FormLabel>
            <FormGroup sx={{ display: 'grid', gap: 1.25, mt: 1 }}>
              {accounts.length === 0 ? (
                <Typography color="text.secondary">No accounts available.</Typography>
              ) : (
                accounts.map((account) => (
                  <Paper
                    key={account.id}
                    sx={{
                      alignItems: { xs: 'stretch', sm: 'center' },
                      display: 'grid',
                      gap: 1.5,
                      gridTemplateColumns: { xs: '1fr', sm: 'minmax(0, 1fr) minmax(140px, 180px)' },
                      p: 1.5,
                    }}
                    variant="outlined"
                  >
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={isAccountSelected(account.id)}
                          onChange={() => toggleAccount(account.id)}
                        />
                      }
                      label={(
                        <Box sx={{ minWidth: 0 }}>
                          <Typography sx={{ fontWeight: 800 }} noWrap>{account.displayName}</Typography>
                          <Typography color="text.secondary" variant="caption" noWrap>{account.email}</Typography>
                        </Box>
                      )}
                    />
                    <TextField
                      aria-label={`${account.displayName} system role`}
                      disabled={!isAccountSelected(account.id)}
                      onChange={(event) => updateAccountRole(account.id, event.target.value)}
                      select
                      size="small"
                      value={getSelectedAccountRole(account.id)}
                    >
                      {roleOptions.map((role) => (
                        <MenuItem key={role.id} value={role.name}>{role.displayName}</MenuItem>
                      ))}
                    </TextField>
                  </Paper>
                ))
              )}
            </FormGroup>
          </FormControl>

          <Button
            startIcon={isCreateMode ? <Server size={16} /> : <Save size={16} />}
            sx={{ justifySelf: 'start' }}
            type="submit"
            variant="contained"
          >
            {isCreateMode ? 'Create system' : 'Save system'}
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
          <Typography component="h2" variant="h2">Systems</Typography>
          <Typography color="text.secondary">Dashboard systems and account access.</Typography>
        </Box>
        <Stack direction="row" spacing={1}>
          <Button onClick={loadSystems} type="button" variant="outlined">Refresh</Button>
          <Button onClick={openCreateSystem} startIcon={<Server size={16} />} type="button" variant="contained">
            Create system
          </Button>
        </Stack>
      </Stack>

      {message && <Alert severity="info">{message}</Alert>}

      <TableContainer aria-label="Systems grid">
        <Table sx={{ minWidth: 760 }}>
          <TableHead>
            <TableRow>
              <TableCell>System</TableCell>
              <TableCell>Key</TableCell>
              <TableCell>Accounts</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {systems.length === 0 ? (
              <TableRow>
                <TableCell colSpan={4}>
                  <Typography color="text.secondary">No systems loaded.</Typography>
                </TableCell>
              </TableRow>
            ) : (
              systems.map((system) => (
                <TableRow key={system.id}>
                  <TableCell>
                    <Typography sx={{ fontWeight: 800 }}>{system.label}</Typography>
                  </TableCell>
                  <TableCell>{system.systemKey}</TableCell>
                  <TableCell>
                    {system.accounts.length === 0
                      ? 'No accounts assigned'
                      : system.accounts.map((account) => `${account.displayName} (${account.role})`).join(', ')}
                  </TableCell>
                  <TableCell align="right">
                    <Button
                      aria-label={`Edit ${system.label}`}
                      onClick={() => openEditSystem(system)}
                      startIcon={<Edit3 size={16} />}
                      type="button"
                      variant="outlined"
                    >
                      Edit
                    </Button>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Paper>
  );
}
