export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface LoginUserDto {
  id: number;
  fullName: string;
  email: string;
  role: string;
}

export interface LoginResponseDto {
  accessToken: string;
  tokenType: string;
  expiresAt: string;
  user: LoginUserDto;
}

export interface CurrentUserResponseDto {
  id: number;
  fullName: string;
  email: string;
  jobTitle?: string | null;
  role: string;
  isActive: boolean;
}

export interface PermissionsResponseDto {
  permissions: string[];
}
