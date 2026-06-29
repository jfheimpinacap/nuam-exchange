export interface LoginRequestDto { email: string; password: string; }
export interface AuthenticatedUserDto { id: string; name: string; email: string; role: string; }
export interface LoginResponseDto { accessToken: string; expiresAt: string; user: AuthenticatedUserDto; }
export interface CurrentUserResponseDto { user: AuthenticatedUserDto; }
