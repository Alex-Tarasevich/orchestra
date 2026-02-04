
import { Integration, IntegrationType } from '../types';
import { getToken } from './authService';
import { MOCK_INTEGRATIONS } from '../constants';

const API_BASE_URL = `${import.meta.env.VITE_API_URL}/v1/integrations`;

export interface IntegrationDTO {
  name: string;
  type: IntegrationType;
  provider: string;
  url: string;
  username?: string;
  apiKey: string;
  filterQuery?: string;
  vectorize?: boolean;
}

export interface CreateIntegrationDTO extends IntegrationDTO {
  workspaceId: string;
}

const getAuthHeaders = () => ({
  'Content-Type': 'application/json',
  'Authorization': `Bearer ${getToken() || ''}`
});

export const getIntegrations = async (workspaceId: string): Promise<Integration[]> => {
  try {
    const response = await fetch(`${API_BASE_URL}?workspaceId=${workspaceId}`, {
        headers: getAuthHeaders()
    });
    if (!response.ok) throw new Error("API Error");
    return await response.json();
  } catch (error) {
    return new Promise((resolve) => {
        setTimeout(() => {
            resolve(MOCK_INTEGRATIONS.filter(i => i.workspaceId === workspaceId));
        }, 300);
    });
  }
};

export const createIntegration = async (data: CreateIntegrationDTO): Promise<Integration> => {
  try {
    const response = await fetch(API_BASE_URL, {
      method: 'POST',
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
        resolve({
          id: Math.random().toString(36).substr(2, 9),
          workspaceId: data.workspaceId, 
          name: data.name,
          type: data.type,
          icon: data.provider,
          url: data.url,
          provider: data.provider,
          username: data.username,
          connected: true,
          lastSync: 'Just now',
          filterQuery: data.filterQuery,
          vectorize: data.vectorize
        });
      }, 500);
    });
  }
};

export const updateIntegration = async (id: string, data: IntegrationDTO): Promise<Integration> => {
  try {
    const response = await fetch(`${API_BASE_URL}/${id}`, {
      method: 'PUT',
      headers: getAuthHeaders(),
      body: JSON.stringify(data),
    });

    if (!response.ok) {
      throw new Error(`Backend error: ${response.statusText}`);
    }

    return await response.json();
  } catch (error) {
    return new Promise((resolve) => {
        setTimeout(() => {
          resolve({
            id: id,
            workspaceId: 'ws-1',
            name: data.name,
            type: data.type,
            icon: data.provider,
            url: data.url,
            provider: data.provider,
            username: data.username,
            connected: true,
            lastSync: 'Just now',
            filterQuery: data.filterQuery,
            vectorize: data.vectorize
          } as Integration);
        }, 500);
      });
  }
};

export const deleteIntegration = async (id: string): Promise<void> => {
  try {
    const response = await fetch(`${API_BASE_URL}/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      throw new Error(`Backend error: ${response.statusText}`);
    }
  } catch (error) {
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve();
      }, 500);
    });
  }
};
