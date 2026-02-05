
import { Workspace } from '../types';
import { getToken } from './authService';

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
    console.error('Failed to fetch workspaces:', error);
    return [];
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
    console.error('Failed to create workspace:', error);
    throw error;
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
    console.error('Failed to update workspace:', error);
    throw error;
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
    console.error('Failed to delete workspace:', error);
    throw error;
  }
};
