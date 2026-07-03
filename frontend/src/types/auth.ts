export type SessionUser = {
  id: string;
  email: string;
  displayName: string;
  roles: string[];
  permissions: string[];
};

export type LoginResponse = {
  accessToken: string;
  expiresAt: string;
  user: SessionUser;
};
