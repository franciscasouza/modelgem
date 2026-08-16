import type {
  AuthUser,
  BootstrapResponse,
  CreateBlankPatternInput,
  CreateCustomerInput,
  CreateMeasurementSetInput,
  Customer,
  ExportJob,
  GeneratePatternInput,
  LoginInput,
  MeasurementSet,
  OverviewCounts,
  PatternDetail,
  PatternDocument,
  PatternSummary,
  RegisterInput,
  TechnicalSheet,
} from "./types";

const API_BASE =
  process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") ||
  "http://localhost:5074";

/** Dev seed tenant from API (DesignService / ADR kickoff). */
export const DEV_TENANT_ID = "11111111-1111-1111-1111-111111111111";

/** @deprecated Prefer tenant from `/me`. Kept only for Dev fallback / cleanup. */
const TENANT_STORAGE_KEY = "mf_tenant_id";

/** Tenant da sessão autenticada (preenchido por AuthProvider / me). */
let sessionTenantId: string | null = null;

export class ApiError extends Error {
  status: number;
  body: unknown;

  constructor(message: string, status: number, body?: unknown) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.body = body;
  }
}

function isBrowser(): boolean {
  return typeof window !== "undefined";
}

export function getSessionTenantId(): string | null {
  return sessionTenantId;
}

export function setSessionTenantId(tenantId: string | null): void {
  sessionTenantId = tenantId;
  if (tenantId && isBrowser()) {
    // Remove legado: fluxo autenticado não depende mais de mf_tenant_id.
    localStorage.removeItem(TENANT_STORAGE_KEY);
  }
}

/** @deprecated Use sessão (`/me`). */
export function getStoredTenantId(): string | null {
  if (!isBrowser()) return null;
  return localStorage.getItem(TENANT_STORAGE_KEY);
}

/** @deprecated Use sessão (`/me`). */
export function setStoredTenantId(tenantId: string): void {
  if (!isBrowser()) return;
  localStorage.setItem(TENANT_STORAGE_KEY, tenantId);
}

/**
 * Resolve tenant id for API calls.
 * Preferência: sessão autenticada (`setSessionTenantId` / GET `/api/v1/auth/me`).
 * Fallback Dev (se `/me` falhar): POST `/api/v1/dev/bootstrap` →
 * `NEXT_PUBLIC_TENANT_ID` → `DEV_TENANT_ID`. Não usar localStorage no fluxo autenticado.
 */
export async function resolveTenantId(): Promise<string> {
  if (sessionTenantId) return sessionTenantId;

  try {
    const me = await request<AuthUser>("/api/v1/auth/me");
    if (me?.tenantId) {
      setSessionTenantId(me.tenantId);
      return me.tenantId;
    }
  } catch {
    // Preferir auth; em Dev ainda há bootstrap/env abaixo.
  }

  try {
    const res = await fetch(`${API_BASE}/api/v1/dev/bootstrap`, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json", Accept: "application/json" },
      body: JSON.stringify({}),
    });
    if (res.ok) {
      const data = (await res.json()) as BootstrapResponse;
      if (data.tenantId) {
        sessionTenantId = data.tenantId;
        return data.tenantId;
      }
    }
  } catch {
    // API offline ou bootstrap indisponível
  }

  return process.env.NEXT_PUBLIC_TENANT_ID || DEV_TENANT_ID;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers);
  if (!headers.has("Accept")) headers.set("Accept", "application/json");
  if (init?.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  let res: Response;
  try {
    res = await fetch(`${API_BASE}${path}`, {
      ...init,
      credentials: "include",
      headers,
    });
  } catch (err) {
    if (err instanceof TypeError) {
      throw new ApiError(
        "Não foi possível conectar à API. Verifique se ela está em http://localhost:5074 e se CORS permite credentials.",
        0,
      );
    }
    throw err;
  }

  if (res.status === 204) return undefined as T;

  const text = await res.text();
  let body: unknown = null;
  if (text) {
    try {
      body = JSON.parse(text);
    } catch {
      body = text;
    }
  }

  if (!res.ok) {
    const message =
      typeof body === "object" &&
      body !== null &&
      "error" in body &&
      typeof (body as { error: unknown }).error === "string"
        ? (body as { error: string }).error
        : typeof body === "object" &&
            body !== null &&
            "title" in body &&
            typeof (body as { title: unknown }).title === "string"
          ? (body as { title: string }).title
          : typeof body === "object" &&
              body !== null &&
              "message" in body &&
              typeof (body as { message: unknown }).message === "string"
            ? (body as { message: string }).message
            : `Erro HTTP ${res.status}`;
    throw new ApiError(message, res.status, body);
  }

  return body as T;
}

