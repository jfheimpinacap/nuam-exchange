import type { CurrentUserResponseDto, LoginRequestDto, LoginResponseDto } from '../contracts/auth';

export interface AuthService {
  login(request: LoginRequestDto): Promise<LoginResponseDto>;
  getCurrentUser(): Promise<CurrentUserResponseDto>;
}
