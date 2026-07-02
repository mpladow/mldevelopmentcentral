import {
  Avatar,
  Box,
  Divider,
  Drawer,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Tooltip,
  Typography,
} from '@mui/material';
import { LayoutDashboard, Server, ShieldCheck, Users } from 'lucide-react';
import type { SessionUser } from '../types/auth';
import type { ActiveView } from '../types/navigation';
import { getInitials } from '../utils/getInitials';

type SidebarProps = {
  activeView: ActiveView;
  isCollapsed: boolean;
  onNavigate: (view: ActiveView) => void;
  user: SessionUser;
};

const expandedWidth = 268;
const collapsedWidth = 78;

export function Sidebar({ activeView, isCollapsed, onNavigate, user }: SidebarProps) {
  const navItems = [
    { icon: Users, label: 'Accounts', view: 'accounts' },
    { icon: Server, label: 'Systems', view: 'systems' },
    { icon: ShieldCheck, label: 'Roles', view: 'roles' },
  ] satisfies Array<{ icon: typeof Users; label: string; view: ActiveView }>;

  return (
    <Drawer
      slotProps={{
        paper: {
          'aria-label': 'Main navigation',
          sx: {
            width: isCollapsed ? collapsedWidth : expandedWidth,
            overflowX: 'hidden',
            borderRight: '1px solid',
            borderColor: 'divider',
            bgcolor: '#111827',
            color: '#fff',
            transition: 'width 195ms cubic-bezier(0.4, 0, 0.2, 1)',
          },
        },
      }}
      sx={{
        flexShrink: 0,
        width: isCollapsed ? collapsedWidth : expandedWidth,
      }}
      variant="permanent"
    >
      <Box sx={{ display: 'grid', gap: 2, p: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, minHeight: 42 }}>
          <Avatar sx={{ bgcolor: 'primary.main', height: 38, width: 38 }}>
            <LayoutDashboard size={21} />
          </Avatar>
          {!isCollapsed && (
            <Typography component="strong" sx={{ fontWeight: 900, whiteSpace: 'nowrap' }}>
              ML Dashboard
            </Typography>
          )}
        </Box>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.25, minWidth: 0 }}>
          <Avatar sx={{ bgcolor: '#e0f2fe', color: '#0f172a', fontWeight: 900 }}>
            {getInitials(user.displayName || user.email)}
          </Avatar>
          {!isCollapsed && (
            <Box className="sidebar-profile-copy" sx={{ minWidth: 0 }}>
              <Typography className="sidebar-profile-name" sx={{ fontWeight: 800 }} noWrap>
                {user.displayName}
              </Typography>
              <Typography className="sidebar-profile-email" color="rgba(255,255,255,0.68)" variant="caption" noWrap>
                {user.email}
              </Typography>
            </Box>
          )}
        </Box>
      </Box>

      <Divider sx={{ borderColor: 'rgba(255,255,255,0.12)' }} />

      <List sx={{ px: 1.5, py: 2 }}>
        {!isCollapsed && (
          <Typography color="rgba(255,255,255,0.58)" sx={{ px: 1.5, pb: 1, fontWeight: 900 }} variant="caption">
            Global
          </Typography>
        )}
        {navItems.map((item) => {
          const Icon = item.icon;
          const selected = activeView === item.view;

          return (
            <Tooltip disableHoverListener={!isCollapsed} key={item.view} placement="right" title={item.label}>
              <ListItemButton
                onClick={() => onNavigate(item.view)}
                selected={selected}
                sx={{
                  borderRadius: 1,
                  color: selected ? '#fff' : 'rgba(255,255,255,0.72)',
                  justifyContent: isCollapsed ? 'center' : 'flex-start',
                  minHeight: 44,
                  px: isCollapsed ? 1 : 1.5,
                  '&.Mui-selected': {
                    bgcolor: 'primary.main',
                  },
                  '&.Mui-selected:hover': {
                    bgcolor: 'primary.dark',
                  },
                }}
              >
                <ListItemIcon sx={{ color: 'inherit', minWidth: isCollapsed ? 0 : 38 }}>
                  <Icon size={19} />
                </ListItemIcon>
                {!isCollapsed && (
                  <ListItemText
                    primary={<Typography component="span" sx={{ fontWeight: 800 }}>{item.label}</Typography>}
                  />
                )}
              </ListItemButton>
            </Tooltip>
          );
        })}
      </List>
    </Drawer>
  );
}
