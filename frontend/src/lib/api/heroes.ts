// Heroes API wrapper for apiPost from client.ts
import { apiPost, type ApiResult } from './client';

// Backend wire contract - matching CreateHeroCommand record.
export type CreateHeroCommand = {
	name: string;
};

// Backend wire contract - matching CreateHeroResponse record.
export type CreateHeroResponse = {
	id: string;
	name: string;
	createdAt: string; // JSON has no Date type, using string instead on serialize
};

export const heroesApi = {
	create(command: CreateHeroCommand): Promise<ApiResult<CreateHeroResponse>> {
		return apiPost<CreateHeroResponse, CreateHeroCommand>('/api/heroes', command);
	}
};
