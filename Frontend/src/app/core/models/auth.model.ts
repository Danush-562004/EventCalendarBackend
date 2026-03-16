export interface LoginRequest {
    usernameOrEmail: string;
    password: string;
}

export interface RegisterRequest {
    username: string;
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    phoneNumber?: string;
}

export interface AuthResponse {
    token: string;
    expiry: string;
    user: UserResponse;
}

export interface UserResponse {
    id: number;
    username: string;
    email: string;
    firstName: string;
    lastName: string;
    fullName: string;
    phoneNumber?: string;
    profilePicture?: string;
    role: string;
    emailNotifications: boolean;
    pushNotifications: boolean;
    createdAt: string;
}
