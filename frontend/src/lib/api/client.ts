// Wrapper for all API calls, returns ApiResult<T>

const API_BASE_URL = 'http://localhost:5132';

// Backend error return through ProblemDetails (RFC 7807)
export type ProblemDetails = {
	type?: string;
	title?: string;
	status?: number;
	detail?: string;
	instance?: string;
	errors?: Record<string, string[]>;  // FluentValidation responses
};

// Discriminated union for all API calls, resulting in either true or false.
export type ApiResult<T> =
	| { ok: true; data: T }
	| { ok: false; problem: ProblemDetails };

// Generic API request for features to use, e.g. CreateHeroResponse from heroes.ts
// No need for try/catch for API calls as none are thrown - Resolve all as { ok: false, problem }
export async function apiRequest<T>(
	path: string,
	init?: RequestInit
): Promise<ApiResult<T>> {
	try {
		const response = await fetch(`${API_BASE_URL}${path}`, {
			...init,
			headers: {
				'Content-Type': 'application/json',
				...(init?.headers ?? {})
			}
		});

		// Handling non-OK responses (4xx + 5xx errors).
		if (!response.ok) {
			let problem: ProblemDetails;
			try {
				problem = await response.json();
			} catch {
				// Throws if body is not valid JSON - Minimal ProblemDetails fallback.
				problem = {
					status: response.status,
					title: response.statusText || 'Request failed'
				};
			}
			return { ok: false, problem };
		}

		// 204 (No Content) special case - used for DELETE operations which returns success without a body.
		if (response.status === 204) {
			return { ok: true, data: undefined as T };
		}

		// Parsing JSON body from backend (2xx results) as T value.
		const data = (await response.json()) as T;
		return { ok: true, data };
	} catch (e) {
		// fetch() only throws on network-level failures (refused, DNS, CORS, abort).
		// 4xx/5xx aren't thrown by fetch — handled in the if (!response.ok) branch above.
		return {
			ok: false,
			problem: {
				title: e instanceof Error ? e.message : 'Network Error',  // Using instanceof to narrow down what was thrown.
				status: 0 // Assigning 0 as status code for network failures.
			}
		};
	}
}

// POST helper
export function apiPost<TResponse, TBody>(
	path: string,
	body: TBody
): Promise<ApiResult<TResponse>> {
	return apiRequest<TResponse>(path, {
		method: 'POST',
		body: JSON.stringify(body)
	});
}

// PUT helper
export function apiPut<TResponse, TBody>(
	path: string,
	body: TBody
): Promise<ApiResult<TResponse>> {
	return apiRequest<TResponse>(path, {
		method: 'PUT',
		body: JSON.stringify(body)
	});
}

// GET helper
export function apiGet<TResponse>(path: string): Promise<ApiResult<TResponse>> {
	return apiRequest<TResponse>(path, { method: 'GET' });
}
