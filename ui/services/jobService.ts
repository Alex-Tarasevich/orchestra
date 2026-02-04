
import { Job } from '../types';
import { getToken } from './authService';
import { MOCK_JOBS } from '../constants';

const API_BASE_URL = `${import.meta.env.VITE_API_URL}/v1/jobs`;

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${getToken() || ''}`
});

export const getJobs = async (workspaceId: string): Promise<Job[]> => {
  try {
    const response = await fetch(`${API_BASE_URL}?workspaceId=${workspaceId}`, {
      headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error("API Error");
    return await response.json();
  } catch (error) {
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve(MOCK_JOBS.filter(j => j.workspaceId === workspaceId));
      }, 300);
    });
  }
};

export const triggerSync = async (workspaceId: string, integrationId: string): Promise<Job> => {
  try {
    const response = await fetch(`${API_BASE_URL}/sync`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ workspaceId, integrationId }),
    });
    if (!response.ok) throw new Error("API Error");
    return await response.json();
  } catch (error) {
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve({
          id: `JOB-SYNC-${Math.random().toString(36).substr(2, 5).toUpperCase()}`,
          workspaceId,
          integrationId,
          status: 'IN_PROGRESS',
          progress: 10,
          type: 'SYNC',
          logs: [
            `[${new Date().toLocaleTimeString()}] Initializing connection to integration ${integrationId}...`,
            `[${new Date().toLocaleTimeString()}] Authenticating with provider...`,
            `[${new Date().toLocaleTimeString()}] Pulling remote delta...`
          ],
          startedAt: new Date().toISOString()
        });
      }, 500);
    });
  }
};

export const cancelJob = async (jobId: string): Promise<void> => {
    try {
        await fetch(`${API_BASE_URL}/${jobId}/cancel`, {
            method: 'POST',
            headers: getAuthHeaders(),
        });
    } catch (e) {
        return Promise.resolve();
    }
};
