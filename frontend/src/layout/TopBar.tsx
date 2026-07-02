import {
  AppBar,
  Avatar,
  Box,
  FormControl,
  IconButton,
  InputAdornment,
  NativeSelect,
  TextField,
  Toolbar,
  Tooltip,
} from '@mui/material';
import { useEffect, useState } from 'react';
import { Bell, CircleUserRound, LogOut, Menu, Search, Settings } from 'lucide-react';
import { apiBaseUrl } from '../config/api';
import { useAuthenticatedSession, useAuth } from '../features/auth/AuthContext';
import type { AvailableSystem } from '../types/systems';
import { getInitials } from '../utils/getInitials';

type TopBarProps = {
  isSidebarCollapsed: boolean;
  onToggleSidebar: () => void;
  systemListVersion: number;
};

export function TopBar({
  isSidebarCollapsed,
  onToggleSidebar,
  systemListVersion,
}: TopBarProps) {
  const { accessToken, user } = useAuthenticatedSession();
  const { logout } = useAuth();
  const [availableSystems, setAvailableSystems] = useState<AvailableSystem[]>([]);
  const [selectedSystemKey, setSelectedSystemKey] = useState('');

  useEffect(() => {
    async function loadAvailableSystems() {
      const response = await fetch(`${apiBaseUrl}/api/systems/available`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });

      if (!response.ok) {
        setAvailableSystems([]);
        setSelectedSystemKey('');
        return;
      }

      const systems = (await response.json()) as AvailableSystem[];
      setAvailableSystems(systems);
      setSelectedSystemKey((currentValue) => currentValue || systems[0]?.systemKey || '');
    }

    void loadAvailableSystems();
  }, [accessToken, systemListVersion]);

  return (
    <AppBar color="inherit" elevation={0} position="sticky" sx={{ borderBottom: '1px solid', borderColor: 'divider' }}>
      <Toolbar sx={{ gap: 1.5, minHeight: 72, px: { xs: 2, md: 3 } }}>
        <Tooltip title={isSidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}>
          <IconButton
            aria-expanded={!isSidebarCollapsed}
            aria-label={isSidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
            onClick={onToggleSidebar}
          >
            <Menu size={20} />
          </IconButton>
        </Tooltip>

        <TextField
          aria-label="Search"
          placeholder="Search"
          size="small"
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <Search size={17} />
                </InputAdornment>
              ),
            },
          }}
          sx={{
            display: { xs: 'none', sm: 'block' },
            maxWidth: 380,
            width: '32vw',
          }}
          type="search"
        />

        <Box sx={{ flex: 1 }} />

        <FormControl size="small" sx={{ minWidth: { xs: 132, sm: 180 } }}>
          <NativeSelect
            inputProps={{ 'aria-label': 'Select system' }}
            onChange={(event) => setSelectedSystemKey(event.target.value)}
            value={selectedSystemKey}
          >
            {availableSystems.length === 0 ? (
              <option value="">No systems</option>
            ) : (
              availableSystems.map((system) => (
                <option key={system.id} value={system.systemKey}>
                  {system.label}
                </option>
              ))
            )}
          </NativeSelect>
        </FormControl>

        <Tooltip title="Notifications">
          <IconButton aria-label="Notifications">
            <Bell size={18} />
          </IconButton>
        </Tooltip>
        <Tooltip title="Settings">
          <IconButton aria-label="Settings">
            <Settings size={18} />
          </IconButton>
        </Tooltip>
        <Avatar sx={{ display: { xs: 'none', sm: 'flex' }, bgcolor: '#eef2ff', color: 'primary.main', fontWeight: 900 }}>
          <CircleUserRound size={18} />
          <Box component="span" sx={{ ml: 0.5, fontSize: '0.78rem' }}>
            {getInitials(user.displayName || user.email)}
          </Box>
        </Avatar>
        <Tooltip title="Sign out">
          <IconButton aria-label="Sign out" onClick={logout}>
            <LogOut size={18} />
          </IconButton>
        </Tooltip>
      </Toolbar>
    </AppBar>
  );
}