async function withTenant<T>(fn: (tenantId: string) => Promise<T>): Promise<T> {
  const tenantId = await resolveTenantId();
  return fn(tenantId);
}

/* —— Response adapters (API camelCase DTOs → UI types) —— */

type ApiPatternSummary = {
  id: string;
  tenantId: string;
  customerId?: string | null;
  name: string;
  referenceCode?: string;
  baseKind?: string;
  status?: string;
  createdAt: string;
  updatedAt?: string;
};

type ApiVersionSummary = {
  id: string;
  patternModelId: string;
  version: number;
  hasGeometry: boolean;
  createdAt: string;
  createdByUserId?: string | null;
};

function mapSummary(raw: ApiPatternSummary): PatternSummary {
  return {
    id: raw.id,
    tenantId: raw.tenantId,
    name: raw.name,
    reference: raw.referenceCode ?? null,
    baseId: raw.baseKind ?? "blank",
    customerId: raw.customerId ?? null,
    createdAt: raw.createdAt,
    updatedAt: raw.updatedAt,
    status: raw.status ?? null,
  };
}

function mapDocument(geometry: unknown): PatternDocument | null {
  if (!geometry || typeof geometry !== "object") return null;
  const g = geometry as PatternDocument;
  if (!Array.isArray(g.pieces)) return null;
  return g;
}

function parametersToGenerateBody(
  parameters: Record<string, number>,
  measurementSetId?: string | null,
) {
  return {
    measurementSetId: measurementSetId || undefined,
    bustCircCm: parameters.bust_circ,
    waistCircCm: parameters.waist_circ,
    hipCircCm: parameters.hip_circ,
    skirtLengthCm: parameters.skirt_length,
    dressLengthCm: parameters.dress_length,
    easeBustCm: parameters.ease_bust,
    easeWaistCm: parameters.ease_waist,
    easeHipCm: parameters.ease_hip,
    waistToHipCm: parameters.waist_to_hip,
    shoulderToBustCm: parameters.shoulder_to_bust,
    bustToWaistCm: parameters.bust_to_waist,
    seamAllowanceCm: parameters.seam_allowance,
    hemAllowanceCm: parameters.hem_allowance,
    waistbandHeightCm: parameters.waistband_height,
  };
}

function mapAuthUser(raw: AuthUser): AuthUser {
  return {
    userId: raw.userId,
    email: raw.email,
    displayName: raw.displayName ?? null,
    tenantId: raw.tenantId,
    organizationName: raw.organizationName,
    role: raw.role,
  };
}

async function loadPatternDetail(
  tid: string,
  patternId: string,
): Promise<PatternDetail> {
  const payload = await request<{
    pattern: ApiPatternSummary;
    latestVersion: ApiVersionSummary | null;
  }>(`/api/v1/tenants/${tid}/patterns/${patternId}`);

  const summary = mapSummary(payload.pattern);
  let document: PatternDocument | null = null;
  let qualityIssues: string[] = [];
  let version = payload.latestVersion?.version;

  if (payload.latestVersion?.hasGeometry) {
    const ver = await request<{
      summary: ApiVersionSummary;
      parametersJson: string;
      geometry: unknown;
      qualityIssues: string[];
    }>(
      `/api/v1/tenants/${tid}/patterns/${patternId}/versions/${payload.latestVersion.id}`,
    );
    document = mapDocument(ver.geometry);
    qualityIssues = ver.qualityIssues ?? [];
    version = ver.summary.version;

    if (
      document &&
      (!document.resolvedParametersCm ||
        !Object.keys(document.resolvedParametersCm).length)
    ) {
      try {
        const parsed = JSON.parse(ver.parametersJson) as Record<string, number>;
        document = { ...document, resolvedParametersCm: parsed };
      } catch {
        /* keep geometry as-is */
      }
    }
  }

  return {
    ...summary,
    version,
    document,
    qualityIssues,
  };
}

