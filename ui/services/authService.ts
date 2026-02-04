
import { User } from '../types';

interface AuthResponse {
  token: string;
  user: User;
}

// API base URL from Aspire service discovery (injected via Vite)
const API_BASE_URL = `${import.meta.env.VITE_API_URL}/v1/auth`;

const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

const getMockUser = (email: string, fullName: string = 'Demo User') => {
    let userId = 'u1';
    let userName = fullName;
    
    if (email === 'test@test.com') {
      userId = 'u-no-ws';
      userName = 'New User';
    } else if (email === 'test2@test.com') {
      userId = 'u-ws-empty';
      userName = 'Empty Workspace User';
    } else if (email === 'test3@test.com') {
      userId = 'u-full';
      userName = 'Power User';
    }
    
    return { id: userId, email, name: userName };
}

export const login = async (email: string, password: string): Promise<AuthResponse> => {
  try {
    // FAST PATH: If using test emails, skip network completely for instant demo
    if (email.includes('@test.com') || email.includes('demo')) {
       await delay(300); // Small artificial delay for realism
       throw new Error("Mock User Detected"); 
    }

    const response = await fetch(`${API_BASE_URL}/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });

    const contentType = response.headers.get("content-type");
    if (contentType && contentType.includes("text/html")) {
        throw new Error("API endpoint not found (returned HTML)");
    }

    if (!response.ok) {
        if (response.status === 404) throw new Error("API endpoint not found");
        if (response.status === 401) throw new Error('Invalid credentials');
        throw new Error('Login failed');
    }

    const data = await response.json();
    localStorage.setItem('nexus_token', data.token);
    localStorage.setItem('nexus_user', JSON.stringify(data.user));
    return data;
  } catch (error: any) {
    if (error.message === 'Invalid credentials') {
        throw error;
    }

    console.warn("Backend API not reachable or Mock User. Falling back to mock auth.");
    
    const mockToken = `ey_mock_jwt_${Date.now()}`;
    const mockUser = getMockUser(email);

    localStorage.setItem('nexus_token', mockToken);
    localStorage.setItem('nexus_user', JSON.stringify(mockUser));
    return { token: mockToken, user: mockUser as any };
  }
};

export const register = async (email: string, password: string, fullName: string): Promise<AuthResponse> => {
  try {
    // FAST PATH
    if (email.includes('@test.com')) {
        await delay(300);
        throw new Error("Mock User Detected");
    }

    const response = await fetch(`${API_BASE_URL}/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password, name: fullName }),
    });

    const contentType = response.headers.get("content-type");
    if (contentType && contentType.includes("text/html")) {
        throw new Error("API endpoint not found (returned HTML)");
    }

    if (!response.ok) {
        if (response.status === 404) throw new Error("API endpoint not found");
        if (response.status === 400) throw new Error('Registration failed: Invalid data');
        throw new Error('Registration failed');
    }

    const data = await response.json();
    localStorage.setItem('nexus_token', data.token);
    localStorage.setItem('nexus_user', JSON.stringify(data.user));
    return data;
  } catch (error: any) {
    if (error.message.startsWith('Registration failed')) {
        throw error;
    }

    console.warn("Backend API not reachable. Falling back to mock auth.");
    
    const mockToken = `ey_mock_jwt_${Date.now()}`;
    const mockUser = getMockUser(email, fullName);

    localStorage.setItem('nexus_token', mockToken);
    localStorage.setItem('nexus_user', JSON.stringify(mockUser));
    return { token: mockToken, user: mockUser as any };
  }
};

export const updateUser = async (data: { name: string, email: string }): Promise<User> => {
  try {
    const response = await fetch(`${API_BASE_URL}/profile`, {
      method: 'PATCH',
      headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${localStorage.getItem('nexus_token')}`
      },
      body: JSON.stringify(data),
    });

    if (!response.ok) throw new Error("Update failed");
    const updatedUser = await response.json();
    localStorage.setItem('nexus_user', JSON.stringify(updatedUser));
    return updatedUser;
  } catch (e) {
    // Mock Fallback
    await delay(500);
    const currentUser = getUser();
    const updatedUser = { ...currentUser, ...data };
    localStorage.setItem('nexus_user', JSON.stringify(updatedUser));
    return updatedUser;
  }
};

export const changePassword = async (currentPassword: string, newPassword: string): Promise<void> => {
    try {
        const response = await fetch(`${API_BASE_URL}/change-password`, {
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${localStorage.getItem('nexus_token')}`
            },
            body: JSON.stringify({ currentPassword, newPassword }),
        });

        if (!response.ok) {
            const err = await response.json();
            throw new Error(err.message || "Failed to change password");
        }
    } catch (e: any) {
        // Mock success for UI demonstration
        await delay(800);
        if (currentPassword === 'incorrect') {
            throw new Error("Current password does not match our records.");
        }
    }
};

export const logout = () => {
  localStorage.removeItem('nexus_token');
  localStorage.removeItem('nexus_user');
  localStorage.removeItem('nexus_active_view');
  localStorage.removeItem('nexus_active_workspace');
};

export const getToken = () => localStorage.getItem('nexus_token');

export const getUser = () => {
  const userStr = localStorage.getItem('nexus_user');
  return userStr ? JSON.parse(userStr) : null;
};
