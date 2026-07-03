export type Role = {
  id: number;
  systemId: number;
  systemKey: string;
  systemLabel: string;
  name: string;
  displayName: string;
  isProtected: boolean;
  permissions: string[];
};

export type PermissionCatalogItem = {
  id: number;
  systemId: number;
  systemKey: string;
  systemLabel: string;
  permissionKey: string;
  displayName: string;
  category: string;
};
