import type { HttpClient } from '../client/HttpClient';
import type { CurrentUserResponseDto, LoginRequestDto, LoginResponseDto } from '../contracts/auth';
import type { AuthService } from './AuthService';

export class HttpAuthService implements AuthService {
  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequestDto): Promise<LoginResponseDto> {
    return this.http.post<LoginResponseDto>('/auth/login', request);
  }

  getCurrentUser(): Promise<CurrentUserResponseDto> {
    return this.http.get<CurrentUserResponseDto>('/auth/me');
  }
}