export const api = {
  baseUrl: API_BASE,

  async register(input: RegisterInput): Promise<AuthUser | void> {
    const body = await request<AuthUser | Record<string, never> | undefined>(
      "/api/v1/auth/register",
      {
        method: "POST",
        body: JSON.stringify({
          organizationName: input.organizationName,
          email: input.email,
          displayName: input.displayName || undefined,
          password: input.password,
        }),
      },
    );
    if (body && typeof body === "object" && "userId" in body && "tenantId" in body) {
      const user = mapAuthUser(body as AuthUser);
      setSessionTenantId(user.tenantId);
      return user;
    }
  },

  async login(input: LoginInput): Promise<AuthUser | void> {
    const body = await request<AuthUser | Record<string, never> | undefined>(
      "/api/v1/auth/login",
      {
        method: "POST",
        body: JSON.stringify({
          email: input.email,
          password: input.password,
        }),
      },
    );
    if (body && typeof body === "object" && "userId" in body && "tenantId" in body) {
      const user = mapAuthUser(body as AuthUser);
      setSessionTenantId(user.tenantId);
      return user;
    }
  },

  async logout(): Promise<void> {
    try {
      await request<void>("/api/v1/auth/logout", { method: "POST" });
    } finally {
      setSessionTenantId(null);
    }
  },

  async me(): Promise<AuthUser> {
    const raw = await request<AuthUser>("/api/v1/auth/me");
    const user = mapAuthUser(raw);
    setSessionTenantId(user.tenantId);
    return user;
  },

  async listCustomers(): Promise<Customer[]> {
    return withTenant((tid) =>
      request<Customer[]>(`/api/v1/tenants/${tid}/customers`),
    );
  },

  async getCustomer(customerId: string): Promise<Customer | null> {
    const list = await this.listCustomers();
    return list.find((c) => c.id === customerId) ?? null;
  },

  async createCustomer(input: CreateCustomerInput): Promise<Customer> {
    return withTenant((tid) =>
      request<Customer>(`/api/v1/tenants/${tid}/customers`, {
        method: "POST",
        body: JSON.stringify(input),
      }),
    );
  },

  async listMeasurementSets(customerId: string): Promise<MeasurementSet[]> {
    return withTenant((tid) =>
      request<MeasurementSet[]>(
        `/api/v1/tenants/${tid}/customers/${customerId}/measurement-sets`,
      ),
    );
  },

  async createMeasurementSet(
    customerId: string,
    input: CreateMeasurementSetInput,
  ): Promise<MeasurementSet> {
    return withTenant((tid) =>
      request<MeasurementSet>(
        `/api/v1/tenants/${tid}/customers/${customerId}/measurement-sets`,
        { method: "POST", body: JSON.stringify(input) },
      ),
    );
  },

  async getOverview(): Promise<OverviewCounts> {
    return withTenant(async (tid) => {
      const raw = await request<{
        customerCount?: number;
        patternCount?: number;
        customersCount?: number;
        patternsCount?: number;
      }>(`/api/v1/tenants/${tid}/overview`);
      return {
        customersCount: raw.customersCount ?? raw.customerCount ?? 0,
        patternsCount: raw.patternsCount ?? raw.patternCount ?? 0,
        pendingApprovalsCount: null,
      };
    });
  },

  async listPatterns(): Promise<PatternSummary[]> {
    return withTenant(async (tid) => {
      const list = await request<ApiPatternSummary[]>(
        `/api/v1/tenants/${tid}/patterns`,
      );
      return (Array.isArray(list) ? list : []).map(mapSummary);
    });
  },

  async getPattern(patternId: string): Promise<PatternDetail> {
    return withTenant((tid) => loadPatternDetail(tid, patternId));
  },

  async createBlankPattern(
    input: CreateBlankPatternInput = {},
  ): Promise<PatternDetail> {
    return withTenant(async (tid) => {
      const created = await request<ApiPatternSummary>(
        `/api/v1/tenants/${tid}/patterns`,
        {
          method: "POST",
          body: JSON.stringify({
            name: input.name ?? "Tela em branco",
            baseKind: "blank",
          }),
        },
      );
      return loadPatternDetail(tid, created.id);
    });
  },

  async generatePattern(input: GeneratePatternInput): Promise<PatternDetail> {
    return withTenant(async (tid) => {
      const created = await request<ApiPatternSummary>(
        `/api/v1/tenants/${tid}/patterns`,
        {
          method: "POST",
          body: JSON.stringify({
            name: input.name,
            baseKind: input.baseId,
            customerId: input.customerId || undefined,
          }),
        },
      );

      await request(`/api/v1/tenants/${tid}/patterns/${created.id}/generate`, {
        method: "POST",
        body: JSON.stringify(
          parametersToGenerateBody(input.parameters, input.measurementSetId),
        ),
      });

      return loadPatternDetail(tid, created.id);
    });
  },

  async regeneratePattern(
    patternId: string,
    parameters: Record<string, number>,
  ): Promise<PatternDetail> {
    return withTenant(async (tid) => {
      await request(`/api/v1/tenants/${tid}/patterns/${patternId}/generate`, {
        method: "POST",
        body: JSON.stringify(parametersToGenerateBody(parameters)),
      });
      return loadPatternDetail(tid, patternId);
    });
  },

  async listPatternVersions(patternId: string): Promise<ApiVersionSummary[]> {
    return withTenant((tid) =>
      request<ApiVersionSummary[]>(
        `/api/v1/tenants/${tid}/patterns/${patternId}/versions`,
      ),
    );
  },

  async getTechnicalSheet(patternId: string): Promise<TechnicalSheet> {
    return withTenant(async (tid) => {
      const raw = await request<{
        id: string;
        patternModelId: string;
        materialsNotes?: string | null;
        constructionNotes?: string | null;
        updatedAt?: string;
      }>(`/api/v1/tenants/${tid}/patterns/${patternId}/technical-sheet`);
      return {
        patternId: raw.patternModelId,
        materialsNotes: raw.materialsNotes ?? "",
        constructionNotes: raw.constructionNotes ?? "",
        updatedAt: raw.updatedAt,
      };
    });
  },

  async updateTechnicalSheet(
    patternId: string,
    patch: { materialsNotes?: string; constructionNotes?: string },
  ): Promise<TechnicalSheet> {
    return withTenant(async (tid) => {
      const raw = await request<{
        patternModelId: string;
        materialsNotes?: string | null;
        constructionNotes?: string | null;
        updatedAt?: string;
      }>(`/api/v1/tenants/${tid}/patterns/${patternId}/technical-sheet`, {
        method: "PUT",
        body: JSON.stringify(patch),
      });
      return {
        patternId: raw.patternModelId,
        materialsNotes: raw.materialsNotes ?? "",
        constructionNotes: raw.constructionNotes ?? "",
        updatedAt: raw.updatedAt,
      };
    });
  },

  async startExport(patternId: string): Promise<ExportJob> {
    return withTenant(async (tid) => {
      const raw = await request<{
        id: string;
        patternModelId: string;
        status: string;
        format?: string;
        downloadUrl?: string | null;
        errorMessage?: string | null;
        createdAt?: string;
        completedAt?: string | null;
      }>(`/api/v1/tenants/${tid}/patterns/${patternId}/exports`, {
        method: "POST",
        body: JSON.stringify({}),
      });
      return mapExportJob(raw);
    });
  },

  async getExportJob(_patternId: string, jobId: string): Promise<ExportJob> {
    return withTenant(async (tid) => {
      const raw = await request<{
        id: string;
        patternModelId: string;
        status: string;
        format?: string;
        downloadUrl?: string | null;
        errorMessage?: string | null;
        createdAt?: string;
        completedAt?: string | null;
      }>(`/api/v1/tenants/${tid}/exports/${jobId}`);
      return mapExportJob(raw);
    });
  },

  exportDownloadUrl(_patternId: string, jobId: string): string {
    const tid =
      sessionTenantId ??
      process.env.NEXT_PUBLIC_TENANT_ID ??
      DEV_TENANT_ID;
    return `${API_BASE}/api/v1/tenants/${tid}/exports/${jobId}/download`;
  },
};

