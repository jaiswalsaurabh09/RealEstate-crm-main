import React, {
  createContext,
  useContext,
  useEffect,
  useState,
} from 'react';
import api from '../api/client';
import type { User } from '../types/api';

interface AuthContextType {
  user: User | null;
  token: string | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

function extractData<T>(response: any): T {
  return response?.data?.data ?? response?.data ?? response;
}

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(
    localStorage.getItem('crm_jwt_token')
  );

  const [user, setUser] = useState<User | null>(() => {
    const saved = localStorage.getItem('crm_user_data');

    try {
      return saved ? JSON.parse(saved) : null;
    } catch {
      return null;
    }
  });

  const [loading, setLoading] = useState(false);

  const login = async (email: string, password: string) => {
    setLoading(true);

    try {
      const response = await api.post('/auth/login', {
        email,
        password,
      });

      const payload = extractData<any>(response);

      const jwt =
        payload?.token ||
        payload?.accessToken ||
        response?.data?.token ||
        response?.data?.accessToken;

      const loggedInUser =
        payload?.user ||
        payload?.employee ||
        response?.data?.user ||
        response?.data?.employee;

      if (!jwt) {
        throw new Error('Login succeeded but no JWT token was returned.');
      }

      const normalizedUser: User = loggedInUser || {
        id: payload?.userId || '',
        name: payload?.name || email,
        email: payload?.email || email,
        role: payload?.role || 'Employee',
      };

      localStorage.setItem('crm_jwt_token', jwt);
      localStorage.setItem(
        'crm_user_data',
        JSON.stringify(normalizedUser)
      );

      setToken(jwt);
      setUser(normalizedUser);
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    localStorage.removeItem('crm_jwt_token');
    localStorage.removeItem('crm_user_data');
    setToken(null);
    setUser(null);
  };

  useEffect(() => {
    if (!token) {
      setUser(null);
    }
  }, [token]);

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        loading,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider');
  }

  return context;
}
