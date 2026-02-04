
import { Workflow } from '../types';
import { MOCK_WORKFLOWS } from '../constants';
import { getToken } from './authService';

const API_BASE_URL = `${import.meta.env.VITE_API_URL}/v1/workflows`;

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${getToken() || ''}`
});

export const getWorkspacesWorkflows = async (workspaceId: string): Promise<Workflow[]> => {
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
    // Mock fallback
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve(MOCK_WORKFLOWS.filter(w => w.workspaceId === workspaceId));
      }, 300);
    });
  }
};

export const createWorkflow = async (workspaceId: string, data: { name: string; nodes: any[]; edges: any[] }): Promise<Workflow> => {
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
    // Mock fallback
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve({
          id: `wf-${Date.now()}`,
          workspaceId,
          name: data.name,
          nodes: data.nodes,
          edges: data.edges
        });
      }, 500);
    });
  }
};

export const updateWorkflow = async (id: string, data: Partial<Workflow>): Promise<Workflow> => {
  try {
    const response = await fetch(`${API_BASE_URL}/${id}`, {
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
    // Mock fallback
    return new Promise((resolve) => {
      setTimeout(() => {
        const mockWorkflow = MOCK_WORKFLOWS.find(w => w.id === id);
        resolve({
          ...(mockWorkflow || { id, workspaceId: 'ws-1', name: '', nodes: [], edges: [] }),
          ...data
        });
      }, 500);
    });
  }
};

export const deleteWorkflow = async (id: string): Promise<void> => {
  try {
    const response = await fetch(`${API_BASE_URL}/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      throw new Error(`Backend error: ${response.statusText}`);
    }
  } catch (error) {
    // Mock fallback
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve();
      }, 500);
    });
  }
};
