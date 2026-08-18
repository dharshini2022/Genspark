export interface UserProfileResponse {
  id: number;
  email: string;
  fullName: string;
  role: string;
  isActive: boolean;
}

export interface UserProfileRequest {
  fullName: string;
  email: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

