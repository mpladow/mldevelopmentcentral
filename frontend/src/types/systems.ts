export type DashboardSystem = {
  id: number;
  systemKey: string;
  label: string;
  sortOrder: number;
  isActive: boolean;
  theme: SystemThemeSettings;
  accounts: SystemAccount[];
};

export type SystemAccount = {
  id: string;
  email: string;
  displayName: string;
  role: string;
};

export type AvailableSystem = {
  id: number;
  systemKey: string;
  label: string;
};

export type SystemThemeSettings = {
  primaryColor: string;
  secondaryColor: string;
  backgroundColor: string;
  surfaceColor: string;
  textColor: string;
  borderRadius: number;
};
