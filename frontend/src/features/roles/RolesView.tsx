import {
  Alert,
  Box,
  Button,
  Checkbox,
  FormControl,
  FormControlLabel,
  FormGroup,
  InputLabel,
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
import { ArrowLeft, Edit3, Plus, Save } from 'lucide-react';
import { FormEvent, useEffect, useMemo, useState } from 'react';
import type { Role } from '../../types/roles';
import { useRoles } from './useRoles';

type RolesViewProps = {
  token: string;
};

type RoleEditorMode =
  | { type: 'list' }
  | { type: 'create' }
  | { type: 'edit'; role: Role };

type RoleFormState = {
  systemId: number;
  name: string;
  displayName: string;
  permissions: string[];
};

const emptyRoleForm: RoleFormState = {
  systemId: 0,
  name: '',
  displayName: '',
  permissions: [],
};

export function RolesView({ token }: RolesViewProps) {
  const {
    createRole,
    loadPermissions,
    loadRoles,
    message,
    permissions,
    roles,
    updateRole,
  } = useRoles(token);
  const [mode, setMode] = useState<RoleEditorMode>({ type: 'list' });
  const [form, setForm] = useState<RoleFormState>(emptyRoleForm);

  const systems = useMemo(
    () => Array.from(
      new Map(roles.map((role) => [role.systemId, {
        id: role.systemId,
        label: role.systemLabel,
      }])).values(),
    ),
    [roles],
  );

  const editablePermissions = permissions.filter((permission) => permission.systemId === form.systemId);

  useEffect(() => {
    void loadRoles();
    void loadPermissions();
  }, [loadPermissions, loadRoles]);

  function openCreateRole() {
    const systemId = systems[0]?.id ?? 0;
    setForm({ ...emptyRoleForm, systemId });
    setMode({ type: 'create' });
  }

  function openEditRole(role: Role) {
    setForm({
      displayName: role.displayName,
      name: role.name,
      permissions: role.permissions,
      systemId: role.systemId,
    });
    setMode({ type: 'edit', role });
  }

  function closeEditor() {
    setMode({ type: 'list' });
  }

  function togglePermission(permissionKey: string) {
    setForm((value) => ({
      ...value,
      permissions: value.permissions.includes(permissionKey)
        ? value.permissions.filter((permission) => permission !== permissionKey)
        : [...value.permissions, permissionKey],
    }));
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (mode.type === 'list') {
      return;
    }

    const didSave = mode.type === 'create'
      ? await createRole(form)
      : await updateRole(mode.role.id, {
        displayName: form.displayName,
        permissions: form.permissions,
      });

    if (didSave) {
      setMode({ type: 'list' });
    }
  }

  if (mode.type !== 'list') {
    const isCreateMode = mode.type === 'create';

    return (
      <Paper component="section" sx={{ display: 'grid', gap: 3, maxWidth: 900, p: 3 }} variant="outlined">
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Box>
            <Typography component="h2" variant="h2">{isCreateMode ? 'Create role' : 'Edit role'}</Typography>
            <Typography color="text.secondary">Assign code-defined permissions to a system-scoped role.</Typography>
          </Box>
          <Button onClick={closeEditor} startIcon={<ArrowLeft size={16} />} type="button" variant="outlined">
            Back
          </Button>
        </Stack>

        <Box component="form" onSubmit={handleSubmit} sx={{ display: 'grid', gap: 2.5, maxWidth: 640 }}>
          <FormControl>
            <TextField
              disabled={!isCreateMode}
              label="System"
              onChange={(event) => setForm((value) => ({ ...value, systemId: Number(event.target.value) }))}
              select
              value={form.systemId}
            >
              {systems.map((system) => (
                <MenuItem key={system.id} value={system.id}>{system.label}</MenuItem>
              ))}
            </TextField>
          </FormControl>
          <TextField
            disabled={!isCreateMode}
            label="Role name"
            onChange={(event) => setForm((value) => ({ ...value, name: event.target.value }))}
            required
            value={form.name}
          />
          <TextField
            label="Display name"
            onChange={(event) => setForm((value) => ({ ...value, displayName: event.target.value }))}
            required
            value={form.displayName}
          />

          <FormControl component="fieldset" variant="standard">
            <InputLabel shrink>Permissions</InputLabel>
            <FormGroup sx={{ display: 'grid', gap: 1, mt: 3 }}>
              {editablePermissions.length === 0 ? (
                <Typography color="text.secondary">No permissions are registered for this system.</Typography>
              ) : (
                editablePermissions.map((permission) => (
                  <FormControlLabel
                    control={(
                      <Checkbox
                        checked={form.permissions.includes(permission.permissionKey)}
                        onChange={() => togglePermission(permission.permissionKey)}
                      />
                    )}
                    key={permission.id}
                    label={`${permission.displayName} (${permission.permissionKey})`}
                  />
                ))
              )}
            </FormGroup>
          </FormControl>

          <Button startIcon={isCreateMode ? <Plus size={16} /> : <Save size={16} />} sx={{ justifySelf: 'start' }} type="submit" variant="contained">
            {isCreateMode ? 'Create role' : 'Save role'}
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
          <Typography component="h2" variant="h2">Roles</Typography>
          <Typography color="text.secondary">System-scoped roles and assigned permissions.</Typography>
        </Box>
        <Stack direction="row" spacing={1}>
          <Button onClick={loadRoles} type="button" variant="outlined">Refresh</Button>
          <Button disabled={systems.length === 0} onClick={openCreateRole} startIcon={<Plus size={16} />} type="button" variant="contained">
            Create role
          </Button>
        </Stack>
      </Stack>

      {message && <Alert severity="info">{message}</Alert>}

      <TableContainer aria-label="Roles grid">
        <Table sx={{ minWidth: 820 }}>
          <TableHead>
            <TableRow>
              <TableCell>Role</TableCell>
              <TableCell>System</TableCell>
              <TableCell>Permissions</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {roles.length === 0 ? (
              <TableRow>
                <TableCell colSpan={4}>
                  <Typography color="text.secondary">No roles loaded.</Typography>
                </TableCell>
              </TableRow>
            ) : (
              roles.map((role) => (
                <TableRow key={role.id}>
                  <TableCell>
                    <Typography sx={{ fontWeight: 800 }}>{role.displayName}</Typography>
                    <Typography color="text.secondary" variant="caption">{role.name}</Typography>
                  </TableCell>
                  <TableCell>{role.systemLabel}</TableCell>
                  <TableCell>
                    {role.permissions.length === 0 ? 'No permissions' : role.permissions.join(', ')}
                  </TableCell>
                  <TableCell align="right">
                    <Button
                      aria-label={`Edit ${role.displayName}`}
                      onClick={() => openEditRole(role)}
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
