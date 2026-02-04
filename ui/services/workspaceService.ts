
import { Workspace } from '../types';
import { MOCK_WORKSPACES } from '../constants';
import { getToken, getUser } from './authService';

const API_BASE_URL = `${import.meta.env.VITE_API_URL}/v1/workspaces`;

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${getToken() || ''}`
});

export const getWorkspaces = async (): Promise<Workspace[]> => {
  try {
    const response = await fetch(API_BASE_URL, {
      headers: getAuthHeaders()
    });

    const contentType = response.headers.get("content-type");
    if (contentType && contentType.includes("text/html")) throw new Error("Not JSON");

    if (!response.ok) {
      throw new Error(`Backend error: ${response.statusText}`);
    }
    return await response.json();
  } catch (error) {
    // console.warn("Backend API not reachable. Falling back to mock data for workspaces.", error);
    return new Promise((resolve) => {
      setTimeout(() => {
        const user = getUser();
        if (user) {
          if (user.id === 'u-no-ws') {
            resolve([]);
            return;
          }
          if (user.id === 'u-ws-empty') {
            resolve([{ id: 'ws-empty', name: 'My First Workspace' }]);
            return;
          }
          if (user.id === 'u-full') {
             resolve(MOCK_WORKSPACES);
             return;
          }
        }
        // Default fallback
        resolve(MOCK_WORKSPACES);
      }, 300);
    });
  }
};

export const createWorkspace = async (name: string): Promise<Workspace> => {
  try {
    const response = await fetch(API_BASE_URL, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ name }),
    });

    const contentType = response.headers.get("content-type");
    if (contentType && contentType.includes("text/html")) throw new Error("Not JSON");

    if (!response.ok) {
      throw new Error(`Backend error: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    // console.warn("Backend API not reachable. Falling back to mock creation.", error);
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve({
          id: `ws-${Date.now()}`,
          name: name
        });
      }, 400);
    });
  }
};

export const updateWorkspace = async (id: string, name: string): Promise<Workspace> => {
  try {
    const response = await fetch(`${API_BASE_URL}/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify({ name }),
    });

    if (!response.ok) throw new Error(`Backend error: ${response.statusText}`);
    return await response.json();
  } catch (error) {
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve({ id, name });
      }, 400);
    });
  }
};

export const deleteWorkspace = async (id: string): Promise<void> => {
  try {
    const response = await fetch(`${API_BASE_URL}/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });

    if (!response.ok) throw new Error(`Backend error: ${response.statusText}`);
  } catch (error) {
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve();
      }, 400);
    });
  }
};
