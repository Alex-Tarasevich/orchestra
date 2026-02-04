
import { Agent } from '../types';
import { MOCK_AGENTS } from '../constants';
import { getToken } from './authService';

const API_BASE_URL = `${import.meta.env.VITE_API_URL}/v1/agents`;

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${getToken() || ''}`
});

export const getAgents = async (workspaceId: string): Promise<Agent[]> => {
  try {
    const response = await fetch(`${API_BASE_URL}?workspaceId=${workspaceId}`, {
      headers: getAuthHeaders()
    });

    const contentType = response.headers.get("content-type");
    if (contentType && contentType.includes("text/html")) throw new Error("Not JSON");

    if (!response.ok) {
      throw new Error(`Backend error: ${response.statusText}`);
    }
    return await response.json();
  } catch (error) {
    // console.warn("Backend API not reachable. Falling back to mock data.", error);
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve(MOCK_AGENTS.filter(a => a.workspaceId === workspaceId));
      }, 300);
    });
  }
};

export const createAgent = async (workspaceId: string, data: { name: string; role: string; capabilities: string[]; toolActionIds: string[]; customInstructions?: string }): Promise<Agent> => {
  try {
    const response = await fetch(API_BASE_URL, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ workspaceId, ...data }),
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
          id: `a-${Date.now()}`,
          workspaceId,
          name: data.name,
          role: data.role,
          status: 'IDLE',
          capabilities: data.capabilities,
          toolActionIds: data.toolActionIds,
          customInstructions: data.customInstructions,
          avatarUrl: `https://picsum.photos/200/200?random=${Math.floor(Math.random() * 1000)}`
        });
      }, 500);
    });
  }
};

export const updateAgent = async (agentId: string, data: Partial<Agent>): Promise<Agent> => {
  try {
    const response = await fetch(`${API_BASE_URL}/${agentId}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });

    const contentType = response.headers.get("content-type");
    if (contentType && contentType.includes("text/html")) throw new Error("Not JSON");

    if (!response.ok) {
      throw new Error(`Backend error: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    return new Promise((resolve) => {
      setTimeout(() => {
        // Return a mock merged object (note: this is a partial mock, 
        // in real app the server returns the full object)
        resolve({
          id: agentId,
          workspaceId: 'ws-1', // Mock assumption
          name: 'Updated Agent', // Fallback defaults if not in data
          role: 'Updated Role',
          status: 'IDLE',
          capabilities: [],
          toolActionIds: [],
          avatarUrl: '',
          ...data
        } as Agent);
      }, 400);
    });
  }
};

export const deleteAgent = async (agentId: string): Promise<void> => {
  try {
    const response = await fetch(`${API_BASE_URL}/${agentId}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      throw new Error(`Backend error: ${response.statusText}`);
    }
  } catch (error) {
    // console.warn("Backend API not reachable. Falling back to mock implementation.", error);
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve();
      }, 500);
    });
  }
};
