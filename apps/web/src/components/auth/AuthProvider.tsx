"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react";
import {
  ApiError,
  api,
  formatApiError,
  setSessionTenantId,
} from "@/lib/api";
import type { AuthUser, LoginInput, RegisterInput } from "@/lib/types";

export type AuthStatus = "loading" | "authenticated" | "anonymous";

interface AuthContextValue {
  user: AuthUser | null;
  status: AuthStatus;
  sessionError: string | null;
  refresh: () => Promise<AuthUser | null>;
  login: (input: LoginInput) => Promise<void>;
  register: (input: RegisterInput) => Promise<AuthUser | null>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [status, setStatus] = useState<AuthStatus>("loading");
  const [sessionError, setSessionError] = useState<string | null>(null);

  const refresh = useCallback(async (): Promise<AuthUser | null> => {
    try {
      const me = await api.me();
      setUser(me);
      setSessionTenantId(me.tenantId);
      setStatus("authenticated");
      setSessionError(null);
      return me;
    } catch (err) {
      setUser(null);
      setSessionTenantId(null);
      setStatus("anonymous");
      if (err instanceof ApiError && err.status !== 401 && err.status !== 404) {
        setSessionError(formatApiError(err));
      } else {
        setSessionError(null);
      }
      return null;
    }
  }, []);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const login = useCallback(
    async (input: LoginInput) => {
      await api.login(input);
      const me = await refresh();
      if (!me) {
        throw new ApiError(
          "Login concluído, mas a sessão não foi confirmada (GET /auth/me). Verifique cookies e CORS com credentials.",
          401,
        );
      }
    },
    [refresh],
  );

  const register = useCallback(
    async (input: RegisterInput): Promise<AuthUser | null> => {
      const fromRegister = await api.register(input);
      if (fromRegister?.tenantId) {
        setUser(fromRegister);
        setSessionTenantId(fromRegister.tenantId);
        setStatus("authenticated");
        setSessionError(null);
        return fromRegister;
      }
      return refresh();
    },
    [refresh],
  );

  const logout = useCallback(async () => {
    try {
      await api.logout();
    } catch {
      // Limpa sessão local mesmo se a API falhar
    } finally {
      setUser(null);
      setSessionTenantId(null);
      setStatus("anonymous");
      setSessionError(null);
    }
  }, []);

  return (
    <AuthContext.Provider
      value={{
        user,
        status,
        sessionError,
        refresh,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth deve ser usado dentro de AuthProvider.");
  }
  return ctx;
}
