export type DashboardSystem = {
  id: number;
  systemKey: string;
  label: string;
  sortOrder: number;
  isActive: boolean;
  accounts: SystemAccount[];
};

export type SystemAccount = {
  id: string;
  email: string;
  displayName: string;
};

export type AvailableSystem = {
  id: number;
  systemKey: string;
  label: string;
};