function mapExportJob(raw: {
  id: string;
  patternModelId: string;
  status: string;
  format?: string;
  downloadUrl?: string | null;
  errorMessage?: string | null;
  createdAt?: string;
  completedAt?: string | null;
}): ExportJob {
  const downloadUrl = raw.downloadUrl
    ? raw.downloadUrl.startsWith("http")
      ? raw.downloadUrl
      : `${API_BASE}${raw.downloadUrl}`
    : null;
  return {
    id: raw.id,
    patternId: raw.patternModelId,
    status: raw.status,
    format: raw.format,
    downloadUrl,
    error: raw.errorMessage ?? null,
    createdAt: raw.createdAt,
    completedAt: raw.completedAt,
  };
}

export function isNotFound(err: unknown): boolean {
  return err instanceof ApiError && err.status === 404;
}

export function formatApiError(err: unknown): string {
  if (err instanceof ApiError) {
    if (err.status === 404) {
      return "Endpoint ainda não disponível na API (404). Confirme se AuthN F1.7 está publicado.";
    }
    if (err.status === 401) {
      return "Sessão inválida ou expirada. Faça login novamente.";
    }
    if (err.status === 403) {
      return "Sem permissão para esta operação.";
    }
    if (err.status === 409) {
      return err.message || "Conflito: e-mail ou organização já cadastrados.";
    }
    if (err.status === 0) return err.message;
    if (err.status === 400 && err.body && typeof err.body === "object") {
      const details = (err.body as { details?: string[] }).details;
      if (Array.isArray(details) && details.length) {
        return `${err.message}: ${details.join("; ")}`;
      }
      const errors = (err.body as { errors?: Record<string, string[]> }).errors;
      if (errors && typeof errors === "object") {
        const msgs = Object.values(errors).flat();
        if (msgs.length) return msgs.join("; ");
      }
    }
    return err.message;
  }
  if (err instanceof TypeError) {
    return "Não foi possível conectar à API. Verifique se ela está em http://localhost:5074 e se CORS permite credentials.";
  }
  if (err instanceof Error) return err.message;
  return "Erro inesperado.";
}
